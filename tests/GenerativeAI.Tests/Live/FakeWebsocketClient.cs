using System.Buffers;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Websocket.Client;

namespace GenerativeAI.Tests.Live;

/// <summary>
/// In-memory <see cref="IWebsocketClient"/> for unit tests. Captures text sent by the client and lets tests
/// push server messages through <see cref="MessageReceived"/>.
/// </summary>
internal sealed class FakeWebsocketClient : IWebsocketClient
{
    private readonly Subject<ResponseMessage> _messages = new();
    private readonly Subject<ReconnectionInfo> _reconnections = new();
    private readonly Subject<DisconnectionInfo> _disconnections = new();
    private readonly List<string> _sent = new();
    private readonly object _lock = new();

    public FakeWebsocketClient(string url)
    {
        Url = new Uri(url);
    }

    /// <summary>Raised synchronously whenever the client under test sends a text message.</summary>
    public event Action<string>? TextSent;

    /// <summary>Snapshot of the text messages sent so far.</summary>
    public IReadOnlyList<string> SentMessages
    {
        get
        {
            lock (_lock)
                return _sent.ToList();
        }
    }

    public bool IsDisposed { get; private set; }

    /// <summary>Pushes a text message as if it were received from the server.</summary>
    public void ReceiveText(string json) => _messages.OnNext(ResponseMessage.TextMessage(json));

    public Uri Url { get; set; }
    public IObservable<ResponseMessage> MessageReceived => _messages.AsObservable();
    public IObservable<ReconnectionInfo> ReconnectionHappened => _reconnections.AsObservable();
    public IObservable<DisconnectionInfo> DisconnectionHappened => _disconnections.AsObservable();
    public TimeSpan? ReconnectTimeout { get; set; }
    public TimeSpan? ErrorReconnectTimeout { get; set; }
    public TimeSpan? LostReconnectTimeout { get; set; }
    public bool IsReconnectionEnabled { get; set; }
    public string? Name { get; set; }
    public bool IsStarted { get; private set; }
    public bool IsRunning { get; private set; }
    public bool TextSenderRunning => IsRunning;
    public bool BinarySenderRunning => IsRunning;
    public bool IsInsideLock => false;
    public bool IsTextMessageConversionEnabled { get; set; } = true;
    public bool IsStreamDisposedAutomatically { get; set; } = true;
    public Encoding? MessageEncoding { get; set; } = Encoding.UTF8;
    public ClientWebSocket NativeClient => throw new NotSupportedException();
    public TimeSpan ConnectTimeout { get; set; }

    public Task Start()
    {
        IsStarted = true;
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StartOrFail() => Start();

    public Task<bool> Stop(WebSocketCloseStatus status, string statusDescription)
    {
        IsRunning = false;
        return Task.FromResult(true);
    }

    public Task<bool> StopOrFail(WebSocketCloseStatus status, string statusDescription) =>
        Stop(status, statusDescription);

    public bool Send(string message)
    {
        lock (_lock)
            _sent.Add(message);
        TextSent?.Invoke(message);
        return true;
    }

    public bool Send(byte[] message) => throw new NotSupportedException();
    public bool Send(ArraySegment<byte> message) => throw new NotSupportedException();
    public bool Send(ReadOnlySequence<byte> message) => throw new NotSupportedException();
    public Task SendInstant(string message) => Task.FromResult(Send(message));
    public Task SendInstant(byte[] message) => throw new NotSupportedException();
    public bool SendAsText(byte[] message) => throw new NotSupportedException();
    public bool SendAsText(ArraySegment<byte> message) => throw new NotSupportedException();
    public bool SendAsText(ReadOnlySequence<byte> message) => throw new NotSupportedException();
    public void StreamFakeMessage(ResponseMessage message) => _messages.OnNext(message);
    public Task Reconnect() => Task.CompletedTask;
    public Task ReconnectOrFail() => Task.CompletedTask;

    public void Dispose()
    {
        IsRunning = false;
        IsDisposed = true;
        _messages.Dispose();
        _reconnections.Dispose();
        _disconnections.Dispose();
    }
}
