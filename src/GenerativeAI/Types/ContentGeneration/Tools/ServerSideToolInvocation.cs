using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A call to a server-side tool (for example Google Search or File Search) made by the model.
/// Returned when <see cref="ToolConfig.IncludeServerSideToolInvocations"/> is true.
/// </summary>
public class ToolCall
{
    /// <summary>
    /// Unique identifier of the tool call; matches <see cref="ToolResponse.Id"/>.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The type of the server-side tool that was called.
    /// </summary>
    [JsonPropertyName("toolType")]
    public ToolType? ToolType { get; set; }

    /// <summary>
    /// The arguments of the tool call.
    /// </summary>
    [JsonPropertyName("args")]
    public JsonNode? Args { get; set; }
}

/// <summary>
/// The result of a server-side tool call.
/// Returned when <see cref="ToolConfig.IncludeServerSideToolInvocations"/> is true.
/// </summary>
public class ToolResponse
{
    /// <summary>
    /// Identifier of the tool call this response belongs to.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The type of the server-side tool that produced the response.
    /// </summary>
    [JsonPropertyName("toolType")]
    public ToolType? ToolType { get; set; }

    /// <summary>
    /// The tool's response.
    /// </summary>
    [JsonPropertyName("response")]
    public JsonNode? Response { get; set; }
}

/// <summary>
/// Type of a server-side tool.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ToolType>))]
public enum ToolType
{
    /// <summary>
    /// The tool type is unspecified.
    /// </summary>
    TOOL_TYPE_UNSPECIFIED,

    /// <summary>
    /// Google Search (web results).
    /// </summary>
    GOOGLE_SEARCH_WEB,

    /// <summary>
    /// Google Search (image results).
    /// </summary>
    GOOGLE_SEARCH_IMAGE,

    /// <summary>
    /// URL context retrieval.
    /// </summary>
    URL_CONTEXT,

    /// <summary>
    /// Google Maps.
    /// </summary>
    GOOGLE_MAPS,

    /// <summary>
    /// File Search.
    /// </summary>
    FILE_SEARCH,

    /// <summary>
    /// Agentic media processing.
    /// </summary>
    MEDIA_PROCESSING
}
