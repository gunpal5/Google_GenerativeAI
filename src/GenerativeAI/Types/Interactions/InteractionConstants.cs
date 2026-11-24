namespace GenerativeAI.Types;

/// <summary>
/// Known values of <see cref="Interaction.Status"/>. The API may return values that are not listed here,
/// so the status is exposed as a string.
/// </summary>
public static class InteractionStatuses
{
    /// <summary>The interaction is still running.</summary>
    public const string InProgress = "in_progress";

    /// <summary>The interaction is waiting for the client (for example, for function results).</summary>
    public const string RequiresAction = "requires_action";

    /// <summary>The interaction completed successfully.</summary>
    public const string Completed = "completed";

    /// <summary>The interaction failed.</summary>
    public const string Failed = "failed";

    /// <summary>The interaction was cancelled.</summary>
    public const string Cancelled = "cancelled";

    /// <summary>The interaction finished without completing.</summary>
    public const string Incomplete = "incomplete";

    /// <summary>The interaction stopped because its budget was exceeded.</summary>
    public const string BudgetExceeded = "budget_exceeded";

    /// <summary>The interaction is queued and has not started yet.</summary>
    public const string Queued = "queued";

    /// <summary>
    /// The per-request decode limit was reached. Pass <see cref="Interaction.ContinuationToken"/> back to
    /// create a continuation (see <c>InteractionsClient.ContinueAsync</c>).
    /// </summary>
    public const string RequiresContinuation = "requires_continuation";

    /// <summary>A live interaction is idle, waiting for client input.</summary>
    public const string Idle = "idle";

    /// <summary>
    /// Returns <c>true</c> when the status means the server is still working on the interaction
    /// (<see cref="InProgress"/>, <see cref="Queued"/>, or no status).
    /// </summary>
    /// <param name="status">The status to check.</param>
    /// <returns><c>true</c> when the interaction is still running.</returns>
    public static bool IsRunning(string? status)
    {
        return string.IsNullOrEmpty(status) || status == InProgress || status == Queued;
    }
}

/// <summary>
/// Known values of the <c>type</c> discriminator of <see cref="InteractionStep"/>.
/// </summary>
public static class InteractionStepTypes
{
    /// <summary>User input step.</summary>
    public const string UserInput = "user_input";

    /// <summary>Model output step.</summary>
    public const string ModelOutput = "model_output";

    /// <summary>Thought (reasoning) step.</summary>
    public const string Thought = "thought";

    /// <summary>Function call step.</summary>
    public const string FunctionCall = "function_call";

    /// <summary>Function result step.</summary>
    public const string FunctionResult = "function_result";

    /// <summary>Code execution call step.</summary>
    public const string CodeExecutionCall = "code_execution_call";

    /// <summary>Code execution result step.</summary>
    public const string CodeExecutionResult = "code_execution_result";

    /// <summary>URL context call step.</summary>
    public const string UrlContextCall = "url_context_call";

    /// <summary>URL context result step.</summary>
    public const string UrlContextResult = "url_context_result";

    /// <summary>Google Search call step.</summary>
    public const string GoogleSearchCall = "google_search_call";

    /// <summary>Google Search result step.</summary>
    public const string GoogleSearchResult = "google_search_result";

    /// <summary>Google Maps call step.</summary>
    public const string GoogleMapsCall = "google_maps_call";

    /// <summary>Google Maps result step.</summary>
    public const string GoogleMapsResult = "google_maps_result";

    /// <summary>File search call step.</summary>
    public const string FileSearchCall = "file_search_call";

    /// <summary>File search result step.</summary>
    public const string FileSearchResult = "file_search_result";

    /// <summary>MCP server tool call step.</summary>
    public const string McpServerToolCall = "mcp_server_tool_call";

    /// <summary>MCP server tool result step.</summary>
    public const string McpServerToolResult = "mcp_server_tool_result";

    /// <summary>Retrieval call step.</summary>
    public const string RetrievalCall = "retrieval_call";

    /// <summary>Retrieval result step.</summary>
    public const string RetrievalResult = "retrieval_result";

    /// <summary>Processing call step.</summary>
    public const string ProcessingCall = "processing_call";

    /// <summary>Processing result step.</summary>
    public const string ProcessingResult = "processing_result";

    /// <summary>Tool search call step.</summary>
    public const string ToolSearchCall = "tool_search_call";

    /// <summary>Tool search result step.</summary>
    public const string ToolSearchResult = "tool_search_result";
}

/// <summary>
/// Known values of the <c>type</c> discriminator of <see cref="InteractionContent"/>.
/// </summary>
public static class InteractionContentTypes
{
    /// <summary>Text content.</summary>
    public const string Text = "text";

    /// <summary>Image content.</summary>
    public const string Image = "image";

    /// <summary>Audio content.</summary>
    public const string Audio = "audio";

    /// <summary>Video content.</summary>
    public const string Video = "video";

    /// <summary>Document content.</summary>
    public const string Document = "document";

    /// <summary>
    /// Returns <c>true</c> when <paramref name="type"/> is one of the content types listed above.
    /// </summary>
    /// <param name="type">The discriminator value.</param>
    /// <returns><c>true</c> for a known content type.</returns>
    public static bool IsKnown(string? type)
    {
        return type is Text or Image or Audio or Video or Document;
    }
}

/// <summary>
/// Known values of the <c>event_type</c> discriminator of <see cref="InteractionSseEvent"/>.
/// </summary>
public static class InteractionEventTypes
{
    /// <summary>The interaction was created.</summary>
    public const string InteractionCreated = "interaction.created";

    /// <summary>The interaction completed.</summary>
    public const string InteractionCompleted = "interaction.completed";

    /// <summary>The interaction status changed.</summary>
    public const string InteractionStatusUpdate = "interaction.status_update";

    /// <summary>A step started.</summary>
    public const string StepStart = "step.start";

    /// <summary>Incremental data for a step.</summary>
    public const string StepDelta = "step.delta";

    /// <summary>A step finished.</summary>
    public const string StepStop = "step.stop";

    /// <summary>An error occurred.</summary>
    public const string Error = "error";

    /// <summary>Live interactions: the client (re)connected to a live interaction.</summary>
    public const string InteractionConnected = "interaction.connected";

    /// <summary>Live interactions: the interaction was cancelled.</summary>
    public const string InteractionCancelled = "interaction.cancelled";
}

/// <summary>
/// Known values of the <c>event_type</c> discriminator of <see cref="InteractionClientEvent"/> (client events of
/// live interactions).
/// </summary>
public static class InteractionClientEventTypes
{
    /// <summary>Starts a new interaction (<see cref="CreateInteractionEvent"/>).</summary>
    public const string InteractionCreate = "interaction.create";

    /// <summary>Reconnects to an existing live interaction (<see cref="ConnectInteractionEvent"/>).</summary>
    public const string InteractionConnect = "interaction.connect";

    /// <summary>Permanently ends the interaction (<see cref="CancelInteractionEvent"/>).</summary>
    public const string InteractionCancel = "interaction.cancel";

    /// <summary>Half-closes the interaction: no new input (<see cref="CompleteInteractionEvent"/>).</summary>
    public const string InteractionComplete = "interaction.complete";

    /// <summary>Gracefully disconnects without ending the interaction (<see cref="InteractionDisconnectEvent"/>).</summary>
    public const string InteractionDisconnect = "interaction.disconnect";

    /// <summary>An atomic input payload (<see cref="InteractionInputEvent"/>).</summary>
    public const string Input = "input";

    /// <summary>An interactive user action (<see cref="InteractionUserActionInput"/>).</summary>
    public const string UserAction = "user_action";

    /// <summary>World setup for world-simulation models (<see cref="InteractionWorldSetupEvent"/>).</summary>
    public const string WorldSetup = "world_setup";
}

/// <summary>
/// Known values of the <c>type</c> discriminator of <see cref="InteractionTool"/>.
/// </summary>
public static class InteractionToolTypes
{
    /// <summary>Function declaration.</summary>
    public const string Function = "function";

    /// <summary>Code execution tool.</summary>
    public const string CodeExecution = "code_execution";

    /// <summary>URL context tool.</summary>
    public const string UrlContext = "url_context";

    /// <summary>Google Search tool.</summary>
    public const string GoogleSearch = "google_search";

    /// <summary>Google Maps tool.</summary>
    public const string GoogleMaps = "google_maps";

    /// <summary>File search tool.</summary>
    public const string FileSearch = "file_search";

    /// <summary>Computer use tool.</summary>
    public const string ComputerUse = "computer_use";

    /// <summary>Remote MCP server tool.</summary>
    public const string McpServer = "mcp_server";

    /// <summary>Retrieval tool.</summary>
    public const string Retrieval = "retrieval";

    /// <summary>Tool search tool, which loads deferred tools on demand.</summary>
    public const string ToolSearch = "tool_search";
}

/// <summary>
/// Known values of the <c>type</c> discriminator of <see cref="InteractionStepDeltaData"/>.
/// </summary>
public static class InteractionDeltaTypes
{
    /// <summary>Text delta.</summary>
    public const string Text = "text";

    /// <summary>Image delta.</summary>
    public const string Image = "image";

    /// <summary>Audio delta.</summary>
    public const string Audio = "audio";

    /// <summary>Video delta.</summary>
    public const string Video = "video";

    /// <summary>Document delta.</summary>
    public const string Document = "document";

    /// <summary>Text annotation delta.</summary>
    public const string TextAnnotation = "text_annotation_delta";

    /// <summary>Function-call arguments delta.</summary>
    public const string Arguments = "arguments_delta";

    /// <summary>Thought signature delta.</summary>
    public const string ThoughtSignature = "thought_signature";

    /// <summary>Thought summary delta.</summary>
    public const string ThoughtSummary = "thought_summary";

    /// <summary>Function result delta.</summary>
    public const string FunctionResult = "function_result";

    /// <summary>Code execution call delta.</summary>
    public const string CodeExecutionCall = "code_execution_call";

    /// <summary>Code execution result delta.</summary>
    public const string CodeExecutionResult = "code_execution_result";

    /// <summary>URL context call delta.</summary>
    public const string UrlContextCall = "url_context_call";

    /// <summary>URL context result delta.</summary>
    public const string UrlContextResult = "url_context_result";

    /// <summary>Google Search call delta.</summary>
    public const string GoogleSearchCall = "google_search_call";

    /// <summary>Google Search result delta.</summary>
    public const string GoogleSearchResult = "google_search_result";

    /// <summary>Google Maps call delta.</summary>
    public const string GoogleMapsCall = "google_maps_call";

    /// <summary>Google Maps result delta.</summary>
    public const string GoogleMapsResult = "google_maps_result";

    /// <summary>File search call delta.</summary>
    public const string FileSearchCall = "file_search_call";

    /// <summary>File search result delta.</summary>
    public const string FileSearchResult = "file_search_result";

    /// <summary>MCP server tool call delta.</summary>
    public const string McpServerToolCall = "mcp_server_tool_call";

    /// <summary>MCP server tool result delta.</summary>
    public const string McpServerToolResult = "mcp_server_tool_result";

    /// <summary>Retrieval call delta.</summary>
    public const string RetrievalCall = "retrieval_call";

    /// <summary>Retrieval result delta.</summary>
    public const string RetrievalResult = "retrieval_result";

    /// <summary>Processing call delta.</summary>
    public const string ProcessingCall = "processing_call";

    /// <summary>Processing result delta.</summary>
    public const string ProcessingResult = "processing_result";

    /// <summary>Tool search call delta.</summary>
    public const string ToolSearchCall = "tool_search_call";

    /// <summary>Tool search result delta.</summary>
    public const string ToolSearchResult = "tool_search_result";
}

/// <summary>
/// Known values for <see cref="InteractionGenerationConfig.ThinkingLevel"/>.
/// </summary>
public static class InteractionThinkingLevels
{
    /// <summary>Minimal thinking.</summary>
    public const string Minimal = "minimal";

    /// <summary>Low thinking.</summary>
    public const string Low = "low";

    /// <summary>Medium thinking.</summary>
    public const string Medium = "medium";

    /// <summary>High thinking.</summary>
    public const string High = "high";

    /// <summary>Extra-high thinking.</summary>
    public const string XHigh = "xhigh";
}

/// <summary>
/// Known values for the <c>service_tier</c> field.
/// </summary>
public static class InteractionServiceTiers
{
    /// <summary>Flex tier.</summary>
    public const string Flex = "flex";

    /// <summary>Standard tier.</summary>
    public const string Standard = "standard";

    /// <summary>Priority tier.</summary>
    public const string Priority = "priority";

    /// <summary>Deferred tier.</summary>
    public const string Deferred = "deferred";
}

/// <summary>
/// Known values for response modalities (<c>response_modalities</c>, <see cref="InteractionModalityTokens.Modality"/>).
/// </summary>
public static class InteractionModalities
{
    /// <summary>Text.</summary>
    public const string Text = "text";

    /// <summary>Image.</summary>
    public const string Image = "image";

    /// <summary>Audio.</summary>
    public const string Audio = "audio";

    /// <summary>Video.</summary>
    public const string Video = "video";

    /// <summary>Document.</summary>
    public const string Document = "document";
}

/// <summary>
/// Known values for the <c>delivery</c> field of media response formats.
/// </summary>
public static class InteractionDeliveryModes
{
    /// <summary>The media bytes are returned inline (base64 in <c>data</c>).</summary>
    public const string Inline = "inline";

    /// <summary>The media is returned as a URI (in <c>uri</c>).</summary>
    public const string Uri = "uri";
}

/// <summary>
/// Known values for the <c>mode</c> of <see cref="InteractionToolChoice"/>.
/// </summary>
public static class InteractionToolChoiceModes
{
    /// <summary>The model decides whether to call tools.</summary>
    public const string Auto = "auto";

    /// <summary>The model must call a tool.</summary>
    public const string Any = "any";

    /// <summary>The model must not call tools.</summary>
    public const string None = "none";

    /// <summary>Validated tool calling.</summary>
    public const string Validated = "validated";
}

/// <summary>
/// Known values for the <c>thinking_summaries</c> field.
/// </summary>
public static class InteractionThinkingSummaries
{
    /// <summary>Thinking summaries are produced automatically.</summary>
    public const string Auto = "auto";

    /// <summary>No thinking summaries.</summary>
    public const string None = "none";
}

/// <summary>
/// Known values for the <c>execution</c> mode of <see cref="InteractionToolSearchTool"/>.
/// </summary>
public static class InteractionToolSearchExecutions
{
    /// <summary>The tool search runs on the server.</summary>
    public const string Server = "server";

    /// <summary>The tool search is executed by the client.</summary>
    public const string Client = "client";
}

/// <summary>
/// Known string values for the <c>processing</c> field of media content (video, audio, document).
/// </summary>
public static class InteractionMediaProcessingModes
{
    /// <summary>Static processing of the media.</summary>
    public const string Static = "static";

    /// <summary>Agentic processing of the media (video only).</summary>
    public const string Agentic = "agentic";
}

/// <summary>
/// Known values of <see cref="InteractionStaticMediaProcessing.ContentLayers"/> (video content layers).
/// </summary>
public static class InteractionVideoContentLayers
{
    /// <summary>Video frames.</summary>
    public const string Frames = "frames";

    /// <summary>The audio track.</summary>
    public const string Audio = "audio";

    /// <summary>The transcript of the audio track.</summary>
    public const string Transcript = "transcript";
}

/// <summary>
/// Known values of <see cref="InteractionPageProcessingOptions.ContentLayers"/>.
/// </summary>
public static class InteractionPageContentLayers
{
    /// <summary>Rendered page images.</summary>
    public const string PageImage = "page_image";

    /// <summary>Extracted page text.</summary>
    public const string PageText = "page_text";
}

/// <summary>
/// Known values of <see cref="InteractionSpreadsheetProcessingOptions.ContentLayers"/>.
/// </summary>
public static class InteractionSpreadsheetContentLayers
{
    /// <summary>The visual rendering of the sheet.</summary>
    public const string Visual = "visual";

    /// <summary>The cell values.</summary>
    public const string Data = "data";

    /// <summary>The cell formulas.</summary>
    public const string Formulas = "formulas";
}

/// <summary>
/// Known values of <see cref="InteractionUserActionInput.Action"/>.
/// </summary>
public static class InteractionUserActionTypes
{
    /// <summary>A key was pressed.</summary>
    public const string KeyDown = "key_down";

    /// <summary>A key was released.</summary>
    public const string KeyUp = "key_up";

    /// <summary>A key was clicked (pressed and released).</summary>
    public const string KeyClick = "key_click";
}

/// <summary>
/// Known values of <see cref="InteractionUserActionInput.Input"/> (the key an action targets).
/// </summary>
public static class InteractionUserActionKeys
{
    /// <summary>Arrow down.</summary>
    public const string ArrowDown = "arrow_down";

    /// <summary>Arrow left.</summary>
    public const string ArrowLeft = "arrow_left";

    /// <summary>Arrow right.</summary>
    public const string ArrowRight = "arrow_right";

    /// <summary>Arrow up.</summary>
    public const string ArrowUp = "arrow_up";

    /// <summary>The A key.</summary>
    public const string KeyA = "key_a";

    /// <summary>The D key.</summary>
    public const string KeyD = "key_d";

    /// <summary>The E key.</summary>
    public const string KeyE = "key_e";

    /// <summary>The S key.</summary>
    public const string KeyS = "key_s";

    /// <summary>The W key.</summary>
    public const string KeyW = "key_w";

    /// <summary>The space bar.</summary>
    public const string Space = "space";
}
