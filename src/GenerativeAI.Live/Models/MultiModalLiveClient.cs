using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GenerativeAI.Core;
using GenerativeAI.Live.Events;
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
    private const string DefaultAudioMimeType = "audio/pcm;rate=16000";

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
        var accessToken = await _platformAdapter.GetAccessTokenAsync().ConfigureAwait(false);
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

    /// <summary>
    /// Gets the WebSocket client used to communicate with the Gemini Multimodal Live API.
    /// </summary>
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
    public bool UseGoogleSearch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the code executor is enabled for the session.
    /// </summary>
    public bool UseCodeExecutor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether input audio transcription is enabled.
    /// When enabled, audio inputs will be transcribed into text for further processing.
    /// </summary>
    public bool InputAudioTranscriptionEnabled { get; set; }

    /// <summary>
    /// Indicates whether transcription of output audio is enabled.
    /// </summary>
    public bool OutputAudioTranscriptionEnabled { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Represents a client for managing multi-modal interactions with generative models.
    /// </summary>
    public MultiModalLiveClient(IPlatformAdapter platformAdapter, string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetySettings = null,
        string? systemInstruction = null,
        bool inputAudioTranscriptionEnabled = false, bool outputAudioTranscriptionEnabled = false,
        ILogger? logger = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(platformAdapter);
        _platformAdapter = platformAdapter;
#else
        _platformAdapter = platformAdapter ?? throw new ArgumentNullException(nameof(platformAdapter));
#endif
        ModelName = platformAdapter.GetMultiModalLiveModalName(modelName) ?? throw new InvalidOperationException($"Failed to get multimodal live model name for '{modelName}'");
        Config = config ?? new GenerationConfig()
        {
            ResponseModalities = new List<Modality> { Modality.TEXT }
        };
        InputAudioTranscriptionEnabled = inputAudioTranscriptionEnabled;
        OutputAudioTranscriptionEnabled = outputAudioTranscriptionEnabled;
        SafetySettings = safetySettings;
        SystemInstruction = systemInstruction;
        _connectionId = Guid.NewGuid();
        _logger = logger;
        _audioBuffer = new List<byte>(); // Initialize the buffer
    }

    #endregion

    #region Events
#pragma warning disable CA1003
    /// <summary>
    /// Event triggered after the client is created, but before a connection is attempted.
    /// </summary>
    public event EventHandler<ClientCreatedEventArgs>? ClientCreated;

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

    /// <summary>
    /// Event triggered when a chunk of text is received from the server during the live API session.
    /// </summary>
    public event EventHandler<TextChunkReceivedArgs>? TextChunkReceived;

    /// <summary>
    /// Event triggered upon receiving input transcription data.
    /// </summary>
    public event EventHandler<Transcription>? InputTranscriptionReceived;

    /// <summary>
    /// An event triggered when an output transcription is received from the system.
    /// </summary>
    
    public event EventHandler<Transcription>? OutputTranscriptionReceived;

    /// <summary>
    /// Message sent by the server to indicate that the current connection should be terminated
    /// and the client should cease sending further requests on this stream.
    /// This is often used for graceful shutdown or when the server is no longer able to
    /// process requests on the current stream.
    /// </summary>

    public event EventHandler<LiveServerGoAway>? GoAwayReceived;


    /// <summary>
    /// Occurs when the server sends an update that allows the current session to be resumed.
    /// This event provides information related to session resumption, enabling the client to continue
    /// an existing session without starting over.
    /// </summary>
    public event EventHandler<LiveServerSessionResumptionUpdate>? SessionResumableUpdateReceived; 
#pragma warning restore CA1003
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
                responsePayload = JsonSerializer.Deserialize(msg.Binary,(JsonTypeInfo<BidiResponsePayload>) DefaultSerializerOptions.Options.GetTypeInfo(typeof(BidiResponsePayload)));
            }
            else
            {
                if (msg.Text == null)
                {
                    _logger?.LogWarning("Received null text message");
                    return;
                }
                responsePayload = JsonSerializer.Deserialize(msg.Text,(JsonTypeInfo<BidiResponsePayload>) DefaultSerializerOptions.Options.GetTypeInfo(typeof(BidiResponsePayload)));
            }

            if (responsePayload == null)
            {
                _logger?.LogWarning("Failed to deserialize message: {MessageType}", msg.MessageType);
                return;
            }
            if (responsePayload.ToolCall != null)
            {
                Task.Run(async () => await CallFunctions(responsePayload.ToolCall).ConfigureAwait(false));   
            }
            ProcessTextChunk(responsePayload);
            ProcessAudioChunk(responsePayload);
            ProcessInputTranscription(responsePayload);
            ProcessOutputTranscription(responsePayload);
            ProcessSessionResumableUpdate(responsePayload);
            ProcessGoAway(responsePayload);
           
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(responsePayload);
#else
        if (responsePayload == null)
        {
            throw new ArgumentNullException(nameof(responsePayload));
        }
#endif

        if (responsePayload.ServerContent?.ModelTurn != null)
        {
            var textParts = responsePayload.ServerContent.ModelTurn.Parts;

            foreach (var part in textParts)
            {
                if (part.Text != null)
                {
                    this.TextChunkReceived?.Invoke(this,
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
#if NET6_0_OR_GREATER
                .Where(p => p.InlineData?.MimeType?.Contains("audio", StringComparison.Ordinal) == true)
#else
                .Where(p => p.InlineData?.MimeType?.Contains("audio") == true)
#endif
                
                .ToList();

            foreach (var blob in audioBlobs)
            {
                if (blob.InlineData != null)
                {
                    ProcessAudioBlob(blob.InlineData);
                }
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
            if (blob.Data == null)
            {
                _logger?.LogWarning("Received blob with null data");
                return;
            }
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
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error processing audio blob for connection {ConnectionId}",
                _connectionId);
        }
#pragma warning restore CA1031
    }

    private static int ExtractSampleRate(string? mimeType)
    {
#if NET6_0_OR_GREATER
        if (mimeType != null && mimeType.Contains("rate=", StringComparison.Ordinal))
#else
        if (mimeType != null && mimeType.Contains("rate="))
#endif
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

    private void ProcessInputTranscription(BidiResponsePayload responsePayload)
    {
        if (responsePayload.ServerContent?.InputTranscription != null)
        {
            InputTranscriptionReceived?.Invoke(this, responsePayload.ServerContent.InputTranscription);
        }
    }
    
    private void ProcessOutputTranscription(BidiResponsePayload responsePayload)
    {
        if (responsePayload.ServerContent?.OutputTranscription != null)
        {
            OutputTranscriptionReceived?.Invoke(this, responsePayload.ServerContent.OutputTranscription);
        }
    }
    
    private void ProcessSessionResumableUpdate(BidiResponsePayload responsePayload)
    {
        if (responsePayload.SessionResumptionUpdate != null)
        {
            SessionResumableUpdateReceived?.Invoke(this, responsePayload.SessionResumptionUpdate);
        }
    }
    
    private void ProcessGoAway(BidiResponsePayload responsePayload)
    {
        if (responsePayload.GoAway != null)
        {
            GoAwayReceived?.Invoke(this, responsePayload.GoAway);
        }
    }
    
    private async Task CallFunctions(BidiGenerateContentToolCall responsePayloadToolCall,
        CancellationToken cancellationToken = default)
    {
        var functionResponses = new List<FunctionResponse>();

        if (responsePayloadToolCall.FunctionCalls != null)
        {
            foreach (var call in responsePayloadToolCall.FunctionCalls)
        {
            if (FunctionTools != null)
            {
                foreach (var tool in FunctionTools)
                {
                    if (call.Name != null && tool.IsContainFunction(call.Name))
                    {
                        _logger?.LogFunctionCall(call.Name);
                        try
                        {
                            var functionResponse = await tool.CallAsync(call, cancellationToken).ConfigureAwait(false);
                            if(functionResponse != null)
                                functionResponses.Add(functionResponse);
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Error calling function {FunctionName} for connection {ConnectionId}",
                                call.Name, _connectionId);
                        }
#pragma warning restore CA1031
                    }
                }
            }
            else
            {
                _logger?.LogWarning("No function tools configured, but a tool call was received: {FunctionName}",
                    call.Name);
            }
        }
        }

        if (functionResponses.Count > 0)
        {
            var toolResponse = new BidiGenerateContentToolResponse()
            {
                FunctionResponses = functionResponses.ToArray()
            };
            await SendToolResponseAsync(toolResponse, cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Connects to the MultiModal Live API WebSocket endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ConnectAsync(bool autoSendSetup = true,CancellationToken cancellationToken = default)
    {
        _logger?.LogConnectionAttempt();

        var url = _platformAdapter.GetMultiModalLiveUrl();
#pragma warning disable CA2000 // Dispose objects before losing scope - object is disposed in Dispose method
        var socketClient = await GetClient().ConfigureAwait(false);
        _client = socketClient.WithReconnect(url); // Use the factory and an extension method for clarity
#pragma warning restore CA2000

        ClientCreated?.Invoke(this, new ClientCreatedEventArgs(_client));

        _client.ReconnectionHappened.Subscribe(info =>
        {
#pragma warning disable CA2254 // Template should be a static expression
            _logger?.LogInformation($"Reconnection happened: {info.Type}");
#pragma warning restore CA2254
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
            if (info.Exception is not null)
            {
                _logger?.LogConnectionClosedWithError(info.Type, info.Exception!);
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(info.Exception!));
            }

            if (!string.IsNullOrEmpty(info.CloseStatusDescription))
            { 
                _logger?.LogConnectionClosedWithStatus(info.CloseStatus, info.CloseStatusDescription);
            }
            else
            {
                _logger?.LogConnectionClosed();
            }

            var eventArgs = !string.IsNullOrEmpty(info.CloseStatusDescription) ?
                new ErrorMessageEventArgs(info.CloseStatusDescription) :
                EventArgs.Empty;
            Disconnected?.Invoke(this, eventArgs);
        });

        try
        {
            await _client.Start().ConfigureAwait(false);
            _logger?.LogConnectionEstablished();
            Connected?.Invoke(this, EventArgs.Empty);
            if (autoSendSetup)
            {
                await SendSetupAsync(cancellationToken).ConfigureAwait(false);
            }
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
            InputAudioTranscription = InputAudioTranscriptionEnabled ? new AudioTranscriptionConfig(): null,
            OutputAudioTranscription = OutputAudioTranscriptionEnabled ? new AudioTranscriptionConfig() : null,
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during disconnect for connection {ConnectionId}", _connectionId);
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex)); 

                // Don't re-throw; we're trying to disconnect
            }
#pragma warning restore CA1031
            finally
            {
                _client.Dispose();
                _client = null;
            }
        }
    }


    /// <summary>
    /// Configures the multi-modal live client by sending setup details including generation settings, tools, and model configurations asynchronously.
    /// </summary>
    /// <param name="setup">The setup configuration for the bidirectional generate content session.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that may be used to cancel the asynchronous operation prior to its completion.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task SendSetupAsync(BidiGenerateContentSetup setup, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(setup);
#else
        if (setup == null) throw new ArgumentNullException(nameof(setup));
#endif
        if (string.IsNullOrEmpty(setup.Model))
            throw new ArgumentException("Model name cannot be null or empty.", nameof(setup));
#if NET6_0_OR_GREATER
#pragma warning disable CA1307 // Specify StringComparison for clarity
        if(!setup.Model.Contains('/'))
#pragma warning restore CA1307
#else
        if(!setup.Model.Contains("/"))
#endif
            throw new ArgumentException("Please provide a valid model name such as 'models/gemini-2.0-flash-live-001'.");
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
            var json = JsonSerializer.Serialize(payload,DefaultSerializerOptions.Options.GetTypeInfo(typeof(BidiClientPayload)));
            _logger?.LogMessageSent(json);

            _client.Send(json);
            //var bytes = Encoding.UTF8.GetBytes(json);
            //_client.Send(bytes); // Removed cancellationToken. This is handled by the library.
            await Task.CompletedTask.ConfigureAwait(false);
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
        await SendAsync(payload, cancellationToken).ConfigureAwait(false);
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
            Turns = [content],
            TurnComplete = true
        };

        var payload = new BidiClientPayload { ClientContent = clientContent };
        await SendAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region IDisposable

    private bool _disposed;

    /// <summary>
    /// Releases the unmanaged resources used by the MultiModalLiveClient and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}