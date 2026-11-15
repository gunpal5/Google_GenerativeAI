using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ModelContextProtocol.Client;

namespace GenerativeAI.Tools.Mcp;

/// <summary>
/// Configuration options for the McpTool.
/// </summary>
public class McpToolOptions
{
    /// <summary>
    /// Gets or sets the timeout for connecting to the MCP server (in milliseconds).
    /// Default is 30000ms (30 seconds).
    /// </summary>
    public int ConnectionTimeoutMs { get; set; } = 30000;

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
    public bool ThrowOnToolCallFailure { get; set; }

    /// <summary>
    /// Gets or sets whether to include detailed error information in function responses.
    /// Default is true.
    /// </summary>
    public bool IncludeDetailedErrors { get; set; } = true;
}

/// <summary>
/// Factory class for creating MCP client transports with common configurations.
/// Supports stdio, HTTP/SSE, and other transport protocols.
/// </summary>
public static class McpTransportFactory
{
    /// <summary>
    /// Creates a stdio transport for launching an MCP server as a subprocess.
    /// </summary>
    /// <param name="name">The name of the MCP server.</param>
    /// <param name="command">The command to execute (e.g., "npx", "python", "node").</param>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="environmentVariables">Optional environment variables.</param>
    /// <param name="workingDirectory">Optional working directory.</param>
    /// <returns>A configured StdioClientTransport.</returns>
    public static IClientTransport CreateStdioTransport(
        string name,
        string command,
        IEnumerable<string> arguments,
        IDictionary<string, string>? environmentVariables = null,
        string? workingDirectory = null)
    {
        var options = new StdioClientTransportOptions
        {
            Name = name,
            Command = command,
            Arguments = new List<string>(arguments),
            EnvironmentVariables = environmentVariables != null
                ? environmentVariables.ToDictionary(kv => kv.Key, kv => (string?)kv.Value)
                : null,
            WorkingDirectory = workingDirectory
        };

        return new StdioClientTransport(options);
    }

    /// <summary>
    /// Creates an HTTP/SSE transport for connecting to a remote MCP server via HTTP.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MCP server (e.g., "http://localhost:8080").</param>
    /// <param name="httpClient">Optional custom HttpClient instance.</param>
    /// <param name="additionalHeaders">Optional additional HTTP headers.</param>
    /// <returns>A configured HttpClientTransport.</returns>
#pragma warning disable CA1054 // Convenience overload; the Uri overload is the primary API
    public static IClientTransport CreateHttpTransport(
        string baseUrl,
#pragma warning restore CA1054
        HttpClient? httpClient = null,
        IDictionary<string, string>? additionalHeaders = null)
        => CreateHttpTransport(new Uri(baseUrl), httpClient, additionalHeaders);

    /// <summary>
    /// Creates an HTTP/SSE transport for connecting to a remote MCP server via HTTP.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MCP server (e.g., "http://localhost:8080").</param>
    /// <param name="httpClient">Optional custom HttpClient instance.</param>
    /// <param name="additionalHeaders">Optional additional HTTP headers.</param>
    /// <returns>A configured HttpClientTransport.</returns>
    public static IClientTransport CreateHttpTransport(
        Uri baseUrl,
        HttpClient? httpClient = null,
        IDictionary<string, string>? additionalHeaders = null)
    {
        var options = new HttpClientTransportOptions
        {
            Endpoint = baseUrl,
            AdditionalHeaders = additionalHeaders != null
                ? new Dictionary<string, string>(additionalHeaders)
                : null
        };

        return httpClient != null
            ? new HttpClientTransport(options, httpClient)
            : new HttpClientTransport(options);
    }

    /// <summary>
    /// Creates an HTTP/SSE transport with authentication headers.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MCP server.</param>
    /// <param name="authToken">Authentication token (will be sent as "Authorization: Bearer {token}").</param>
    /// <param name="httpClient">Optional custom HttpClient instance.</param>
    /// <returns>A configured HttpClientTransport with authentication.</returns>
#pragma warning disable CA1054 // Convenience overload; the Uri overload is the primary API
    public static IClientTransport CreateHttpTransportWithAuth(
        string baseUrl,
#pragma warning restore CA1054
        string authToken,
        HttpClient? httpClient = null)
        => CreateHttpTransportWithAuth(new Uri(baseUrl), authToken, httpClient);

    /// <summary>
    /// Creates an HTTP/SSE transport with authentication headers.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MCP server.</param>
    /// <param name="authToken">Authentication token (will be sent as "Authorization: Bearer {token}").</param>
    /// <param name="httpClient">Optional custom HttpClient instance.</param>
    /// <returns>A configured HttpClientTransport with authentication.</returns>
    public static IClientTransport CreateHttpTransportWithAuth(
        Uri baseUrl,
        string authToken,
        HttpClient? httpClient = null)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {authToken}" }
        };

        return CreateHttpTransport(baseUrl, httpClient, headers);
    }

    /// <summary>
    /// Creates an HTTP/SSE transport with custom headers.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MCP server.</param>
    /// <param name="headers">Custom HTTP headers.</param>
    /// <param name="httpClient">Optional custom HttpClient instance.</param>
    /// <returns>A configured HttpClientTransport with custom headers.</returns>
#pragma warning disable CA1054 // Convenience overload; the Uri overload is the primary API
    public static IClientTransport CreateHttpTransportWithHeaders(
        string baseUrl,
#pragma warning restore CA1054
        IDictionary<string, string> headers,
        HttpClient? httpClient = null)
        => CreateHttpTransportWithHeaders(new Uri(baseUrl), headers, httpClient);

    /// <summary>
    /// Creates an HTTP/SSE transport with custom headers.
    /// </summary>
    /// <param name="baseUrl">The base URL of the MCP server.</param>
    /// <param name="headers">Custom HTTP headers.</param>
    /// <param name="httpClient">Optional custom HttpClient instance.</param>
    /// <returns>A configured HttpClientTransport with custom headers.</returns>
    public static IClientTransport CreateHttpTransportWithHeaders(
        Uri baseUrl,
        IDictionary<string, string> headers,
        HttpClient? httpClient = null)
        => CreateHttpTransport(baseUrl, httpClient, headers);
}
