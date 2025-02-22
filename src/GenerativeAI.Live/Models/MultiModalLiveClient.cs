using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Live.Helper;
using GenerativeAI.Live.Logging;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;
using Websocket.Client;

// Assuming you have the logging extensions in this namespace

namespace GenerativeAI.Live;

/// <summary>
/// A client for interacting with the Gemini Multimodal Live API using WebSockets.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live">See Official API Documentation</seealso>
public class MultiModalLiveClient : IDisposable
{
    #region Constants

    private const int DefaultSampleRate = 24000;
    private const string DefaultAudioMimeType = "audio/pcm; rate=16000;";

    #endregion

    #region Private Fields

    AudioHeaderInfo? _lastHeaderInfo;

    private async Task<ClientWebSocket> GetClient()
    {
        var client = new ClientWebSocket()
        {
            Options =
            {
                KeepAliveInterval = TimeSpan.FromSeconds(10),
            }
        };
        var accessToken = await _platformAdapter.GetAccessTokenAsync();
        if(accessToken != null)
            client.Options.SetRequestHeader("Authorization", $"Bearer {accessToken.AccessToken}");
        return client;
    }
    
    private List<byte> _audioBuffer;
    private IWebsocketClient? _client;
    private readonly Guid _connectionId;
    private readonly ILogger? _logger;
    private readonly IPlatformAdapter _platformAdapter;

    #endregion

    #region Properties

    public IWebsocketClient? Client => _client;

    /// <summary>
    /// Gets the unique identifier for this WebSocket connection.
    /// </summary>
    public Guid ConnectionId => _connectionId;

    /// <summary>
    /// Gets or sets the name of the model being used.
    /// </summary>
    public string ModelName { get; set; }

    /// <summary>
    /// Gets the configuration settings for content generation.
    /// </summary>
    public GenerationConfig? Config { get; }

    /// <summary>
    /// Gets the collection of safety settings applied to the generation process.
    /// </summary>
    public ICollection<SafetySetting>? SafetySettings { get; }

    /// <summary>
    /// Gets the system instruction to guide model behavior during the session.
    /// </summary>
    public string? SystemInstruction { get; }

    /// <summary>
    /// Gets or sets the list of function tools available for the session.
    /// </summary>
    public List<IFunctionTool>? FunctionTools { get; set; }

    /// <summary>
    /// Gets or sets the configuration settings for the enabled tools.
    /// </summary>
    public ToolConfig? ToolConfig { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Google Search is enabled for the session.
    /// </summary>
    public bool UseGoogleSearch { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the code executor is enabled for the session.
    /// </summary>
    public bool UseCodeExecutor { get; set; } = false;

    #endregion

    #region Constructors

    /// <summary>
    /// Represents a client for managing multi-modal interactions with generative models.
    /// </summary>
    public MultiModalLiveClient(IPlatformAdapter platformAdapter, string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetySettings = null,
        string? systemInstruction = null,
        ILogger? logger = null)
    {
        _platformAdapter = platformAdapter ?? throw new ArgumentNullException(nameof(platformAdapter));
        ModelName = platformAdapter.GetMultiModalLiveModalName(modelName);
        Config = config ?? new GenerationConfig()
        {
            ResponseModalities = new List<Modality> { Modality.TEXT }
        };
        SafetySettings = safetySettings;
        SystemInstruction = systemInstruction;
        _connectionId = Guid.NewGuid();
        _logger = logger;
        _audioBuffer = new List<byte>(); // Initialize the buffer
    }

    #endregion

    #region Events

    /// <summary>
    /// Event triggered when an audio chunk is received.
    /// </summary>
    public event EventHandler<AudioBufferReceivedEventArgs>? AudioChunkReceived;

    /// <summary>
    /// Event triggered when the audio reception is completed.
    /// </summary>
    public event EventHandler<AudioBufferReceivedEventArgs>? AudioReceiveCompleted;

    /// <summary>
    /// Event triggered when generation is interrupted.
    /// </summary>
    public event EventHandler? GenerationInterrupted;

    /// <summary>
    /// Event triggered when a message is received from the server.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Event triggered when the WebSocket client is successfully connected.
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    /// Event triggered when the WebSocket client is disconnected.
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// Event triggered when an error occurs.
    /// </summary>
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;

    public event EventHandler<TextChunkReceivedArgs>? TextChunkReceived;

    #endregion

    #region Private Methods

    private void ProcessReceivedMessage(ResponseMessage msg)
    {
        _logger?.LogMessageReceived(msg.MessageType);

        try
        {
            BidiResponsePayload? responsePayload = null;
            if (msg.MessageType == WebSocketMessageType.Binary)
            {
                responsePayload = JsonSerializer.Deserialize<BidiResponsePayload>(msg.Binary);
            }
            else
            {
                responsePayload = JsonSerializer.Deserialize<BidiResponsePayload>(msg.Text);
            }

            if (responsePayload == null)
            {
                _logger?.LogWarning("Failed to deserialize message: {MessageType}", msg.MessageType);
                return;
            }
            if (responsePayload.ToolCall != null)
            {
                Task.Run(async () => await CallFunctions(responsePayload.ToolCall));   
            }
            ProcessTextChunk(responsePayload);
            ProcessAudioChunk(responsePayload);
           
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(responsePayload));
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Error deserializing message: {MessageType}", msg.MessageType);
            // Optionally re-throw or handle the error as appropriate
        }
    }

   

    private void ProcessTextChunk(BidiResponsePayload responsePayload)
    {
        if (responsePayload == null)
        {
            throw new ArgumentNullException(nameof(responsePayload));
        }

        if (responsePayload.ServerContent?.ModelTurn != null)
        {
            var textParts = responsePayload.ServerContent.ModelTurn.Parts;

            foreach (var part in textParts)
            {
                if (part.Text != null)
                {
                    this.TextChunkReceived.Invoke(this,
                        new TextChunkReceivedArgs(part.Text, responsePayload.ServerContent.TurnComplete == true));
                    _logger?.LogInformation("Text chunk received: {Text}", part.Text);
                }
            }
        }

        if (responsePayload.ServerContent?.TurnComplete == true)
        {
            _logger?.LogInformation("Text generation completed.");
        }

        if (responsePayload.ServerContent?.Interrupted == true)
        {
            _logger?.LogWarning("Text generation interrupted.");
            HandleInterruption();
        }
    }


    private void ProcessAudioChunk(BidiResponsePayload responsePayload)
    {
        if (responsePayload.ServerContent?.ModelTurn?.Parts != null)
        {
            var audioBlobs = responsePayload.ServerContent.ModelTurn.Parts
                .Where(p => p.InlineData?.MimeType?.Contains("audio") == true)
                .Select(p => p.InlineData!)
                .ToList();

            foreach (var blob in audioBlobs)
            {
                ProcessAudioBlob(blob);
            }
        }

        if (responsePayload.ServerContent?.TurnComplete == true)
        {
            CompleteAudioProcessing();
        }

        if (responsePayload.ServerContent?.Interrupted == true)
        {
            HandleInterruption();
        }
    }

    private void ProcessAudioBlob(Blob blob)
    {
        try
        {
            var audioBuffer = Convert.FromBase64String(blob.Data);
            int sampleRate = ExtractSampleRate(blob.MimeType);
            bool hasHeader = AudioHelper.IsValidWaveHeader(audioBuffer);

            var headerInfo = new AudioHeaderInfo()
            {
                Channels = 1,
                BitsPerSample = 16,
                SampleRate = sampleRate,
                HasHeader = hasHeader
            };
            this._lastHeaderInfo = headerInfo;

            var bufferReceived = new AudioBufferReceivedEventArgs(audioBuffer, headerInfo);

            _audioBuffer.AddRange(audioBuffer);
            _logger?.LogAudioChunkReceived(sampleRate, hasHeader, bufferReceived.Buffer.Length);
            AudioChunkReceived?.Invoke(this, bufferReceived);
        }
        catch (FormatException ex)
        {
            _logger?.LogError(ex, "Error decoding base64 audio data for connection {ConnectionId}", _connectionId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error processing audio blob for connection {ConnectionId}",
                _connectionId);
        }
    }

    private int ExtractSampleRate(string? mimeType)
    {
        if (mimeType != null && mimeType.Contains("rate="))
        {
            if (int.TryParse(mimeType.Split("rate=")[1].Split(";")[0], out var rate))
            {
                return rate;
            }
        }

        return DefaultSampleRate;
    }

    private void CompleteAudioProcessing()
    {
        if (_audioBuffer.Count == 0)
            return;
        var headerInfo = _lastHeaderInfo ?? new AudioHeaderInfo()
        {
            Channels = 1,
            BitsPerSample = 16,
            SampleRate = DefaultSampleRate,
            HasHeader = AudioHelper.IsValidWaveHeader(_audioBuffer.ToArray())
        };
        var bufferReceived = new AudioBufferReceivedEventArgs(_audioBuffer.ToArray(), headerInfo);
        _audioBuffer.Clear();
        _logger?.LogAudioReceiveCompleted(bufferReceived.Buffer.Length);
        AudioReceiveCompleted?.Invoke(this, bufferReceived);
        _lastHeaderInfo = null;
    }

    private void HandleInterruption()
    {
        _logger?.LogGenerationInterrupted();
        GenerationInterrupted?.Invoke(this, EventArgs.Empty);
        _audioBuffer.Clear();
    }

    private async Task CallFunctions(BidiGenerateContentToolCall responsePayloadToolCall,
        CancellationToken cancellationToken = default)
    {
        var functionResponses = new List<FunctionResponse>();

        foreach (var call in responsePayloadToolCall.FunctionCalls)
        {
            if (FunctionTools != null)
            {
                foreach (var tool in FunctionTools)
                {
                    if (tool.IsContainFunction(call.Name))
                    {
                        _logger?.LogFunctionCall(call.Name);
                        try
                        {
                            var functionResponse = await tool.CallAsync(call, cancellationToken);
                            functionResponses.Add(functionResponse);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error calling function {FunctionName} for connection {ConnectionId}",
                                call.Name, _connectionId);
                        }
                    }
                }
            }
            else
            {
                _logger?.LogWarning("No function tools configured, but a tool call was received: {FunctionName}",
                    call.Name);
            }
        }

        var toolResponse = new BidiGenerateContentToolResponse()
        {
            FunctionResponses = functionResponses.ToArray()
        };
        await SendToolResponseAsync(toolResponse, cancellationToken);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Connects to the MultiModal Live API WebSocket endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogConnectionAttempt();

        var url = _platformAdapter.GetMultiModalLiveUrl();
        var socketClient = await GetClient();
        _client = socketClient.WithReconnect(url); // Use the factory and an extension method for clarity

        _client.ReconnectionHappened.Subscribe(info =>
        {
            _logger?.LogInformation($"Reconnection happened: {info.Type}");
            // Consider re-sending setup or other state restoration here
        });

        _client.MessageReceived.ObserveOn(TaskPoolScheduler.Default)
            .Subscribe(ProcessReceivedMessage, ex =>
            {
                _logger?.LogError(ex, "Error in MessageReceived subscription for connection {ConnectionId}",
                    _connectionId);
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex)); 
            });

        _client.DisconnectionHappened.Subscribe(info =>
        {
            if (info.Type == DisconnectionType.Error)
            {
                _logger?.LogConnectionClosedWithError(info.Type, info.Exception!);
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(info.Exception!)); 
            }
            else
            {
                _logger?.LogConnectionClosed();
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        });

        try
        {
            await _client.Start().ConfigureAwait(false);
            _logger?.LogConnectionEstablished();
            Connected?.Invoke(this, EventArgs.Empty);
            await SendSetupAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to WebSocket for connection {ConnectionId}", _connectionId);
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }
    }

    /// <summary>
    /// Sends a setup configuration that initializes the generative model with
    /// appropriate tools, system instructions, and generation settings.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests during the setup process.</param>
    /// <returns>A task that represents the asynchronous operation of sending the setup configuration.</returns>
    public async Task SendSetupAsync(CancellationToken cancellationToken = default)
    {
        var tools = this.FunctionTools?.Select(s => s.AsTool()).ToList() ?? new List<Tool>();

        if (UseCodeExecutor)
            tools.Add(new Tool { CodeExecution = new CodeExecutionTool() });
        if (UseGoogleSearch)
            tools.Add(new Tool { GoogleSearch = new GoogleSearchTool() });

        var setup = new BidiGenerateContentSetup()
        {
           GenerationConfig = this.Config,
           Model = this.ModelName,
           SystemInstruction = !string.IsNullOrEmpty(SystemInstruction)
                 ? new Content(this.SystemInstruction, Roles.System)
                 : null,
            Tools = tools.Count > 0 ? tools.ToArray() : null,
        };
        await SendSetupAsync(setup, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disconnects the client from the MultiModal Live API and releases related resources.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client != null)
        {
            try
            {
                //Use close status and description.
                await _client.Stop(WebSocketCloseStatus.NormalClosure, "Client Disconnecting").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during disconnect for connection {ConnectionId}", _connectionId);
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex)); 

                // Don't re-throw; we're trying to disconnect
            }
            finally
            {
                _client.Dispose();
                _client = null;
            }
        }
    }


    /// <summary>
    /// Sends a setup message to configure the multi-modal live client with the provided generation settings and tools.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public async Task SendSetupAsync(BidiGenerateContentSetup setup, CancellationToken cancellationToken = default)
    {
        var payload = new BidiClientPayload { Setup = setup };
        await SendAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a client content message to the connected generative AI service.
    /// </summary>
    /// <param name="clientContent">The content to be sent, encapsulated in a <see cref="BidiGenerateContentClientContent"/> object.</param>
    /// <param name="cancellationToken">A token to observe for cancellation of the send operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendClientContentAsync(BidiGenerateContentClientContent clientContent,
        CancellationToken cancellationToken = default)
    {
        var payload = new BidiClientPayload { ClientContent = clientContent };
        await SendAsync(payload, cancellationToken).ConfigureAwait(false);
        _logger?.LogClientContentSent();
    }

    /// <summary>
    /// Sends a tool response message of type <see cref="BidiGenerateContentToolResponse"/> through the WebSocket connection.
    /// </summary>
    /// <param name="toolResponse">The tool response to be sent.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendToolResponseAsync(BidiGenerateContentToolResponse toolResponse,
        CancellationToken cancellationToken = default)
    {
        var payload = new BidiClientPayload { ToolResponse = toolResponse };
        await SendAsync(payload, cancellationToken).ConfigureAwait(false);
        _logger?.LogToolResponseSent();
    }


    private async Task SendAsync(BidiClientPayload payload, CancellationToken cancellationToken = default)
    {
        if (_client?.IsRunning != true)
        {
            var ex = new InvalidOperationException("The WebSocket client is not connected.");
            _logger?.LogError(ex, "SendAsync called when client is not running for connection {ConnectionId}",
                _connectionId);
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex)); 

            throw ex;
        }

        try
        {
            var json = JsonSerializer.Serialize(payload,DefaultSerializerOptions.Options);
            _logger?.LogMessageSent(json);

            _client.Send(json);
            //var bytes = Encoding.UTF8.GetBytes(json);
            //_client.Send(bytes); // Removed cancellationToken. This is handled by the library.
            await Task.CompletedTask;
        }
        catch (WebSocketException ex)
        {
            _logger?.LogError(ex, "WebSocket error sending message for connection {ConnectionId}", _connectionId);
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex)); 

            throw; // Re-throw to inform the caller
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during SendAsync for connection {ConnectionId}", _connectionId);
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex)); 
            throw; // Re-throw to inform the caller
        }
    }

    /// <summary>
    /// Assigns a collection of function tools and the associated configuration to the client.
    /// </summary>
    /// <param name="functionTools">The list of function tools to be added.</param>
    /// <param name="toolConfig">The configuration settings for the added tools.</param>
    public void AddFunctionTools(List<IFunctionTool> functionTools, ToolConfig? toolConfig)
    {
        this.FunctionTools = functionTools;
        this.ToolConfig = toolConfig;
    }

    /// <summary>
    /// Asynchronously sends audio data to the multi-modal client for processing.
    /// </summary>
    /// <param name="audioData">The audio data in byte array format to be sent.</param>
    /// <param name="mimeType">The MIME type of the audio data. Defaults to "audio/pcm; rate=16000;".</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendAudioAsync(byte[] audioData, string mimeType = DefaultAudioMimeType,
        CancellationToken cancellationToken = default)
    {
        var realtimeInput = new BidiGenerateContentRealtimeInput
        {
            MediaChunks = new[] { new Blob() { Data = Convert.ToBase64String(audioData), MimeType = mimeType } }
        };

        var payload = new BidiClientPayload { RealtimeInput = realtimeInput };
        await SendAsync(payload, cancellationToken);
    }

    /// <summary>
    /// Sends a text prompt as a user input to the model for processing.
    /// </summary>
    /// <param name="prompt">The text input provided by the user to the model.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete, allowing cancellation if needed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SentTextAsync(string prompt,
        CancellationToken cancellationToken = default)
    {
        var content = new Content(prompt, Roles.User);
        var clientContent = new BidiGenerateContentClientContent()
        {
            Turns = [content]
        };

        var payload = new BidiClientPayload { ClientContent = clientContent };
        await SendAsync(payload, cancellationToken);
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources (like the WebsocketClient)
                DisconnectAsync().GetAwaiter().GetResult(); // Synchronous disconnect
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}