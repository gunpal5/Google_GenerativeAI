using System.Collections.Generic;

namespace GenerativeAI.Tools.Mcp;

/// <summary>
/// Configuration for connecting to an MCP (Model Context Protocol) server.
/// </summary>
public class McpServerConfig
{
    /// <summary>
    /// Gets or sets the name of the MCP server for identification purposes.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command to execute for starting the MCP server process (e.g., "npx", "node", "python").
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the command-line arguments to pass to the MCP server process.
    /// </summary>
    public List<string> Arguments { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets optional environment variables to set for the MCP server process.
    /// </summary>
    public Dictionary<string, string>? Environment { get; set; }

    /// <summary>
    /// Gets or sets the working directory for the MCP server process.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Gets or sets the timeout for connecting to the MCP server (in milliseconds).
    /// Default is 30000ms (30 seconds).
    /// </summary>
    public int ConnectionTimeoutMs { get; set; } = 30000;
}

/// <summary>
/// Configuration options for the McpTool.
/// </summary>
public class McpToolOptions
{
    /// <summary>
    /// Gets or sets whether to automatically reconnect to the MCP server if the connection is lost.
    /// Default is true.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of reconnection attempts.
    /// Default is 3.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to throw exceptions on tool call failures.
    /// If false, failures will be returned as error responses.
    /// Default is false.
    /// </summary>
    public bool ThrowOnToolCallFailure { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include detailed error information in function responses.
    /// Default is true.
    /// </summary>
    public bool IncludeDetailedErrors { get; set; } = true;
}
