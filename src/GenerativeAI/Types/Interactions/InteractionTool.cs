using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// A tool the model may use during an interaction, discriminated by <c>type</c>.
/// Unknown tool types are read as <see cref="UnknownInteractionTool"/>.
/// </summary>
[JsonConverter(typeof(InteractionToolConverter))]
public abstract class InteractionTool
{
    /// <summary>
    /// Initializes a new tool with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionTool(string type)
    {
        Type = type;
    }

    /// <summary>The tool type discriminator (see <see cref="InteractionToolTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>A function declaration (<c>function</c>).</summary>
public sealed class InteractionFunctionTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFunctionTool"/> class.</summary>
    public InteractionFunctionTool() : base(InteractionToolTypes.Function)
    {
    }

    /// <summary>The name of the function.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>A description of the function.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The JSON Schema of the function's parameters.</summary>
    [JsonPropertyName("parameters")]
    public JsonNode? Parameters { get; set; }

    /// <summary>
    /// If true, loading of the function is deferred: the model only sees <see cref="ShortDescription"/> and loads
    /// the full declaration on demand with the <see cref="InteractionToolSearchTool"/>.
    /// </summary>
    [JsonPropertyName("defer_loading")]
    public bool? DeferLoading { get; set; }

    /// <summary>
    /// A brief description of the function, shown to the model as a short summary of functions with
    /// <see cref="DeferLoading"/> set to true.
    /// </summary>
    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }
}

/// <summary>The code execution tool (<c>code_execution</c>).</summary>
public sealed class InteractionCodeExecutionTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCodeExecutionTool"/> class.</summary>
    public InteractionCodeExecutionTool() : base(InteractionToolTypes.CodeExecution)
    {
    }
}

/// <summary>The URL context tool (<c>url_context</c>).</summary>
public sealed class InteractionUrlContextTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUrlContextTool"/> class.</summary>
    public InteractionUrlContextTool() : base(InteractionToolTypes.UrlContext)
    {
    }
}

/// <summary>The Google Search grounding tool (<c>google_search</c>).</summary>
public sealed class InteractionGoogleSearchTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleSearchTool"/> class.</summary>
    public InteractionGoogleSearchTool() : base(InteractionToolTypes.GoogleSearch)
    {
    }

    /// <summary>The types of search grounding to enable (<c>web_search</c>, <c>image_search</c>, <c>enterprise_web_search</c>).</summary>
    [JsonPropertyName("search_types")]
    public List<string>? SearchTypes { get; set; }
}

/// <summary>The Google Maps grounding tool (<c>google_maps</c>).</summary>
public sealed class InteractionGoogleMapsTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionGoogleMapsTool"/> class.</summary>
    public InteractionGoogleMapsTool() : base(InteractionToolTypes.GoogleMaps)
    {
    }

    /// <summary>Whether to return a widget context token in the tool call result.</summary>
    [JsonPropertyName("enable_widget")]
    public bool? EnableWidget { get; set; }

    /// <summary>The latitude of the user's location.</summary>
    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    /// <summary>The longitude of the user's location.</summary>
    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}

/// <summary>The file search tool (<c>file_search</c>).</summary>
public sealed class InteractionFileSearchTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFileSearchTool"/> class.</summary>
    public InteractionFileSearchTool() : base(InteractionToolTypes.FileSearch)
    {
    }

    /// <summary>The file search store names to search.</summary>
    [JsonPropertyName("file_search_store_names")]
    public List<string>? FileSearchStoreNames { get; set; }

    /// <summary>Metadata filter to apply to the semantic retrieval documents and chunks.</summary>
    [JsonPropertyName("metadata_filter")]
    public string? MetadataFilter { get; set; }

    /// <summary>The number of semantic retrieval chunks to retrieve.</summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }
}

/// <summary>The computer use tool (<c>computer_use</c>).</summary>
public sealed class InteractionComputerUseTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionComputerUseTool"/> class.</summary>
    public InteractionComputerUseTool() : base(InteractionToolTypes.ComputerUse)
    {
    }

    /// <summary>The environment being operated (<c>browser</c>, <c>mobile</c>, <c>desktop</c>).</summary>
    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    /// <summary>Disabled safety policies (e.g. <c>financial_transactions</c>).</summary>
    [JsonPropertyName("disabled_safety_policies")]
    public List<string>? DisabledSafetyPolicies { get; set; }

    /// <summary>Whether to enable the prompt injection detection check.</summary>
    [JsonPropertyName("enable_prompt_injection_detection")]
    public bool? EnablePromptInjectionDetection { get; set; }

    /// <summary>Predefined functions that are excluded from the model call.</summary>
    [JsonPropertyName("excluded_predefined_functions")]
    public List<string>? ExcludedPredefinedFunctions { get; set; }
}

/// <summary>Tools allowed for an MCP server or a tool choice.</summary>
public sealed class InteractionAllowedTools
{
    /// <summary>The tool choice mode (see <see cref="InteractionToolChoiceModes"/>).</summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    /// <summary>The names of the allowed tools.</summary>
    [JsonPropertyName("tools")]
    public List<string>? Tools { get; set; }
}

/// <summary>A remote MCP server whose tools the model may call (<c>mcp_server</c>).</summary>
public sealed class InteractionMcpServerTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionMcpServerTool"/> class.</summary>
    public InteractionMcpServerTool() : base(InteractionToolTypes.McpServer)
    {
    }

    /// <summary>The name of the MCP server.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The full URL of the MCP server endpoint.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Headers to send to the server (authentication, etc.).</summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>The allowed tools.</summary>
    [JsonPropertyName("allowed_tools")]
    public List<InteractionAllowedTools>? AllowedTools { get; set; }

    /// <summary>If true, loading of the tools of this MCP server is deferred (see <see cref="InteractionToolSearchTool"/>).</summary>
    [JsonPropertyName("defer_loading")]
    public bool? DeferLoading { get; set; }
}

/// <summary>
/// A tool that lets the model search for and load tools into its context on demand (<c>tool_search</c>), so clients
/// don't have to load every tool definition up front (see <see cref="InteractionFunctionTool.DeferLoading"/>).
/// </summary>
public sealed class InteractionToolSearchTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionToolSearchTool"/> class.</summary>
    public InteractionToolSearchTool() : base(InteractionToolTypes.ToolSearch)
    {
    }

    /// <summary>The execution mode of the tool search (see <see cref="InteractionToolSearchExecutions"/>).</summary>
    [JsonPropertyName("execution")]
    public string? Execution { get; set; }

    /// <summary>The name of the function. Set only for client-side execution.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>A description of the function. Set only for client-side execution.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The JSON Schema of the function's parameters. Set only for client-side execution.</summary>
    [JsonPropertyName("parameters")]
    public JsonNode? Parameters { get; set; }
}

/// <summary>Vertex AI Search configuration for the retrieval tool.</summary>
public sealed class InteractionVertexAISearchConfig
{
    /// <summary>Vertex AI Search datastores.</summary>
    [JsonPropertyName("datastores")]
    public List<string>? Datastores { get; set; }

    /// <summary>Vertex AI Search engine.</summary>
    [JsonPropertyName("engine")]
    public string? Engine { get; set; }
}

/// <summary>Configuration of a third-party search provider (Exa.ai or Parallel.ai) for the retrieval tool.</summary>
public sealed class InteractionExternalSearchConfig
{
    /// <summary>The API key of the provider.</summary>
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }

    /// <summary>Provider-specific parameters.</summary>
    [JsonPropertyName("custom_config")]
    public JsonObject? CustomConfig { get; set; }
}

/// <summary>The retrieval tool (<c>retrieval</c>).</summary>
public sealed class InteractionRetrievalTool : InteractionTool
{
    /// <summary>Initializes a new instance of the <see cref="InteractionRetrievalTool"/> class.</summary>
    public InteractionRetrievalTool() : base(InteractionToolTypes.Retrieval)
    {
    }

    /// <summary>The types of retrieval to enable (<c>vertex_ai_search</c>, <c>rag_store</c>, <c>exa_ai_search</c>, <c>parallel_ai_search</c>).</summary>
    [JsonPropertyName("retrieval_types")]
    public List<string>? RetrievalTypes { get; set; }

    /// <summary>Vertex AI Search configuration.</summary>
    [JsonPropertyName("vertex_ai_search_config")]
    public InteractionVertexAISearchConfig? VertexAISearchConfig { get; set; }

    /// <summary>Exa.ai search configuration.</summary>
    [JsonPropertyName("exa_ai_search_config")]
    public InteractionExternalSearchConfig? ExaAISearchConfig { get; set; }

    /// <summary>Parallel.ai search configuration.</summary>
    [JsonPropertyName("parallel_ai_search_config")]
    public InteractionExternalSearchConfig? ParallelAISearchConfig { get; set; }

    /// <summary>
    /// RAG store configuration (<c>rag_resources</c>, <c>rag_retrieval_config</c>, ...), passed through as JSON.
    /// </summary>
    [JsonPropertyName("rag_store_config")]
    public JsonObject? RagStoreConfig { get; set; }
}

/// <summary>A tool whose <c>type</c> this SDK does not know.</summary>
public sealed class UnknownInteractionTool : InteractionTool
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionTool"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionTool(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the tool.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}
