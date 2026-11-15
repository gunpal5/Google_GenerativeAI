using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A remote MCP server that the model calls server-side.
/// For client-side MCP tools see <c>GenerativeAI.Tools.Mcp.McpTool</c>.
/// </summary>
public class McpServer
{
    /// <summary>
    /// Name of the MCP server.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Streamable HTTP transport used to reach the server.
    /// </summary>
    [JsonPropertyName("streamableHttpTransport")]
    public StreamableHttpTransport? StreamableHttpTransport { get; set; }
}

/// <summary>
/// Streamable HTTP transport settings for a server-side MCP server.
/// </summary>
public class StreamableHttpTransport
{
    /// <summary>
    /// The URL of the MCP endpoint.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// HTTP headers sent with each request, for example authentication headers.
    /// </summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// HTTP request timeout, as a duration string (for example "30s").
    /// </summary>
    [JsonPropertyName("timeout")]
    public string? Timeout { get; set; }

    /// <summary>
    /// Timeout for reading server-sent events, as a duration string (for example "300s").
    /// </summary>
    [JsonPropertyName("sseReadTimeout")]
    public string? SseReadTimeout { get; set; }

    /// <summary>
    /// Whether to close the session when the transport is closed.
    /// </summary>
    [JsonPropertyName("terminateOnClose")]
    public bool? TerminateOnClose { get; set; }
}
