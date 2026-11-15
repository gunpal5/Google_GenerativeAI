using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// A step of an interaction (user input, model output, thought, tool call or tool result), discriminated by
/// <c>type</c>. Unknown step types are read as <see cref="UnknownInteractionStep"/>.
/// </summary>
[JsonConverter(typeof(InteractionStepConverter))]
public abstract class InteractionStep
{
    /// <summary>
    /// Initializes a new step with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionStep(string type)
    {
        Type = type;
    }

    /// <summary>The step type discriminator (see <see cref="InteractionStepTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Base class for tool call steps, which carry the call <see cref="Id"/>.</summary>
public abstract class InteractionCallStep : InteractionStep
{
    /// <summary>
    /// Initializes a new call step with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionCallStep(string type) : base(type)
    {
    }

    /// <summary>Required. A unique ID for this specific tool call.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>Base class for tool result steps, which reference the call by <see cref="CallId"/>.</summary>
public abstract class InteractionResultStep : InteractionStep
{
    /// <summary>
    /// Initializes a new result step with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionResultStep(string type) : base(type)
    {
    }

    /// <summary>Required. ID matching the ID of the corresponding call step.</summary>
    [JsonPropertyName("call_id")]
    public string? CallId { get; set; }
}

/// <summary>User input (<c>user_input</c>).</summary>
public sealed class InteractionUserInputStep : InteractionStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUserInputStep"/> class.</summary>
    public InteractionUserInputStep() : base(InteractionStepTypes.UserInput)
    {
    }

    /// <summary>The user content.</summary>
    [JsonPropertyName("content")]
    public List<InteractionContent>? Content { get; set; }
}

/// <summary>Model output (<c>model_output</c>).</summary>
public sealed class InteractionModelOutputStep : InteractionStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionModelOutputStep"/> class.</summary>
    public InteractionModelOutputStep() : base(InteractionStepTypes.ModelOutput)
    {
    }

    /// <summary>The generated content (text, image, audio, video, ...).</summary>
    [JsonPropertyName("content")]
    public List<InteractionContent>? Content { get; set; }

    /// <summary>The error, if generating this output failed.</summary>
    [JsonPropertyName("error")]
    public InteractionRpcStatus? Error { get; set; }
}

/// <summary>A thought / reasoning step (<c>thought</c>).</summary>
public sealed class InteractionThoughtStep : InteractionStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionThoughtStep"/> class.</summary>
    public InteractionThoughtStep() : base(InteractionStepTypes.Thought)
    {
    }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    /// <summary>A summary of the thought (text or image content).</summary>
    [JsonPropertyName("summary")]
    public List<InteractionContent>? Summary { get; set; }
}

/// <summary>A function call requested by the model (<c>function_call</c>).</summary>
public sealed class InteractionFunctionCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFunctionCallStep"/> class.</summary>
    public InteractionFunctionCallStep() : base(InteractionStepTypes.FunctionCall)
    {
    }

    /// <summary>Required. The name of the function to call.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Required. The arguments to pass to the function.</summary>
    [JsonPropertyName("arguments")]
    public JsonObject? Arguments { get; set; }
}

/// <summary>The result of a function call, sent back by the client (<c>function_result</c>).</summary>
public sealed class InteractionFunctionResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFunctionResultStep"/> class.</summary>
    public InteractionFunctionResultStep() : base(InteractionStepTypes.FunctionResult)
    {
    }

    /// <summary>The name of the tool that was called.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Required. The result of the tool call: a JSON object, a string, or an array of content blocks.
    /// </summary>
    [JsonPropertyName("result")]
    public JsonNode? Result { get; set; }

    /// <summary>Whether the tool call resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }
}

/// <summary>Arguments of a code execution call.</summary>
public sealed class InteractionCodeExecutionArguments
{
    /// <summary>The code to be executed.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>Programming language of the code (<c>python</c>, <c>bash</c>).</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

/// <summary>A code execution call (<c>code_execution_call</c>).</summary>
public sealed class InteractionCodeExecutionCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCodeExecutionCallStep"/> class.</summary>
    public InteractionCodeExecutionCallStep() : base(InteractionStepTypes.CodeExecutionCall)
    {
    }

    /// <summary>The arguments to pass to the code execution.</summary>
    [JsonPropertyName("arguments")]
    public InteractionCodeExecutionArguments? Arguments { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>The result of a code execution (<c>code_execution_result</c>).</summary>
public sealed class InteractionCodeExecutionResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCodeExecutionResultStep"/> class.</summary>
    public InteractionCodeExecutionResultStep() : base(InteractionStepTypes.CodeExecutionResult)
    {
    }

    /// <summary>The output of the code execution: a string or an array of content blocks.</summary>
    [JsonPropertyName("result")]
    public JsonNode? Result { get; set; }

    /// <summary>Whether the code execution resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>Arguments of a URL context call.</summary>
public sealed class InteractionUrlContextArguments
{
    /// <summary>The URLs to fetch.</summary>
    [JsonPropertyName("urls")]
    public List<string>? Urls { get; set; }
}

/// <summary>A URL context call (<c>url_context_call</c>).</summary>
public sealed class InteractionUrlContextCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUrlContextCallStep"/> class.</summary>
    public InteractionUrlContextCallStep() : base(InteractionStepTypes.UrlContextCall)
    {
    }

    /// <summary>The arguments to pass to the URL context tool.</summary>
    [JsonPropertyName("arguments")]
    public InteractionUrlContextArguments? Arguments { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>One fetched URL of a URL context result.</summary>
public sealed class InteractionUrlContextResult
{
    /// <summary>The status of the URL retrieval (<c>success</c>, <c>error</c>, <c>paywall</c>, <c>unsafe</c>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>The URL that was fetched.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>The result of a URL context call (<c>url_context_result</c>).</summary>
public sealed class InteractionUrlContextResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUrlContextResultStep"/> class.</summary>
    public InteractionUrlContextResultStep() : base(InteractionStepTypes.UrlContextResult)
    {
    }

    /// <summary>Required. The results of the URL context.</summary>
    [JsonPropertyName("result")]
    public List<InteractionUrlContextResult>? Result { get; set; }

    /// <summary>Whether the URL context resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>Query arguments shared by the Google Search, Google Maps and Retrieval calls.</summary>
public sealed class InteractionQueryArguments
{
    /// <summary>The queries to be executed.</summary>
    [JsonPropertyName("queries")]
    public List<string>? Queries { get; set; }
}

/// <summary>A Google Search call (<c>google_search_call</c>).</summary>
public sealed class InteractionGoogleSearchCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleSearchCallStep"/> class.</summary>
    public InteractionGoogleSearchCallStep() : base(InteractionStepTypes.GoogleSearchCall)
    {
    }

    /// <summary>The arguments to pass to Google Search.</summary>
    [JsonPropertyName("arguments")]
    public InteractionQueryArguments? Arguments { get; set; }

    /// <summary>The type of search grounding enabled (<c>web_search</c>, <c>image_search</c>, <c>enterprise_web_search</c>).</summary>
    [JsonPropertyName("search_type")]
    public string? SearchType { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>One Google Search result.</summary>
public sealed class InteractionGoogleSearchResult
{
    /// <summary>Web content snippet that can be embedded in a web page or an app webview.</summary>
    [JsonPropertyName("search_suggestions")]
    public string? SearchSuggestions { get; set; }
}

/// <summary>The result of a Google Search call (<c>google_search_result</c>).</summary>
public sealed class InteractionGoogleSearchResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleSearchResultStep"/> class.</summary>
    public InteractionGoogleSearchResultStep() : base(InteractionStepTypes.GoogleSearchResult)
    {
    }

    /// <summary>Required. The results of the Google Search.</summary>
    [JsonPropertyName("result")]
    public List<InteractionGoogleSearchResult>? Result { get; set; }

    /// <summary>Whether the Google Search resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>A Google Maps call (<c>google_maps_call</c>).</summary>
public sealed class InteractionGoogleMapsCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleMapsCallStep"/> class.</summary>
    public InteractionGoogleMapsCallStep() : base(InteractionStepTypes.GoogleMapsCall)
    {
    }

    /// <summary>The arguments to pass to the Google Maps tool.</summary>
    [JsonPropertyName("arguments")]
    public InteractionQueryArguments? Arguments { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>A place returned by Google Maps.</summary>
public sealed class InteractionGoogleMapsPlace
{
    /// <summary>The name of the place.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The ID of the place.</summary>
    [JsonPropertyName("place_id")]
    public string? PlaceId { get; set; }

    /// <summary>Review snippets of the place.</summary>
    [JsonPropertyName("review_snippets")]
    public List<InteractionReviewSnippet>? ReviewSnippets { get; set; }

    /// <summary>The URL of the place.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>One Google Maps result.</summary>
public sealed class InteractionGoogleMapsResult
{
    /// <summary>The places found.</summary>
    [JsonPropertyName("places")]
    public List<InteractionGoogleMapsPlace>? Places { get; set; }

    /// <summary>A widget context token, when the widget was requested.</summary>
    [JsonPropertyName("widget_context_token")]
    public string? WidgetContextToken { get; set; }
}

/// <summary>The result of a Google Maps call (<c>google_maps_result</c>).</summary>
public sealed class InteractionGoogleMapsResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleMapsResultStep"/> class.</summary>
    public InteractionGoogleMapsResultStep() : base(InteractionStepTypes.GoogleMapsResult)
    {
    }

    /// <summary>The results of the Google Maps call.</summary>
    [JsonPropertyName("result")]
    public List<InteractionGoogleMapsResult>? Result { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>A file search call (<c>file_search_call</c>).</summary>
public sealed class InteractionFileSearchCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFileSearchCallStep"/> class.</summary>
    public InteractionFileSearchCallStep() : base(InteractionStepTypes.FileSearchCall)
    {
    }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>The result of a file search call (<c>file_search_result</c>).</summary>
public sealed class InteractionFileSearchResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFileSearchResultStep"/> class.</summary>
    public InteractionFileSearchResultStep() : base(InteractionStepTypes.FileSearchResult)
    {
    }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>A call to a tool of a remote MCP server (<c>mcp_server_tool_call</c>).</summary>
public sealed class InteractionMcpServerToolCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionMcpServerToolCallStep"/> class.</summary>
    public InteractionMcpServerToolCallStep() : base(InteractionStepTypes.McpServerToolCall)
    {
    }

    /// <summary>Required. The name of the tool which was called.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Required. The name of the MCP server.</summary>
    [JsonPropertyName("server_name")]
    public string? ServerName { get; set; }

    /// <summary>Required. The JSON object of arguments for the tool.</summary>
    [JsonPropertyName("arguments")]
    public JsonObject? Arguments { get; set; }
}

/// <summary>The result of an MCP server tool call (<c>mcp_server_tool_result</c>).</summary>
public sealed class InteractionMcpServerToolResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionMcpServerToolResultStep"/> class.</summary>
    public InteractionMcpServerToolResultStep() : base(InteractionStepTypes.McpServerToolResult)
    {
    }

    /// <summary>Name of the tool which was called.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The name of the MCP server.</summary>
    [JsonPropertyName("server_name")]
    public string? ServerName { get; set; }

    /// <summary>Required. The output of the MCP server call: a JSON object, a string, or an array of content blocks.</summary>
    [JsonPropertyName("result")]
    public JsonNode? Result { get; set; }
}

/// <summary>A retrieval call (<c>retrieval_call</c>).</summary>
public sealed class InteractionRetrievalCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionRetrievalCallStep"/> class.</summary>
    public InteractionRetrievalCallStep() : base(InteractionStepTypes.RetrievalCall)
    {
    }

    /// <summary>The arguments to pass to the retrieval tool.</summary>
    [JsonPropertyName("arguments")]
    public InteractionQueryArguments? Arguments { get; set; }

    /// <summary>The type of retrieval (<c>vertex_ai_search</c>, <c>rag_store</c>, <c>exa_ai_search</c>, <c>parallel_ai_search</c>).</summary>
    [JsonPropertyName("retrieval_type")]
    public string? RetrievalType { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>The result of a retrieval call (<c>retrieval_result</c>).</summary>
public sealed class InteractionRetrievalResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionRetrievalResultStep"/> class.</summary>
    public InteractionRetrievalResultStep() : base(InteractionStepTypes.RetrievalResult)
    {
    }

    /// <summary>Whether the retrieval resulted in an error.</summary>
    [JsonPropertyName("is_error")]
    public bool? IsError { get; set; }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>A processing call, e.g. media processing (<c>processing_call</c>).</summary>
public sealed class InteractionProcessingCallStep : InteractionCallStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionProcessingCallStep"/> class.</summary>
    public InteractionProcessingCallStep() : base(InteractionStepTypes.ProcessingCall)
    {
    }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>The result of a processing call (<c>processing_result</c>).</summary>
public sealed class InteractionProcessingResultStep : InteractionResultStep
{
    /// <summary>Initializes a new instance of the <see cref="InteractionProcessingResultStep"/> class.</summary>
    public InteractionProcessingResultStep() : base(InteractionStepTypes.ProcessingResult)
    {
    }

    /// <summary>A signature hash for backend validation (base64).</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>
/// A step whose <c>type</c> this SDK does not know. <see cref="Raw"/> keeps the original JSON and is written back
/// unchanged (for example when the step is passed back as input).
/// </summary>
public sealed class UnknownInteractionStep : InteractionStep
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionStep"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionStep(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the step.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}
