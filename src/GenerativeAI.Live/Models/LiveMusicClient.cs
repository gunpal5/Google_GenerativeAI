using System.Globalization;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Core;
using GenerativeAI.Live.Events;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace GenerativeAI.Live;

/// <summary>
/// [Experimental] A client for real-time music generation with Lyria RealTime over the Gemini API Live Music
/// WebSocket endpoint (<c>BidiGenerateMusic</c>).
/// </summary>
/// <remarks>
/// Typical usage: <see cref="ConnectAsync"/>, then <see cref="SetWeightedPromptsAsync(IEnumerable{WeightedPrompt}, CancellationToken)"/>
/// and optionally <see cref="SetMusicGenerationConfigAsync"/>, then <see cref="PlayAsync"/>. Audio arrives through
/// <see cref="AudioChunkReceived"/> as raw 16-bit PCM (48 kHz stereo by default). Prompts and config can be changed
/// while playing to steer the music. Live Music is only available on the Gemini API (not Vertex AI).
/// </remarks>
/// <seealso href="https://ai.google.dev/gemini-api/docs/music-generation">See Official API Documentation</seealso>
public class LiveMusicClient : IDisposable
{
    #region Constants

    /// <summary>
    /// The default sample rate of Lyria RealTime audio output, in Hz.
    /// </summary>
    public const int DefaultSampleRate = 48000;

    /// <summary>
    /// The default number of channels of Lyria RealTime audio output.
    /// </summary>
    public const int DefaultChannels = 2;

    #endregion

    #region Private Fields

#pragma warning disable CA2213 // Disposed in DisconnectAsync, which Dispose(bool) calls
    private IWebsocketClient? _client;
#pragma warning restore CA2213
    private readonly List<IDisposable> _subscriptions = new();
    private readonly object _stateLock = new();
    private readonly ILogger? _logger;
    private readonly string _apiKey;
    private TaskCompletionSource<bool> _setupComplete = NewSetupCompletionSource();
    private List<WeightedPrompt>? _lastWeightedPrompts;
    private LiveMusicGenerationConfig? _lastConfig;
    private LiveMusicPlaybackControl? _lastPlaybackControl;
    private bool _disposed;

    private static readonly JsonTypeInfo<LiveMusicClientMessage> ClientMessageTypeInfo =
#if NET11_0_OR_GREATER
        DefaultSerializerOptions.Options.GetTypeInfo<LiveMusicClientMessage>();
#else
        (JsonTypeInfo<LiveMusicClientMessage>)DefaultSerializerOptions.Options.GetTypeInfo(typeof(LiveMusicClientMessage));
#endif

    private static readonly JsonTypeInfo<LiveMusicServerMessage> ServerMessageTypeInfo =
#if NET11_0_OR_GREATER
        DefaultSerializerOptions.Options.GetTypeInfo<LiveMusicServerMessage>();
#else
        (JsonTypeInfo<LiveMusicServerMessage>)DefaultSerializerOptions.Options.GetTypeInfo(typeof(LiveMusicServerMessage));
#endif

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveMusicClient"/> class.
    /// </summary>
    /// <param name="platformAdapter">The Gemini API platform adapter (a <see cref="GoogleAIPlatformAdapter"/>) that provides the API key.</param>
    /// <param name="modelName">The model to use. Defaults to <see cref="GoogleAIModels.Music.LyriaRealtimeExp"/>.</param>
    /// <param name="logger">An optional logger.</param>
    /// <exception cref="NotSupportedException">The platform adapter is not a Gemini API adapter (Live Music is not supported on Vertex AI).</exception>
    public LiveMusicClient(IPlatformAdapter platformAdapter, string modelName = GoogleAIModels.Music.LyriaRealtimeExp,
        ILogger? logger = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(platformAdapter);
        ArgumentNullException.ThrowIfNull(modelName);
#else
        if (platformAdapter == null) throw new ArgumentNullException(nameof(platformAdapter));
        if (modelName == null) throw new ArgumentNullException(nameof(modelName));
#endif
        if (platformAdapter is not GoogleAIPlatformAdapter googleAdapter)
            throw new NotSupportedException("Live music generation is only supported on the Gemini API, not on Vertex AI.");
        if (string.IsNullOrEmpty(googleAdapter.Credentials?.ApiKey))
            throw new ArgumentException("An API key is required for Live Music.", nameof(platformAdapter));

        _apiKey = googleAdapter.Credentials!.ApiKey!;
        ModelName = modelName.ToModelId();
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveMusicClient"/> class with a Gemini API key.
    /// </summary>
    /// <param name="apiKey">The Gemini API key. When null, the <c>GOOGLE_API_KEY</c> environment variable is used.</param>
    /// <param name="modelName">The model to use. Defaults to <see cref="GoogleAIModels.Music.LyriaRealtimeExp"/>.</param>
    /// <param name="logger">An optional logger.</param>
    public LiveMusicClient(string? apiKey, string modelName = GoogleAIModels.Music.LyriaRealtimeExp,
        ILogger? logger = null)
#pragma warning disable CA2000 // The adapter only holds credentials; it is not disposable state owned by this client
        : this(new GoogleAIPlatformAdapter(apiKey), modelName, logger)
#pragma warning restore CA2000
    {
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the WebSocket client used for the current connection, or null when not connected.
    /// </summary>
    public IWebsocketClient? Client => _client;

    /// <summary>
    /// Gets the fully qualified model name sent in the setup message (for example <c>models/lyria-realtime-exp</c>).
    /// </summary>
    public string ModelName { get; }

    /// <summary>
    /// Gets or sets the API version used in the WebSocket URL. Defaults to <c>v1alpha</c>.
    /// </summary>
    public string ApiVersion { get; set; } = ApiVersions.v1Alpha;

    /// <summary>
    /// Gets or sets how long <see cref="ConnectAsync"/> waits for the server's <c>setupComplete</c> message.
    /// Defaults to 30 seconds.
    /// </summary>
    public TimeSpan SetupTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets whether the server has acknowledged the setup message on the current connection.
    /// </summary>
    public bool IsSetupComplete => _setupComplete.Task.Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// Gets or sets whether, after the underlying WebSocket reconnects, the client re-sends the setup message and then
    /// restores the last weighted prompts, generation config and (if playing) the <c>PLAY</c> signal. Defaults to true.
    /// </summary>
    public bool RestoreStateOnReconnect { get; set; } = true;

    /// <summary>
    /// Optional factory that <see cref="ConnectAsync"/> uses to create the WebSocket client instead of the default
    /// reconnecting <see cref="WebsocketClient"/>. It receives the Live Music endpoint URL (which already contains the
    /// API key as the <c>key</c> query parameter) and must return a client that has not been started;
    /// <see cref="ConnectAsync"/> subscribes to it and then calls <see cref="IWebsocketClient.Start"/>.
    /// Useful for custom transports, proxies and unit testing.
    /// </summary>
    public Func<string, IWebsocketClient>? WebsocketClientFactory { get; set; }

    #endregion

    #region Events
#pragma warning disable CA1003
    /// <summary>
    /// Occurs after the WebSocket client is created, before the connection is started.
    /// </summary>
    public event EventHandler<ClientCreatedEventArgs>? ClientCreated;

    /// <summary>
    /// Occurs when the WebSocket connection is established (before setup is acknowledged).
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    /// Occurs when the server acknowledges the setup message (<c>setupComplete</c>).
    /// </summary>
    public event EventHandler? SetupCompleted;

    /// <summary>
    /// Occurs when the server sends an audio chunk. Raised once per chunk, in order.
    /// </summary>
    public event EventHandler<LiveMusicAudioChunkReceivedEventArgs>? AudioChunkReceived;

    /// <summary>
    /// Occurs when the server reports that a prompt was filtered.
    /// </summary>
    public event EventHandler<LiveMusicFilteredPromptEventArgs>? FilteredPromptReceived;

    /// <summary>
    /// Occurs for every message received from the server, after the specific events have been raised.
    /// </summary>
    public event EventHandler<LiveMusicMessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Occurs when the WebSocket connection is closed. When the server sent a close reason,
    /// the event args are an <see cref="ErrorMessageEventArgs"/> carrying it.
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// Occurs when an error happens while connecting, sending or processing messages
    /// (including exceptions thrown by event handlers).
    /// </summary>
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;
#pragma warning restore CA1003
    #endregion

    #region Connection

    /// <summary>
    /// Builds the Live Music WebSocket URL, including the API key.
    /// </summary>
    /// <returns>The WebSocket URL.</returns>
    public string GetLiveMusicUrl()
    {
#if NET6_0_OR_GREATER
        var baseUrl = BaseUrls.GoogleLiveMusic.Replace("{version}", ApiVersion, StringComparison.Ordinal);
#else
        var baseUrl = BaseUrls.GoogleLiveMusic.Replace("{version}", ApiVersion);
#endif
        return $"{baseUrl}?key={Uri.EscapeDataString(_apiKey)}";
    }

    /// <summary>
    /// Connects to the Live Music endpoint and sends the setup message with the model name.
    /// </summary>
    /// <param name="waitForSetupComplete">
    /// When true (the default), waits until the server acknowledges the setup with <c>setupComplete</c>
    /// (or <see cref="SetupTimeout"/> elapses) before returning, as the protocol requires before sending other messages.
    /// </param>
    /// <param name="cancellationToken">A token to cancel waiting for the setup acknowledgement.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">The server did not acknowledge the setup within <see cref="SetupTimeout"/>.</exception>
    public async Task ConnectAsync(bool waitForSetupComplete = true, CancellationToken cancellationToken = default)
    {
#if NET7_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed) throw new ObjectDisposedException(nameof(LiveMusicClient));
#endif
        // Connecting again replaces the previous websocket; release it so it and its subscriptions don't leak.
        ReleaseClient();
        lock (_stateLock)
        {
            _lastWeightedPrompts = null;
            _lastConfig = null;
            _lastPlaybackControl = null;
            _setupComplete = NewSetupCompletionSource();
        }

        var url = GetLiveMusicUrl();
        IWebsocketClient client;
        if (WebsocketClientFactory != null)
        {
            client = WebsocketClientFactory(url)
                     ?? throw new InvalidOperationException($"{nameof(WebsocketClientFactory)} returned null.");
        }
        else
        {
#pragma warning disable CA2000 // Owned by the WebsocketClient, which is disposed in ReleaseClient
            var socket = new ClientWebSocket { Options = { KeepAliveInterval = TimeSpan.FromSeconds(10) } };
            client = socket.WithReconnect(url);
#pragma warning restore CA2000
        }

        _client = client;
        ClientCreated?.Invoke(this, new ClientCreatedEventArgs(client));

        _subscriptions.Add(client.ReconnectionHappened.Subscribe(info =>
        {
            if (info.Type == ReconnectionType.Initial)
                return;
            if (_logger?.IsEnabled(LogLevel.Information) == true)
                _logger.LogInformation("Live Music reconnection happened: {ReconnectionType}", info.Type);
            // The server expects a new setup message on every new connection.
            _ = RestoreAfterReconnectAsync();
        }));

        _subscriptions.Add(client.MessageReceived.ObserveOn(TaskPoolScheduler.Default)
            .Subscribe(ProcessReceivedMessage, ex =>
            {
                _logger?.LogError(ex, "Error in Live Music message stream");
                RaiseError(ex);
            }));

        _subscriptions.Add(client.DisconnectionHappened.Subscribe(OnDisconnected));

        try
        {
            await client.Start().ConfigureAwait(false);
            Connected?.Invoke(this, EventArgs.Empty);
            await SendSetupAsync(cancellationToken).ConfigureAwait(false);
            if (waitForSetupComplete)
                await WaitForSetupCompleteAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to the Live Music API");
            RaiseError(ex);
            throw;
        }
    }

    /// <summary>
    /// Waits until the server acknowledges the setup message on the current connection.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>A task that completes when <c>setupComplete</c> has been received.</returns>
    /// <exception cref="TimeoutException">The server did not acknowledge the setup within <see cref="SetupTimeout"/>.</exception>
    public async Task WaitForSetupCompleteAsync(CancellationToken cancellationToken = default)
    {
        Task task;
        lock (_stateLock)
            task = _setupComplete.Task;
        try
        {
            await task.WaitAsync(SetupTimeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException(
                $"The Live Music server did not acknowledge the setup within {SetupTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds.", ex);
        }
    }

    /// <summary>
    /// Sends the setup message (<c>{"setup":{"model":"models/..."}}</c>). Called by <see cref="ConnectAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendSetupAsync(CancellationToken cancellationToken = default) =>
        SendAsync(new LiveMusicClientMessage { Setup = new LiveMusicClientSetup { Model = ModelName } },
            cancellationToken);

    /// <summary>
    /// Closes the WebSocket connection and releases its resources.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        var client = _client;
        if (client == null)
            return;
        try
        {
            await client.Stop(WebSocketCloseStatus.NormalClosure, "Client Disconnecting").ConfigureAwait(false);
        }
#pragma warning disable CA1031 // Disconnecting must not throw
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while disconnecting from the Live Music API");
            RaiseError(ex);
        }
#pragma warning restore CA1031
        finally
        {
            // Release subscriptions too, so a disposed socket's events can't reach this instance.
            ReleaseClient();
        }
    }

    #endregion

    #region Client Messages

    /// <summary>
    /// Sets the weighted prompts that steer the music (<c>{"clientContent":{"weightedPrompts":[...]}}</c>).
    /// Weights must not be 0; the server normalizes the weights of all prompts in the message.
    /// </summary>
    /// <param name="prompts">The weighted prompts.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetWeightedPromptsAsync(IEnumerable<WeightedPrompt> prompts,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(prompts);
#else
        if (prompts == null) throw new ArgumentNullException(nameof(prompts));
#endif
        var list = prompts.ToList();
        if (list.Count == 0)
            throw new ArgumentException("At least one weighted prompt is required.", nameof(prompts));
        if (list.Any(p => p == null))
            throw new ArgumentException("Weighted prompts cannot contain null entries.", nameof(prompts));

        await SendAsync(new LiveMusicClientMessage
        {
            ClientContent = new LiveMusicClientContent { WeightedPrompts = list }
        }, cancellationToken).ConfigureAwait(false);
        lock (_stateLock)
            _lastWeightedPrompts = list;
    }

    /// <summary>
    /// Sets a single text prompt with weight 1.0.
    /// </summary>
    /// <param name="text">The text prompt.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetWeightedPromptsAsync(string text, CancellationToken cancellationToken = default) =>
        SetWeightedPromptsAsync(new[] { new WeightedPrompt { Text = text, Weight = 1.0 } }, cancellationToken);

    /// <summary>
    /// Sets the music generation config (<c>{"musicGenerationConfig":{...}}</c>). Changes to <c>bpm</c> or
    /// <c>scale</c> take full effect after <see cref="ResetContextAsync"/> (or stop/play).
    /// </summary>
    /// <param name="config">The music generation config.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetMusicGenerationConfigAsync(LiveMusicGenerationConfig config,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config);
#else
        if (config == null) throw new ArgumentNullException(nameof(config));
#endif
        await SendAsync(new LiveMusicClientMessage { MusicGenerationConfig = config }, cancellationToken)
            .ConfigureAwait(false);
        lock (_stateLock)
            _lastConfig = config;
    }

    /// <summary>
    /// Starts (or resumes) music generation (<c>{"playbackControl":"PLAY"}</c>).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PlayAsync(CancellationToken cancellationToken = default) =>
        SendPlaybackControlAsync(LiveMusicPlaybackControl.PLAY, cancellationToken);

    /// <summary>
    /// Pauses music generation (<c>{"playbackControl":"PAUSE"}</c>). Use <see cref="PlayAsync"/> to resume.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task PauseAsync(CancellationToken cancellationToken = default) =>
        SendPlaybackControlAsync(LiveMusicPlaybackControl.PAUSE, cancellationToken);

    /// <summary>
    /// Stops music generation and resets the context while retaining prompts and config
    /// (<c>{"playbackControl":"STOP"}</c>). Use <see cref="PlayAsync"/> to restart.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken = default) =>
        SendPlaybackControlAsync(LiveMusicPlaybackControl.STOP, cancellationToken);

    /// <summary>
    /// Resets the generation context without stopping the music; prompts and config are retained
    /// (<c>{"playbackControl":"RESET_CONTEXT"}</c>).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ResetContextAsync(CancellationToken cancellationToken = default) =>
        SendPlaybackControlAsync(LiveMusicPlaybackControl.RESET_CONTEXT, cancellationToken);

    /// <summary>
    /// Sends a playback control signal (<c>{"playbackControl":"..."}</c>).
    /// </summary>
    /// <param name="playbackControl">The playback control signal.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendPlaybackControlAsync(LiveMusicPlaybackControl playbackControl,
        CancellationToken cancellationToken = default)
    {
        await SendAsync(new LiveMusicClientMessage { PlaybackControl = playbackControl }, cancellationToken)
            .ConfigureAwait(false);
        if (playbackControl != LiveMusicPlaybackControl.RESET_CONTEXT)
        {
            lock (_stateLock)
                _lastPlaybackControl = playbackControl;
        }
    }

    /// <summary>
    /// Sends a raw client message over the WebSocket.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">The client is not connected.</exception>
    public Task SendAsync(LiveMusicClientMessage message, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(message);
#else
        if (message == null) throw new ArgumentNullException(nameof(message));
#endif
        cancellationToken.ThrowIfCancellationRequested();

        var client = _client;
        if (client?.IsRunning != true)
        {
            var ex = new InvalidOperationException("The Live Music WebSocket client is not connected.");
            RaiseError(ex);
            throw ex;
        }

        var json = JsonSerializer.Serialize(message, ClientMessageTypeInfo);
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            _logger.LogDebug("Live Music message sent: {Message}", json);
        if (!client.Send(json))
        {
            var ex = new InvalidOperationException("The Live Music message could not be queued for sending.");
            RaiseError(ex);
            throw ex;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Receiving

    private void ProcessReceivedMessage(ResponseMessage msg)
    {
        // Runs on the Rx scheduler: any exception escaping here would tear down the subscription, so all are caught.
        try
        {
            LiveMusicServerMessage? message;
            if (msg.MessageType == WebSocketMessageType.Binary)
            {
                if (msg.Binary == null || msg.Binary.Length == 0)
                    return;
                message = JsonSerializer.Deserialize(msg.Binary, ServerMessageTypeInfo);
            }
            else
            {
                if (string.IsNullOrEmpty(msg.Text))
                    return;
                message = JsonSerializer.Deserialize(msg.Text, ServerMessageTypeInfo);
            }

            if (message == null)
                return;

            HandleServerMessage(message);
        }
#pragma warning disable CA1031 // Exceptions (including from user event handlers) are reported via ErrorOccurred
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing Live Music server message");
            RaiseError(ex);
        }
#pragma warning restore CA1031
    }

    private void HandleServerMessage(LiveMusicServerMessage message)
    {
        if (message.SetupComplete != null)
        {
            TaskCompletionSource<bool> tcs;
            lock (_stateLock)
                tcs = _setupComplete;
            tcs.TrySetResult(true);
            SetupCompleted?.Invoke(this, EventArgs.Empty);
        }

        if (message.ServerContent?.AudioChunks is { Count: > 0 } chunks)
        {
            foreach (var chunk in chunks)
            {
                if (chunk == null)
                    continue;
                var (sampleRate, channels) = ParseAudioFormat(chunk.MimeType);
                AudioChunkReceived?.Invoke(this, new LiveMusicAudioChunkReceivedEventArgs(chunk, sampleRate, channels));
            }
        }

        if (message.FilteredPrompt != null)
        {
            if (_logger?.IsEnabled(LogLevel.Warning) == true)
                _logger.LogWarning("Live Music prompt filtered: {Reason}", message.FilteredPrompt.FilteredReason);
            FilteredPromptReceived?.Invoke(this, new LiveMusicFilteredPromptEventArgs(message.FilteredPrompt));
        }

        MessageReceived?.Invoke(this, new LiveMusicMessageReceivedEventArgs(message));
    }

    /// <summary>
    /// Parses the sample rate and channel count from an audio MIME type such as <c>audio/l16;rate=48000;channels=2</c>.
    /// </summary>
    /// <param name="mimeType">The MIME type.</param>
    /// <returns>The sample rate and channel count, using Lyria RealTime defaults for missing values.</returns>
    internal static (int SampleRate, int Channels) ParseAudioFormat(string? mimeType)
    {
        var sampleRate = DefaultSampleRate;
        var channels = DefaultChannels;
        if (string.IsNullOrEmpty(mimeType))
            return (sampleRate, channels);

        foreach (var part in mimeType!.Split(';'))
        {
            var kv = part.Split('=');
            if (kv.Length != 2)
                continue;
            var key = kv[0].Trim();
            if (!int.TryParse(kv[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ||
                value <= 0)
                continue;
            if (string.Equals(key, "rate", StringComparison.OrdinalIgnoreCase))
                sampleRate = value;
            else if (string.Equals(key, "channels", StringComparison.OrdinalIgnoreCase))
                channels = value;
        }

        return (sampleRate, channels);
    }

    private void OnDisconnected(DisconnectionInfo info)
    {
#pragma warning disable CA1031 // Runs inside an Rx callback; exceptions are reported via ErrorOccurred
        try
        {
            if (info.Exception is not null)
            {
                _logger?.LogError(info.Exception, "Live Music connection closed with error ({Type})", info.Type);
                RaiseError(info.Exception);
            }

            var reason = info.CloseStatusDescription;
            TaskCompletionSource<bool> tcs;
            lock (_stateLock)
                tcs = _setupComplete;
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetException(new InvalidOperationException(string.IsNullOrEmpty(reason)
                    ? "The Live Music connection closed before setup completed."
                    : $"The Live Music connection closed before setup completed: {reason}"));
            }

            Disconnected?.Invoke(this,
                string.IsNullOrEmpty(reason) ? EventArgs.Empty : new ErrorMessageEventArgs(reason!));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling Live Music disconnection");
            RaiseError(ex);
        }
#pragma warning restore CA1031
    }

    private async Task RestoreAfterReconnectAsync()
    {
#pragma warning disable CA1031 // Fire-and-forget from the reconnection callback; failures are reported via ErrorOccurred
        try
        {
            List<WeightedPrompt>? prompts;
            LiveMusicGenerationConfig? config;
            LiveMusicPlaybackControl? playback;
            lock (_stateLock)
            {
                // The previous setup acknowledgement belongs to the old connection.
                if (_setupComplete.Task.IsCompleted)
                    _setupComplete = NewSetupCompletionSource();
                prompts = _lastWeightedPrompts;
                config = _lastConfig;
                playback = _lastPlaybackControl;
            }

            await SendSetupAsync().ConfigureAwait(false);
            if (!RestoreStateOnReconnect)
                return;
            await WaitForSetupCompleteAsync().ConfigureAwait(false);

            if (prompts != null)
                await SendAsync(new LiveMusicClientMessage
                    { ClientContent = new LiveMusicClientContent { WeightedPrompts = prompts } }).ConfigureAwait(false);
            if (config != null)
                await SendAsync(new LiveMusicClientMessage { MusicGenerationConfig = config }).ConfigureAwait(false);
            if (playback == LiveMusicPlaybackControl.PLAY)
                await SendAsync(new LiveMusicClientMessage { PlaybackControl = LiveMusicPlaybackControl.PLAY })
                    .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to restore the Live Music session after reconnect");
            RaiseError(ex);
        }
#pragma warning restore CA1031
    }

    #endregion

    #region Helpers

    private static TaskCompletionSource<bool> NewSetupCompletionSource() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private void RaiseError(Exception ex)
    {
        var handlers = ErrorOccurred;
        if (handlers == null)
            return;
        var args = new ErrorEventArgs(ex);
        // Invoke each handler separately so one throwing handler neither breaks the client nor starves the others.
        foreach (var handler in handlers.GetInvocationList().Cast<EventHandler<ErrorEventArgs>>())
        {
#pragma warning disable CA1031 // A throwing ErrorOccurred handler must not break the client
            try
            {
                handler(this, args);
            }
            catch (Exception handlerEx)
            {
                _logger?.LogError(handlerEx, "ErrorOccurred handler threw");
            }
#pragma warning restore CA1031
        }
    }

    private void ReleaseClient()
    {
        foreach (var subscription in _subscriptions)
            subscription.Dispose();
        _subscriptions.Clear();

        _client?.Dispose();
        _client = null;
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases the resources used by the <see cref="LiveMusicClient"/>.
    /// </summary>
    /// <param name="disposing">true to release managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
            DisconnectAsync().GetAwaiter().GetResult();
        _disposed = true;
    }

    /// <summary>
    /// Disconnects and releases the resources used by the <see cref="LiveMusicClient"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
