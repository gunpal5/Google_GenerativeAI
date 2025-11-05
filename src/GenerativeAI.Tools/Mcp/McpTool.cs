using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Types;
using ModelContextProtocol;
using ModelContextProtocol.Transports;

namespace GenerativeAI.Tools.Mcp;

/// <summary>
/// A tool implementation that integrates MCP (Model Context Protocol) servers with Google Generative AI.
/// This allows you to expose tools from any MCP server to Gemini models for function calling.
/// Supports all MCP transport protocols: stdio, HTTP/SSE, and custom transports.
/// </summary>
/// <remarks>
/// <para>
/// McpTool connects to an MCP server via the official C# SDK and automatically discovers
/// and exposes all available tools from that server. When the model requests to call a function,
/// McpTool forwards the request to the MCP server and returns the response.
/// </para>
/// <para>
/// Example usage with stdio transport:
/// <code>
/// var transport = McpTransportFactory.CreateStdioTransport(
///     "my-server",
///     "npx",
///     new[] { "-y", "@modelcontextprotocol/server-everything" }
/// );
///
/// using var mcpTool = await McpTool.CreateAsync(transport);
///
/// var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
/// model.AddFunctionTool(mcpTool);
///
/// var result = await model.GenerateContentAsync("Your query here");
/// </code>
/// </para>
/// <para>
/// Example usage with HTTP transport:
/// <code>
/// var transport = McpTransportFactory.CreateHttpTransport("http://localhost:8080");
///
/// using var mcpTool = await McpTool.CreateAsync(transport);
/// // Use as above
/// </code>
/// </para>
/// </remarks>
public class McpTool : GoogleFunctionTool, IDisposable, IAsyncDisposable
{
    private readonly Func<IClientTransport>? _transportFactory;
    private readonly McpToolOptions _options;
    private McpClient? _client;
    private IClientTransport? _transport;
    private List<FunctionDeclaration> _functionDeclarations;
    private Dictionary<string, ModelContextProtocol.Protocol.Types.Tool> _mcpTools;
    private bool _disposed;
    private int _reconnectAttempts;
    private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Gets the MCP client instance. May be null if not connected.
    /// </summary>
    public McpClient? Client => _client;

    /// <summary>
    /// Gets whether the tool is currently connected to the MCP server.
    /// </summary>
    public bool IsConnected => _client != null && _transport != null;

    private McpTool(IClientTransport transport, McpToolOptions? options = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _options = options ?? new McpToolOptions();
        _functionDeclarations = new List<FunctionDeclaration>();
        _mcpTools = new Dictionary<string, ModelContextProtocol.Protocol.Types.Tool>();
        _transportFactory = null;
    }

    private McpTool(Func<IClientTransport> transportFactory, McpToolOptions? options = null)
    {
        _transportFactory = transportFactory ?? throw new ArgumentNullException(nameof(transportFactory));
        _options = options ?? new McpToolOptions();
        _functionDeclarations = new List<FunctionDeclaration>();
        _mcpTools = new Dictionary<string, ModelContextProtocol.Protocol.Types.Tool>();
    }

    /// <summary>
    /// Creates a new McpTool instance and connects using the provided transport.
    /// Supports all MCP transport types: stdio, HTTP/SSE, etc.
    /// </summary>
    /// <param name="transport">The MCP client transport (stdio, HTTP, etc.).</param>
    /// <param name="options">Optional configuration options for the tool behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A connected McpTool instance ready to use.</returns>
    /// <exception cref="ArgumentNullException">Thrown when transport is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when unable to connect to the MCP server.</exception>
    public static async Task<McpTool> CreateAsync(
        IClientTransport transport,
        McpToolOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var tool = new McpTool(transport, options);
        await tool.ConnectAsync(cancellationToken).ConfigureAwait(false);
        return tool;
    }

    /// <summary>
    /// Creates a new McpTool instance with a transport factory for auto-reconnection support.
    /// The factory will be called to create a new transport when reconnection is needed.
    /// </summary>
    /// <param name="transportFactory">Factory function that creates a new transport instance.</param>
    /// <param name="options">Optional configuration options for the tool behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A connected McpTool instance ready to use.</returns>
    public static async Task<McpTool> CreateAsync(
        Func<IClientTransport> transportFactory,
        McpToolOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var tool = new McpTool(transportFactory, options);
        tool._transport = transportFactory();
        await tool.ConnectAsync(cancellationToken).ConfigureAwait(false);
        return tool;
    }

    /// <summary>
    /// Creates multiple McpTool instances from a list of transports.
    /// </summary>
    /// <param name="transports">List of MCP client transports.</param>
    /// <param name="options">Optional configuration options for the tool behavior.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A list of connected McpTool instances.</returns>
    public static async Task<List<McpTool>> CreateMultipleAsync(
        IEnumerable<IClientTransport> transports,
        McpToolOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = transports.Select(transport => CreateAsync(transport, options, cancellationToken));
        var tools = await Task.WhenAll(tasks).ConfigureAwait(false);
        return tools.ToList();
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_transport == null)
                throw new InvalidOperationException("Transport is not initialized");

            // Create MCP client
            using var timeoutCts = new CancellationTokenSource(_options.ConnectionTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            _client = await McpClient.CreateAsync(_transport, linkedCts.Token).ConfigureAwait(false);

            // Discover available tools
            await RefreshToolsAsync(cancellationToken).ConfigureAwait(false);

            _reconnectAttempts = 0;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Refreshes the list of available tools from the MCP server.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    public async Task RefreshToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_client == null)
            throw new InvalidOperationException("MCP client is not connected. Call ConnectAsync first.");

        var tools = await _client.ListToolsAsync(cancellationToken).ConfigureAwait(false);

        _mcpTools.Clear();
        _functionDeclarations.Clear();

        foreach (var tool in tools)
        {
            _mcpTools[tool.Name] = tool;
            _functionDeclarations.Add(ConvertMcpToolToFunctionDeclaration(tool));
        }
    }

    private FunctionDeclaration ConvertMcpToolToFunctionDeclaration(ModelContextProtocol.Protocol.Types.Tool mcpTool)
    {
        var declaration = new FunctionDeclaration
        {
            Name = mcpTool.Name,
            Description = mcpTool.Description ?? string.Empty
        };

        // Convert MCP tool's input schema to Google's Schema format
        if (mcpTool.InputSchema != null)
        {
            declaration.ParametersJsonSchema = JsonNode.Parse(mcpTool.InputSchema.ToJsonString());
        }

        return declaration;
    }

    /// <inheritdoc/>
    public override Tool AsTool()
    {
        return new Tool
        {
            FunctionDeclarations = _functionDeclarations
        };
    }

    /// <inheritdoc/>
    public override async Task<FunctionResponse?> CallAsync(
        FunctionCall functionCall,
        CancellationToken cancellationToken = default)
    {
        if (functionCall == null)
            throw new ArgumentNullException(nameof(functionCall));

        if (!IsContainFunction(functionCall.Name))
        {
            throw new ArgumentException($"Function '{functionCall.Name}' is not available in this MCP server.");
        }

        try
        {
            // Ensure we're connected
            if (!IsConnected && _options.AutoReconnect)
            {
                await TryReconnectAsync(cancellationToken).ConfigureAwait(false);
            }

            if (_client == null)
            {
                throw new InvalidOperationException("MCP client is not connected.");
            }

            // Convert arguments to dictionary
            var arguments = new Dictionary<string, object?>();
            if (functionCall.Args != null)
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(functionCall.Args.ToJsonString());
                foreach (var property in jsonElement.EnumerateObject())
                {
                    arguments[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText());
                }
            }

            // Call the MCP tool
            var result = await _client.CallToolAsync(
                functionCall.Name,
                arguments,
                cancellationToken
            ).ConfigureAwait(false);

            // Convert MCP response to FunctionResponse
            var responseNode = new JsonObject
            {
                ["name"] = functionCall.Name
            };

            // MCP returns content as a list of content items
            if (result.Content != null && result.Content.Any())
            {
                var contentArray = new JsonArray();
                foreach (var content in result.Content)
                {
                    var contentObj = new JsonObject
                    {
                        ["type"] = content.Type
                    };

                    if (content.Type == "text" && !string.IsNullOrEmpty(content.Text))
                    {
                        contentObj["text"] = content.Text;
                    }
                    else if (content.Type == "image" && !string.IsNullOrEmpty(content.Data))
                    {
                        contentObj["data"] = content.Data;
                        contentObj["mimeType"] = content.MimeType;
                    }
                    else if (content.Type == "resource" && content.Resource != null)
                    {
                        contentObj["resource"] = JsonNode.Parse(JsonSerializer.Serialize(content.Resource));
                    }

                    contentArray.Add(contentObj);
                }
                responseNode["content"] = contentArray;
            }
            else
            {
                responseNode["content"] = string.Empty;
            }

            // Include error information if present
            if (result.IsError)
            {
                responseNode["isError"] = true;
                if (_options.IncludeDetailedErrors)
                {
                    responseNode["error"] = "Tool execution resulted in an error";
                }
            }

            return new FunctionResponse
            {
                Id = functionCall.Id,
                Name = functionCall.Name,
                Response = responseNode
            };
        }
        catch (Exception ex)
        {
            if (_options.ThrowOnToolCallFailure)
            {
                throw;
            }

            // Return error as function response
            var errorResponse = new JsonObject
            {
                ["name"] = functionCall.Name,
                ["error"] = _options.IncludeDetailedErrors
                    ? $"{ex.GetType().Name}: {ex.Message}"
                    : "Tool execution failed"
            };

            return new FunctionResponse
            {
                Id = functionCall.Id,
                Name = functionCall.Name,
                Response = errorResponse
            };
        }
    }

    /// <inheritdoc/>
    public override bool IsContainFunction(string name)
    {
        return _mcpTools.ContainsKey(name);
    }

    /// <summary>
    /// Gets a list of all available function names from the MCP server.
    /// </summary>
    /// <returns>List of function names.</returns>
    public IReadOnlyList<string> GetAvailableFunctions()
    {
        return _mcpTools.Keys.ToList();
    }

    /// <summary>
    /// Gets detailed information about a specific function.
    /// </summary>
    /// <param name="functionName">The name of the function.</param>
    /// <returns>The FunctionDeclaration for the specified function, or null if not found.</returns>
    public FunctionDeclaration? GetFunctionInfo(string functionName)
    {
        return _functionDeclarations.FirstOrDefault(f => f.Name == functionName);
    }

    private async Task TryReconnectAsync(CancellationToken cancellationToken)
    {
        if (_reconnectAttempts >= _options.MaxReconnectAttempts)
        {
            throw new InvalidOperationException(
                $"Failed to reconnect to MCP server after {_options.MaxReconnectAttempts} attempts.");
        }

        _reconnectAttempts++;

        // Dispose old connection
        if (_client != null)
        {
            await _client.DisposeAsync().ConfigureAwait(false);
            _client = null;
        }

        if (_transport != null)
        {
            await _transport.DisposeAsync().ConfigureAwait(false);
            _transport = null;
        }

        // Create new transport if factory is available
        if (_transportFactory != null)
        {
            _transport = _transportFactory();
        }
        else
        {
            throw new InvalidOperationException(
                "Cannot reconnect without a transport factory. Use CreateAsync with a factory function for auto-reconnection support.");
        }

        // Reconnect
        await ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the McpTool and closes the connection to the MCP server.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the McpTool and closes the connection to the MCP server.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _connectionLock.Dispose();

            if (_client != null)
            {
                // For sync dispose, we can't await, so just dispose synchronously
                try
                {
                    _client.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                catch
                {
                    // Best effort cleanup
                }
            }

            if (_transport != null)
            {
                try
                {
                    _transport.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        _disposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_client != null)
        {
            await _client.DisposeAsync().ConfigureAwait(false);
        }

        if (_transport != null)
        {
            await _transport.DisposeAsync().ConfigureAwait(false);
        }

        _connectionLock.Dispose();
    }
}
