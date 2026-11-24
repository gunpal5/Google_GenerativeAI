using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// [Experimental] An open live interaction: the Interactions API over a WebSocket (python-genai
/// <c>InteractionWebSocketConnection</c>). Live Interactions support is experimental and may change in future versions.
/// </summary>
/// <remarks>
/// <para>Open one with <see cref="InteractionsClient.ConnectAsync(string, InteractionConnectOptions?, CancellationToken)"/>.
/// Send input with <see cref="SendAsync(string, CancellationToken)"/> and its overloads, and read server events with
/// <see cref="ReceiveAsync"/>.</para>
/// <para>Terminal server events (<c>interaction.completed</c>, <c>interaction.cancelled</c>,
/// <c>reconnection_required</c>), <see cref="CancelAsync"/> and fatal transport or protocol errors close the local
/// WebSocket. <see cref="DetachAsync"/> (and <see cref="DisposeAsync"/>) on an active connection sends
/// <c>interaction.disconnect</c> and closes the WebSocket without ending the server-side interaction, which can be
/// resumed with another connect. To end the interaction, call <see cref="CompleteAsync"/> and read events until
/// <c>interaction.completed</c>, or call <see cref="CancelAsync"/>.</para>
/// </remarks>
public sealed class InteractionWebSocketConnection : IAsyncDisposable, IDisposable
{
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);
    private const int AbnormalClosure = 1006;

    private readonly WebSocket _socket;
    private readonly JsonSerializerOptions _options;
    private readonly ILogger? _logger;
    private readonly Queue<InteractionSseEvent> _pending = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _sessionCts = new();
    private readonly byte[] _buffer = new byte[16 * 1024];

    private volatile bool _terminal;
    private volatile bool _detached;
    private volatile bool _connectionError;
    private int _socketClosed;
    private int _receiving;
    private int _disposed;

    /// <summary>
    /// Wraps an open WebSocket (normally created by <see cref="InteractionsClient.ConnectAsync(string, InteractionConnectOptions?, CancellationToken)"/>).
    /// The connection owns <paramref name="webSocket"/> and disposes it.
    /// </summary>
    /// <param name="webSocket">An open WebSocket to the live interactions endpoint, after the initial event was sent.</param>
    /// <param name="interactionId">The interaction ID, if known.</param>
    /// <param name="serializerOptions">JSON options; defaults to <see cref="InteractionWebSocketProtocol.SerializerOptions"/>.</param>
    /// <param name="logger">Optional logger.</param>
    public InteractionWebSocketConnection(WebSocket webSocket, string? interactionId = null,
        JsonSerializerOptions? serializerOptions = null, ILogger? logger = null)
    {
        _socket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        Id = interactionId;
        _options = serializerOptions ?? InteractionWebSocketProtocol.SerializerOptions;
        _logger = logger;
    }

    /// <summary>
    /// The interaction ID: the one connected to, or for a new interaction the ID reported by
    /// <c>interaction.created</c> once it has been received.
    /// </summary>
    public string? Id { get; private set; }

    /// <summary>Whether the interaction reached a terminal state (completed, cancelled or reconnection required).</summary>
    public bool IsTerminal => _terminal;

    /// <summary>Whether this connection was detached (or closed after a protocol error).</summary>
    public bool IsDetached => _detached;

    /// <summary>Sends text input (an <c>input</c> event with one text block).</summary>
    /// <param name="text">The text.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the frame has been sent.</returns>
    public Task SendAsync(string text, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(text);
#else
        if (text == null) throw new ArgumentNullException(nameof(text));
#endif
        return SendAsync(new InteractionClientEventBatch(InteractionInputEvent.FromText(text)), cancellationToken);
    }

    /// <summary>Sends one client event.</summary>
    /// <param name="clientEvent">The event.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the frame has been sent.</returns>
    public Task SendAsync(InteractionClientEvent clientEvent, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(clientEvent);
#else
        if (clientEvent == null) throw new ArgumentNullException(nameof(clientEvent));
#endif
        return SendAsync(new InteractionClientEventBatch(clientEvent), cancellationToken);
    }

    /// <summary>Sends several client events in one <c>{"events": [...]}</c> frame.</summary>
    /// <param name="events">The events (at least one).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the frame has been sent.</returns>
    public Task SendAsync(IEnumerable<InteractionClientEvent> events, CancellationToken cancellationToken = default)
    {
        return SendAsync(InteractionClientEventBatch.FromEvents(events), cancellationToken);
    }

    /// <summary>
    /// Sends a client payload: one event or an event list. Sending an <c>interaction.cancel</c> event makes the
    /// connection terminal and closes it, like <see cref="CancelAsync"/>.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the frame has been sent.</returns>
    /// <exception cref="InvalidOperationException">The interaction is terminal or the connection is detached.</exception>
    /// <exception cref="ArgumentException">The payload is an empty event list.</exception>
    /// <exception cref="InteractionWebSocketException">The connection is closed.</exception>
    public async Task SendAsync(InteractionClientEventBatch payload, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(payload);
#else
        if (payload == null) throw new ArgumentNullException(nameof(payload));
#endif
        EnsureActive();
        var json = InteractionWebSocketProtocol.SerializeClientPayload(payload, _options);
        await SendPayloadAsync(json, cancellationToken).ConfigureAwait(false);
        if (InteractionWebSocketProtocol.PayloadHasCancel(payload))
        {
            _terminal = true;
            await EndSessionAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reads server events (in wire order, with <c>{"events": [...]}</c> frames split) until the interaction becomes
    /// terminal or the connection is detached. Breaking out of the loop keeps the unread events of the current frame
    /// for the next call; only one receive loop may run at a time. <c>error</c> events are yielded, not thrown.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token. Cancelling a pending read aborts the WebSocket (a <see cref="WebSocket"/> restriction).
    /// </param>
    /// <returns>The server events.</returns>
    /// <exception cref="InteractionWebSocketException">
    /// The connection closed before a terminal event (<see cref="InteractionWebSocketException.CloseCode"/> is set), or
    /// a frame was invalid (the connection is then detached).
    /// </exception>
    public async IAsyncEnumerable<InteractionSseEvent> ReceiveAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!_detached)
        {
            if (Interlocked.Exchange(ref _receiving, 1) == 1)
                throw new InvalidOperationException("Only one receive iterator may be active at a time.");

            bool hasEvents;
            try
            {
                hasEvents = await FillPendingAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (InteractionWebSocketException ex) when (ex.CloseCode == null)
            {
                // Protocol error: the stream can no longer be trusted, so drop the connection.
                await CloseTransportAsync().ConfigureAwait(false);
                throw;
            }
            finally
            {
                Volatile.Write(ref _receiving, 0);
            }

            if (!hasEvents)
                yield break;

            InteractionSseEvent item;
            lock (_pending)
            {
                if (_pending.Count == 0)
                    continue;
                item = _pending.Dequeue();
            }

            if (Id == null && item is LiveInteractionCreatedEvent { InteractionId: { Length: > 0 } createdId })
                Id = createdId;

            var terminal = InteractionWebSocketProtocol.IsTerminalEventType(item.EventType);
            if (terminal)
            {
                _terminal = true;
                lock (_pending)
                    _pending.Clear();
                await CloseSocketAsync(graceful: true).ConfigureAwait(false);
            }

            yield return item;

            if (terminal || _detached)
                yield break;
        }
    }

    /// <summary>
    /// Requests completion (<c>interaction.complete</c>): no new input, but the server finishes its output. Keep
    /// reading <see cref="ReceiveAsync"/> until <c>interaction.completed</c>. Does nothing once terminal.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the frame has been sent.</returns>
    /// <exception cref="InvalidOperationException">The connection is detached.</exception>
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (_terminal)
            return;
        if (_detached)
            throw new InvalidOperationException("This live interaction is detached.");
        await SendPayloadAsync(Serialize(new CompleteInteractionEvent()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels the interaction on the server (<c>interaction.cancel</c>), aborting work in flight, and closes the
    /// connection. Does nothing once terminal.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the frame has been sent and the connection closed.</returns>
    /// <exception cref="InvalidOperationException">The connection is detached.</exception>
    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        if (_terminal)
            return;
        if (_detached)
            throw new InvalidOperationException("This live interaction is detached.");
        await SendPayloadAsync(Serialize(new CancelInteractionEvent()), cancellationToken).ConfigureAwait(false);
        _terminal = true;
        await EndSessionAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Detaches from the interaction without ending it: sends <c>interaction.disconnect</c> (unless the interaction is
    /// terminal or the connection failed), drops unread events, ends any running <see cref="ReceiveAsync"/> loop and
    /// closes the WebSocket. The interaction can be resumed with another connect. Never throws transport errors.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token for sending the disconnect frame.</param>
    /// <returns>A task that completes when the connection is closed.</returns>
    public async Task DetachAsync(CancellationToken cancellationToken = default)
    {
        if (_detached)
            return;
        var sendDisconnect = !_terminal && !_connectionError;
        _detached = true;
        lock (_pending)
            _pending.Clear();

        if (sendDisconnect)
        {
#pragma warning disable CA1031 // Detaching is best effort; the socket is closed below either way
            try
            {
                await SendPayloadAsync(Serialize(new InteractionDisconnectEvent()), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to send interaction.disconnect");
            }
#pragma warning restore CA1031
        }

        await EndSessionAsync().ConfigureAwait(false);
    }

    /// <summary>Detaches (see <see cref="DetachAsync"/>) and releases the WebSocket.</summary>
    /// <returns>A task that completes when the connection is disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Volatile.Read(ref _disposed) != 0)
            return;
        await DetachAsync().ConfigureAwait(false);
        Dispose();
    }

    /// <summary>
    /// Aborts the WebSocket and releases it without sending <c>interaction.disconnect</c>; prefer
    /// <see cref="DisposeAsync"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        _detached = true;
        CancelSession();
        if (Interlocked.Exchange(ref _socketClosed, 1) == 0)
            _socket.Abort();
        _socket.Dispose();
        _sessionCts.Dispose();
        _sendLock.Dispose();
    }

    private void EnsureActive()
    {
        if (_terminal)
            throw new InvalidOperationException("This live interaction is already terminal.");
        if (_detached)
            throw new InvalidOperationException("This live interaction is detached.");
    }

    private string Serialize(InteractionClientEvent clientEvent) =>
        InteractionWebSocketProtocol.SerializeClientPayload(new InteractionClientEventBatch(clientEvent), _options);

    /// <summary>Sends the initial event during setup; failures surface as <see cref="InteractionWebSocketException"/>.</summary>
    internal Task SendInitialEventAsync(InteractionClientEvent initialEvent, CancellationToken cancellationToken) =>
        SendPayloadAsync(Serialize(initialEvent), cancellationToken);

    private async Task SendPayloadAsync(string json, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (IsTransportError(ex) && !cancellationToken.IsCancellationRequested)
        {
            _connectionError = true;
            var code = GetCloseCode();
            await CloseSocketAsync(graceful: false).ConfigureAwait(false);
            throw ClosedError(code);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task<bool> FillPendingAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            lock (_pending)
            {
                if (_pending.Count > 0)
                    return true;
            }

            if (_terminal || _detached)
                return false;

            byte[]? frame;
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _sessionCts.Token))
            {
                try
                {
                    frame = await ReadFrameAsync(linked.Token).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Whatever ended the read, the session is over
                catch (Exception) when ((_terminal || _detached) && !cancellationToken.IsCancellationRequested)
                {
                    // The wait was ended by CancelAsync/DetachAsync (or a terminal send).
                    return false;
                }
#pragma warning restore CA1031
                catch (Exception ex) when (IsTransportError(ex) && !cancellationToken.IsCancellationRequested)
                {
                    frame = null;
                }
            }

            if (frame == null)
            {
                _connectionError = true;
                var code = GetCloseCode();
                await CloseSocketAsync(graceful: true).ConfigureAwait(false);
                if (_terminal || _detached)
                    return false;
                throw ClosedError(code);
            }

            if (_terminal || _detached)
                return false;

            var events = InteractionWebSocketProtocol.ParseServerFrame(frame, _options);
            lock (_pending)
            {
                foreach (var item in events)
                    _pending.Enqueue(item);
            }
        }
    }

    /// <summary>Reads one complete message; returns <c>null</c> when the peer sent a close frame.</summary>
    private async Task<byte[]?> ReadFrameAsync(CancellationToken cancellationToken)
    {
        using var message = new MemoryStream();
        while (true)
        {
            var result = await _socket.ReceiveAsync(new ArraySegment<byte>(_buffer), cancellationToken)
                .ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
                return null;
#pragma warning disable CA1849 // MemoryStream writes complete synchronously
            message.Write(_buffer, 0, result.Count);
#pragma warning restore CA1849
            if (result.EndOfMessage)
                return message.ToArray();
        }
    }

    private static bool IsTransportError(Exception ex) =>
        ex is WebSocketException or ObjectDisposedException or IOException or OperationCanceledException
            or InvalidOperationException;

    private int GetCloseCode()
    {
#pragma warning disable CA1031 // A disposed socket has no close status
        try
        {
            return _socket.CloseStatus is { } status ? (int)status : AbnormalClosure;
        }
        catch (Exception)
        {
            return AbnormalClosure;
        }
#pragma warning restore CA1031
    }

    private static InteractionWebSocketException ClosedError(int? closeCode) =>
        new(closeCode == null
            ? "The live interaction connection closed."
            : $"The live interaction connection closed with code {closeCode}.", closeCode);

    /// <summary>Ends a pending receive and closes the socket (terminal or detached state already set).</summary>
    private Task EndSessionAsync()
    {
        CancelSession();
        return CloseSocketAsync(graceful: true);
    }

    /// <summary>Marks the connection detached, drops unread events and closes the socket (after a protocol error).</summary>
    internal Task CloseTransportAsync()
    {
        _detached = true;
        lock (_pending)
            _pending.Clear();
        return EndSessionAsync();
    }

    private void CancelSession()
    {
#pragma warning disable CA1031 // Already disposed: nothing is waiting
        try
        {
            _sessionCts.Cancel();
        }
        catch (Exception)
        {
            // ignored
        }
#pragma warning restore CA1031
    }

    private async Task CloseSocketAsync(bool graceful)
    {
        if (Interlocked.Exchange(ref _socketClosed, 1) != 0)
            return;
#pragma warning disable CA1031 // Closing is best effort
        try
        {
            if (graceful && _socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                using var timeout = new CancellationTokenSource(CloseTimeout);
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, timeout.Token)
                    .ConfigureAwait(false);
            }
            else
            {
                _socket.Abort();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Failed to close the live interaction WebSocket");
        }
#pragma warning restore CA1031
    }
}
