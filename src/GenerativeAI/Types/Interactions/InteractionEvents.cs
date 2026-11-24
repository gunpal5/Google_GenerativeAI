using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// A Server-Sent Event of a streamed interaction, discriminated by <c>event_type</c>. Unknown event types are
/// read as <see cref="UnknownInteractionSseEvent"/>. It is also the base of the server events of live interactions
/// (python-genai <c>ServerEvent</c>): <see cref="InteractionConnectedEvent"/>, <see cref="InteractionCancelledEvent"/>,
/// <see cref="LiveInteractionCreatedEvent"/>, <see cref="LiveInteractionCompletedEvent"/> and the shared step and error
/// events.
/// </summary>
[JsonConverter(typeof(InteractionSseEventConverter))]
public abstract class InteractionSseEvent
{
    /// <summary>
    /// Initializes a new event with the given discriminator.
    /// </summary>
    /// <param name="eventType">The <c>event_type</c> discriminator.</param>
    protected InteractionSseEvent(string eventType)
    {
        EventType = eventType;
    }

    /// <summary>The event type (see <see cref="InteractionEventTypes"/>).</summary>
    [JsonPropertyName("event_type")]
    public string EventType { get; }

    /// <summary>
    /// Token that identifies this event; pass it as <c>lastEventId</c> to resume the stream after this event.
    /// </summary>
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>The interaction was created (<c>interaction.created</c>).</summary>
public sealed class InteractionCreatedEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCreatedEvent"/> class.</summary>
    public InteractionCreatedEvent() : base(InteractionEventTypes.InteractionCreated)
    {
    }

    /// <summary>The (partial) interaction resource.</summary>
    [JsonPropertyName("interaction")]
    public Interaction? Interaction { get; set; }
}

/// <summary>The interaction completed (<c>interaction.completed</c>).</summary>
public sealed class InteractionCompletedEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCompletedEvent"/> class.</summary>
    public InteractionCompletedEvent() : base(InteractionEventTypes.InteractionCompleted)
    {
    }

    /// <summary>The (partial) interaction resource.</summary>
    [JsonPropertyName("interaction")]
    public Interaction? Interaction { get; set; }
}

/// <summary>The interaction status changed (<c>interaction.status_update</c>).</summary>
public sealed class InteractionStatusUpdateEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionStatusUpdateEvent"/> class.</summary>
    public InteractionStatusUpdateEvent() : base(InteractionEventTypes.InteractionStatusUpdate)
    {
    }

    /// <summary>The interaction ID.</summary>
    [JsonPropertyName("interaction_id")]
    public string? InteractionId { get; set; }

    /// <summary>The new status (see <see cref="InteractionStatuses"/>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Continuation token to resume decoding, when the status requires continuation.</summary>
    [JsonPropertyName("continuation_token")]
    public string? ContinuationToken { get; set; }

    /// <summary>Token usage so far.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }
}

/// <summary>A step started (<c>step.start</c>).</summary>
public sealed class InteractionStepStartEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionStepStartEvent"/> class.</summary>
    public InteractionStepStartEvent() : base(InteractionEventTypes.StepStart)
    {
    }

    /// <summary>The index of the step.</summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>The step (possibly without its final content, which arrives as deltas).</summary>
    [JsonPropertyName("step")]
    public InteractionStep? Step { get; set; }
}

/// <summary>Metadata accompanying a streamed delta.</summary>
public sealed class InteractionStepDeltaMetadata
{
    /// <summary>Total token usage so far.</summary>
    [JsonPropertyName("total_usage")]
    public InteractionUsage? TotalUsage { get; set; }
}

/// <summary>Incremental data for a step (<c>step.delta</c>).</summary>
public sealed class InteractionStepDeltaEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionStepDeltaEvent"/> class.</summary>
    public InteractionStepDeltaEvent() : base(InteractionEventTypes.StepDelta)
    {
    }

    /// <summary>The index of the step the delta belongs to.</summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>The delta.</summary>
    [JsonPropertyName("delta")]
    public InteractionStepDeltaData? Delta { get; set; }

    /// <summary>Optional metadata.</summary>
    [JsonPropertyName("metadata")]
    public InteractionStepDeltaMetadata? Metadata { get; set; }
}

/// <summary>A step finished (<c>step.stop</c>).</summary>
public sealed class InteractionStepStopEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionStepStopEvent"/> class.</summary>
    public InteractionStepStopEvent() : base(InteractionEventTypes.StepStop)
    {
    }

    /// <summary>The index of the step.</summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>Token usage of this step.</summary>
    [JsonPropertyName("step_usage")]
    public InteractionUsage? StepUsage { get; set; }

    /// <summary>Token usage of the interaction so far.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }
}

/// <summary>An error occurred while streaming (<c>error</c>).</summary>
public sealed class InteractionErrorEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionErrorEvent"/> class.</summary>
    public InteractionErrorEvent() : base(InteractionEventTypes.Error)
    {
    }

    /// <summary>The error.</summary>
    [JsonPropertyName("error")]
    public InteractionError? Error { get; set; }
}

/// <summary>An event whose <c>event_type</c> this SDK does not know.</summary>
public sealed class UnknownInteractionSseEvent : InteractionSseEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionSseEvent"/> class.
    /// </summary>
    /// <param name="eventType">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionSseEvent(string? eventType, JsonNode raw) : base(eventType ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the event.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>
/// The payload of a <c>step.delta</c> event, discriminated by <c>type</c>. Unknown delta types are read as
/// <see cref="UnknownInteractionStepDeltaData"/>.
/// </summary>
[JsonConverter(typeof(InteractionStepDeltaDataConverter))]
public abstract class InteractionStepDeltaData
{
    /// <summary>
    /// Initializes a new delta with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionStepDeltaData(string type)
    {
        Type = type;
    }

    /// <summary>The delta type (see <see cref="InteractionDeltaTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>A signature hash for backend validation, on delta types that carry one.</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>A text delta (<c>text</c>).</summary>
public sealed class InteractionTextDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionTextDelta"/> class.</summary>
    public InteractionTextDelta() : base(InteractionDeltaTypes.Text)
    {
    }

    /// <summary>The text chunk.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>Base class for media deltas (image, audio, video, document).</summary>
public abstract class InteractionMediaDelta : InteractionStepDeltaData
{
    /// <summary>
    /// Initializes a new media delta with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionMediaDelta(string type) : base(type)
    {
    }

    /// <summary>The base64-encoded media bytes.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>The MIME type.</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>The media URI.</summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

/// <summary>An image delta (<c>image</c>).</summary>
public sealed class InteractionImageDelta : InteractionMediaDelta
{
    /// <summary>Initializes a new instance of the <see cref="InteractionImageDelta"/> class.</summary>
    public InteractionImageDelta() : base(InteractionDeltaTypes.Image)
    {
    }

    /// <summary>The media resolution.</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

/// <summary>An audio delta (<c>audio</c>).</summary>
public sealed class InteractionAudioDelta : InteractionMediaDelta
{
    /// <summary>Initializes a new instance of the <see cref="InteractionAudioDelta"/> class.</summary>
    public InteractionAudioDelta() : base(InteractionDeltaTypes.Audio)
    {
    }

    /// <summary>The number of audio channels.</summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    /// <summary>The sample rate of the audio.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }
}

/// <summary>A video delta (<c>video</c>).</summary>
public sealed class InteractionVideoDelta : InteractionMediaDelta
{
    /// <summary>Initializes a new instance of the <see cref="InteractionVideoDelta"/> class.</summary>
    public InteractionVideoDelta() : base(InteractionDeltaTypes.Video)
    {
    }

    /// <summary>The media resolution.</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

/// <summary>A document delta (<c>document</c>).</summary>
public sealed class InteractionDocumentDelta : InteractionMediaDelta
{
    /// <summary>Initializes a new instance of the <see cref="InteractionDocumentDelta"/> class.</summary>
    public InteractionDocumentDelta() : base(InteractionDeltaTypes.Document)
    {
    }
}

/// <summary>Annotations for the text of the step (<c>text_annotation_delta</c>).</summary>
public sealed class InteractionTextAnnotationDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionTextAnnotationDelta"/> class.</summary>
    public InteractionTextAnnotationDelta() : base(InteractionDeltaTypes.TextAnnotation)
    {
    }

    /// <summary>Citation information.</summary>
    [JsonPropertyName("annotations")]
    public List<InteractionAnnotation>? Annotations { get; set; }
}

/// <summary>A chunk of function-call arguments JSON (<c>arguments_delta</c>).</summary>
public sealed class InteractionArgumentsDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionArgumentsDelta"/> class.</summary>
    public InteractionArgumentsDelta() : base(InteractionDeltaTypes.Arguments)
    {
    }

    /// <summary>A partial JSON string of the arguments.</summary>
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}

/// <summary>A thought signature (<c>thought_signature</c>).</summary>
public sealed class InteractionThoughtSignatureDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionThoughtSignatureDelta"/> class.</summary>
    public InteractionThoughtSignatureDelta() : base(InteractionDeltaTypes.ThoughtSignature)
    {
    }
}

/// <summary>A thought summary chunk (<c>thought_summary</c>).</summary>
public sealed class InteractionThoughtSummaryDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionThoughtSummaryDelta"/> class.</summary>
    public InteractionThoughtSummaryDelta() : base(InteractionDeltaTypes.ThoughtSummary)
    {
    }

    /// <summary>The summary content.</summary>
    [JsonPropertyName("content")]
    public InteractionContent? Content { get; set; }
}

/// <summary>A function result delta (<c>function_result</c>).</summary>
public sealed class InteractionFunctionResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFunctionResultDelta"/> class.</summary>
    public InteractionFunctionResultDelta() : base(InteractionDeltaTypes.FunctionResult)
    {
    }

    /// <summary>The result: a JSON object, a string, or an array of content blocks.</summary>
    [JsonPropertyName("result")]
    public JsonNode? Result { get; set; }

    /// <summary>The tool name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Whether the call resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }
}

/// <summary>A code execution call delta (<c>code_execution_call</c>).</summary>
public sealed class InteractionCodeExecutionCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCodeExecutionCallDelta"/> class.</summary>
    public InteractionCodeExecutionCallDelta() : base(InteractionDeltaTypes.CodeExecutionCall)
    {
    }

    /// <summary>The code execution arguments.</summary>
    [JsonPropertyName("arguments")]
    public InteractionCodeExecutionArguments? Arguments { get; set; }
}

/// <summary>A code execution result delta (<c>code_execution_result</c>).</summary>
public sealed class InteractionCodeExecutionResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCodeExecutionResultDelta"/> class.</summary>
    public InteractionCodeExecutionResultDelta() : base(InteractionDeltaTypes.CodeExecutionResult)
    {
    }

    /// <summary>The output of the code execution.</summary>
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    /// <summary>Whether the execution resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }
}

/// <summary>A URL context call delta (<c>url_context_call</c>).</summary>
public sealed class InteractionUrlContextCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUrlContextCallDelta"/> class.</summary>
    public InteractionUrlContextCallDelta() : base(InteractionDeltaTypes.UrlContextCall)
    {
    }

    /// <summary>The URL context arguments.</summary>
    [JsonPropertyName("arguments")]
    public InteractionUrlContextArguments? Arguments { get; set; }
}

/// <summary>A URL context result delta (<c>url_context_result</c>).</summary>
public sealed class InteractionUrlContextResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUrlContextResultDelta"/> class.</summary>
    public InteractionUrlContextResultDelta() : base(InteractionDeltaTypes.UrlContextResult)
    {
    }

    /// <summary>The URL context results.</summary>
    [JsonPropertyName("result")]
    public List<InteractionUrlContextResult>? Result { get; set; }

    /// <summary>Whether the call resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }
}

/// <summary>A Google Search call delta (<c>google_search_call</c>).</summary>
public sealed class InteractionGoogleSearchCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleSearchCallDelta"/> class.</summary>
    public InteractionGoogleSearchCallDelta() : base(InteractionDeltaTypes.GoogleSearchCall)
    {
    }

    /// <summary>The search arguments.</summary>
    [JsonPropertyName("arguments")]
    public InteractionQueryArguments? Arguments { get; set; }
}

/// <summary>A Google Search result delta (<c>google_search_result</c>).</summary>
public sealed class InteractionGoogleSearchResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleSearchResultDelta"/> class.</summary>
    public InteractionGoogleSearchResultDelta() : base(InteractionDeltaTypes.GoogleSearchResult)
    {
    }

    /// <summary>The search results.</summary>
    [JsonPropertyName("result")]
    public List<InteractionGoogleSearchResult>? Result { get; set; }

    /// <summary>Whether the search resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }
}

/// <summary>A Google Maps call delta (<c>google_maps_call</c>).</summary>
public sealed class InteractionGoogleMapsCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleMapsCallDelta"/> class.</summary>
    public InteractionGoogleMapsCallDelta() : base(InteractionDeltaTypes.GoogleMapsCall)
    {
    }

    /// <summary>The Google Maps arguments.</summary>
    [JsonPropertyName("arguments")]
    public InteractionQueryArguments? Arguments { get; set; }
}

/// <summary>A Google Maps result delta (<c>google_maps_result</c>).</summary>
public sealed class InteractionGoogleMapsResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleMapsResultDelta"/> class.</summary>
    public InteractionGoogleMapsResultDelta() : base(InteractionDeltaTypes.GoogleMapsResult)
    {
    }

    /// <summary>The Google Maps results.</summary>
    [JsonPropertyName("result")]
    public List<InteractionGoogleMapsResult>? Result { get; set; }
}

/// <summary>A file search call delta (<c>file_search_call</c>).</summary>
public sealed class InteractionFileSearchCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFileSearchCallDelta"/> class.</summary>
    public InteractionFileSearchCallDelta() : base(InteractionDeltaTypes.FileSearchCall)
    {
    }
}

/// <summary>A file search result delta (<c>file_search_result</c>).</summary>
public sealed class InteractionFileSearchResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFileSearchResultDelta"/> class.</summary>
    public InteractionFileSearchResultDelta() : base(InteractionDeltaTypes.FileSearchResult)
    {
    }

    /// <summary>The file search results (their schema is not defined by python-genai; kept as JSON).</summary>
    [JsonPropertyName("result")]
    public List<JsonObject>? Result { get; set; }
}

/// <summary>An MCP server tool call delta (<c>mcp_server_tool_call</c>).</summary>
public sealed class InteractionMcpServerToolCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionMcpServerToolCallDelta"/> class.</summary>
    public InteractionMcpServerToolCallDelta() : base(InteractionDeltaTypes.McpServerToolCall)
    {
    }

    /// <summary>The tool name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The MCP server name.</summary>
    [JsonPropertyName("server_name")]
    public string? ServerName { get; set; }

    /// <summary>The tool arguments.</summary>
    [JsonPropertyName("arguments")]
    public JsonObject? Arguments { get; set; }
}

/// <summary>An MCP server tool result delta (<c>mcp_server_tool_result</c>).</summary>
public sealed class InteractionMcpServerToolResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionMcpServerToolResultDelta"/> class.</summary>
    public InteractionMcpServerToolResultDelta() : base(InteractionDeltaTypes.McpServerToolResult)
    {
    }

    /// <summary>The tool name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The MCP server name.</summary>
    [JsonPropertyName("server_name")]
    public string? ServerName { get; set; }

    /// <summary>The result: a JSON object, a string, or an array of content blocks.</summary>
    [JsonPropertyName("result")]
    public JsonNode? Result { get; set; }
}

/// <summary>A retrieval call delta (<c>retrieval_call</c>).</summary>
public sealed class InteractionRetrievalCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionRetrievalCallDelta"/> class.</summary>
    public InteractionRetrievalCallDelta() : base(InteractionDeltaTypes.RetrievalCall)
    {
    }

    /// <summary>The retrieval arguments.</summary>
    [JsonPropertyName("arguments")]
    public InteractionQueryArguments? Arguments { get; set; }

    /// <summary>The type of retrieval.</summary>
    [JsonPropertyName("retrieval_type")]
    public string? RetrievalType { get; set; }
}

/// <summary>A retrieval result delta (<c>retrieval_result</c>).</summary>
public sealed class InteractionRetrievalResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionRetrievalResultDelta"/> class.</summary>
    public InteractionRetrievalResultDelta() : base(InteractionDeltaTypes.RetrievalResult)
    {
    }

    /// <summary>Whether the retrieval resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }
}

/// <summary>A processing call delta (<c>processing_call</c>).</summary>
public sealed class InteractionProcessingCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionProcessingCallDelta"/> class.</summary>
    public InteractionProcessingCallDelta() : base(InteractionDeltaTypes.ProcessingCall)
    {
    }
}

/// <summary>A processing result delta (<c>processing_result</c>).</summary>
public sealed class InteractionProcessingResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionProcessingResultDelta"/> class.</summary>
    public InteractionProcessingResultDelta() : base(InteractionDeltaTypes.ProcessingResult)
    {
    }
}

/// <summary>A tool search call delta (<c>tool_search_call</c>).</summary>
public sealed class InteractionToolSearchCallDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionToolSearchCallDelta"/> class.</summary>
    public InteractionToolSearchCallDelta() : base(InteractionDeltaTypes.ToolSearchCall)
    {
    }

    /// <summary>The tool search arguments.</summary>
    [JsonPropertyName("arguments")]
    public InteractionToolSearchCallArguments? Arguments { get; set; }
}

/// <summary>A tool search result delta (<c>tool_search_result</c>).</summary>
public sealed class InteractionToolSearchResultDelta : InteractionStepDeltaData
{
    /// <summary>Initializes a new instance of the <see cref="InteractionToolSearchResultDelta"/> class.</summary>
    public InteractionToolSearchResultDelta() : base(InteractionDeltaTypes.ToolSearchResult)
    {
    }

    /// <summary>The retrieved function declarations.</summary>
    [JsonPropertyName("result")]
    public List<InteractionFunctionTool>? Result { get; set; }
}

/// <summary>A delta whose <c>type</c> this SDK does not know.</summary>
public sealed class UnknownInteractionStepDeltaData : InteractionStepDeltaData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionStepDeltaData"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionStepDeltaData(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the delta.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}
