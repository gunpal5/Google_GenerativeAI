# McpTool - Model Context Protocol Integration

The `McpTool` class provides seamless integration between **Model Context Protocol (MCP)** servers and **Google Generative AI**. It implements the `IFunctionTool` interface, allowing MCP server tools to be used directly with Gemini models.

## Overview

MCP (Model Context Protocol) is an open protocol that standardizes how applications provide context to AI models. With `McpTool`, you can:

- Connect to any MCP server (local or remote)
- Automatically discover and expose tools from MCP servers
- Use multiple MCP servers simultaneously
- Handle reconnections and errors gracefully

## Installation

The `McpTool` class is included in the `Google_GenerativeAI.Tools` package:

```bash
dotnet add package Google_GenerativeAI.Tools
```

The package already includes the required `ModelContextProtocol` dependency.

## Quick Start

```csharp
using GenerativeAI;
using GenerativeAI.Tools.Mcp;

// Configure MCP server
var config = new McpServerConfig
{
    Name = "my-mcp-server",
    Command = "npx",
    Arguments = new List<string> { "-y", "@modelcontextprotocol/server-everything" }
};

// Create and connect to MCP server
using var mcpTool = await McpTool.CreateAsync(config);

// Add to Gemini model
var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
model.AddFunctionTool(mcpTool);
model.FunctionCallingBehaviour.AutoCallFunction = true;

// Use the model
var result = await model.GenerateContentAsync("Your prompt here");
```

## Architecture

```
┌─────────────────┐
│  Gemini Model   │
└────────┬────────┘
         │ IFunctionTool
         │
┌────────▼────────┐
│    McpTool      │
└────────┬────────┘
         │ MCP Protocol
         │
┌────────▼────────┐
│   MCP Server    │
│ (Any Language)  │
└─────────────────┘
```

## Core Components

### 1. McpServerConfig

Configures the connection to an MCP server:

```csharp
public class McpServerConfig
{
    public string Name { get; set; }                       // Server identifier
    public string Command { get; set; }                    // Executable command
    public List<string> Arguments { get; set; }            // Command arguments
    public Dictionary<string, string>? Environment { get; set; } // Env variables
    public string? WorkingDirectory { get; set; }          // Working directory
    public int ConnectionTimeoutMs { get; set; } = 30000;  // Timeout (default: 30s)
}
```

### 2. McpToolOptions

Controls the behavior of `McpTool`:

```csharp
public class McpToolOptions
{
    public bool AutoReconnect { get; set; } = true;             // Auto-reconnect on disconnect
    public int MaxReconnectAttempts { get; set; } = 3;          // Max reconnection attempts
    public bool ThrowOnToolCallFailure { get; set; } = false;   // Throw on errors
    public bool IncludeDetailedErrors { get; set; } = true;     // Include error details
}
```

### 3. McpTool

The main class that bridges MCP servers with Google Generative AI:

```csharp
public class McpTool : GoogleFunctionTool, IDisposable, IAsyncDisposable
{
    // Create a single MCP tool
    public static Task<McpTool> CreateAsync(
        McpServerConfig config,
        McpToolOptions? options = null,
        CancellationToken cancellationToken = default);

    // Create multiple MCP tools at once
    public static Task<List<McpTool>> CreateMultipleAsync(
        IEnumerable<McpServerConfig> configs,
        McpToolOptions? options = null,
        CancellationToken cancellationToken = default);

    // Properties
    public McpClient? Client { get; }       // Underlying MCP client
    public bool IsConnected { get; }        // Connection status

    // Methods
    public Task RefreshToolsAsync(CancellationToken cancellationToken = default);
    public IReadOnlyList<string> GetAvailableFunctions();
    public FunctionDeclaration? GetFunctionInfo(string functionName);

    // IFunctionTool implementation
    public override Tool AsTool();
    public override Task<FunctionResponse?> CallAsync(FunctionCall functionCall, ...);
    public override bool IsContainFunction(string name);
}
```

## Usage Examples

### Example 1: Single MCP Server

```csharp
// Configure server
var config = new McpServerConfig
{
    Name = "filesystem",
    Command = "npx",
    Arguments = new List<string>
    {
        "-y",
        "@modelcontextprotocol/server-filesystem",
        "/home/user/documents"  // Allowed directory
    }
};

// Connect
using var mcpTool = await McpTool.CreateAsync(config);

// Check available functions
Console.WriteLine("Available functions:");
foreach (var funcName in mcpTool.GetAvailableFunctions())
{
    var info = mcpTool.GetFunctionInfo(funcName);
    Console.WriteLine($"  - {funcName}: {info?.Description}");
}

// Use with model
var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
model.AddFunctionTool(mcpTool);
model.FunctionCallingBehaviour.FunctionEnabled = true;
model.FunctionCallingBehaviour.AutoCallFunction = true;

var result = await model.GenerateContentAsync(
    "List all text files in the documents directory");
Console.WriteLine(result.Text);
```

### Example 2: Multiple MCP Servers

```csharp
var configs = new List<McpServerConfig>
{
    new()
    {
        Name = "filesystem",
        Command = "npx",
        Arguments = new() { "-y", "@modelcontextprotocol/server-filesystem", "/data" }
    },
    new()
    {
        Name = "github",
        Command = "npx",
        Arguments = new() { "-y", "@modelcontextprotocol/server-github" },
        Environment = new() { { "GITHUB_TOKEN", githubToken } }
    },
    new()
    {
        Name = "weather",
        Command = "python",
        Arguments = new() { "weather_server.py" }
    }
};

// Connect to all servers
var mcpTools = await McpTool.CreateMultipleAsync(configs);

// Add all tools to model
var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
foreach (var tool in mcpTools)
{
    model.AddFunctionTool(tool);
    Console.WriteLine($"Added {tool.GetAvailableFunctions().Count} functions from {tool.Client?.Name}");
}

// Now the model can use tools from all servers
var result = await model.GenerateContentAsync(
    "Check the weather and create a GitHub issue if it's going to rain tomorrow");

// Cleanup
foreach (var tool in mcpTools)
{
    await tool.DisposeAsync();
}
```

### Example 3: Custom Configuration

```csharp
// Advanced server configuration
var config = new McpServerConfig
{
    Name = "my-custom-server",
    Command = "node",
    Arguments = new List<string> { "server.js", "--port", "8080" },
    ConnectionTimeoutMs = 60000,  // 60 seconds
    Environment = new Dictionary<string, string>
    {
        { "NODE_ENV", "production" },
        { "API_KEY", Environment.GetEnvironmentVariable("API_KEY") ?? "" },
        { "LOG_LEVEL", "debug" }
    },
    WorkingDirectory = "/path/to/server"
};

// Custom tool options
var options = new McpToolOptions
{
    AutoReconnect = true,
    MaxReconnectAttempts = 5,
    ThrowOnToolCallFailure = false,  // Return errors in response instead
    IncludeDetailedErrors = true     // Include full error details
};

using var mcpTool = await McpTool.CreateAsync(config, options);

// Monitor connection status
if (mcpTool.IsConnected)
{
    Console.WriteLine("Successfully connected!");
}

// Manually refresh tools if server adds new ones
await mcpTool.RefreshToolsAsync();
```

### Example 4: Manual Function Calling

For more control over function execution:

```csharp
using var mcpTool = await McpTool.CreateAsync(config);

var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
model.AddFunctionTool(mcpTool);

// Disable automatic function calling
model.FunctionCallingBehaviour.AutoCallFunction = false;

var result = await model.GenerateContentAsync("Your query");

// Manually handle function calls
var candidate = result.Candidates?.FirstOrDefault();
if (candidate?.Content?.Parts != null)
{
    foreach (var part in candidate.Content.Parts)
    {
        if (part.FunctionCall != null)
        {
            Console.WriteLine($"Model wants to call: {part.FunctionCall.Name}");

            // Manually execute the function
            var response = await mcpTool.CallAsync(part.FunctionCall);

            Console.WriteLine($"Function result: {response?.Response}");

            // Send response back to model if needed
            // ...
        }
    }
}
```

## Error Handling

### Connection Errors

```csharp
try
{
    using var mcpTool = await McpTool.CreateAsync(config);
}
catch (TimeoutException)
{
    Console.WriteLine("Connection timeout - server took too long to start");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Failed to connect: {ex.Message}");
}
```

### Function Call Errors

With `ThrowOnToolCallFailure = false` (default), errors are returned in the response:

```csharp
var response = await mcpTool.CallAsync(functionCall);

var responseJson = response?.Response;
if (responseJson?["isError"]?.GetValue<bool>() == true)
{
    var error = responseJson["error"]?.ToString();
    Console.WriteLine($"Function failed: {error}");
}
```

With `ThrowOnToolCallFailure = true`, exceptions are thrown:

```csharp
try
{
    var response = await mcpTool.CallAsync(functionCall);
}
catch (Exception ex)
{
    Console.WriteLine($"Function call failed: {ex.Message}");
}
```

## Supported MCP Servers

`McpTool` supports any MCP server that implements the Model Context Protocol specification. This includes:

### Official Servers

- **@modelcontextprotocol/server-everything** - Demo server with all features
- **@modelcontextprotocol/server-filesystem** - File system operations
- **@modelcontextprotocol/server-github** - GitHub API operations
- **@modelcontextprotocol/server-postgres** - PostgreSQL database queries
- **@modelcontextprotocol/server-sqlite** - SQLite database queries
- **@modelcontextprotocol/server-memory** - Persistent memory/notes

### Community Servers

Find more at:
- https://github.com/modelcontextprotocol/servers
- https://github.com/topics/mcp-server

### Custom Servers

You can create your own MCP server in any language:
- C#: Use the official ModelContextProtocol SDK
- Python: Use the official `mcp` package
- Node.js/TypeScript: Use the official `@modelcontextprotocol/sdk`

## Best Practices

### 1. Always Dispose

```csharp
// Using statement (recommended)
await using var mcpTool = await McpTool.CreateAsync(config);

// Or manually
var mcpTool = await McpTool.CreateAsync(config);
try
{
    // Use the tool
}
finally
{
    await mcpTool.DisposeAsync();
}
```

### 2. Handle Connection Loss

```csharp
var options = new McpToolOptions
{
    AutoReconnect = true,
    MaxReconnectAttempts = 3
};

using var mcpTool = await McpTool.CreateAsync(config, options);

// The tool will automatically attempt to reconnect if connection is lost
```

### 3. Monitor Connection Status

```csharp
if (!mcpTool.IsConnected)
{
    Console.WriteLine("Warning: MCP server is not connected");
    // Handle appropriately
}
```

### 4. Refresh Tools Periodically

If your MCP server dynamically adds/removes tools:

```csharp
// Refresh every 5 minutes
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
while (await timer.WaitForNextTickAsync())
{
    await mcpTool.RefreshToolsAsync();
}
```

### 5. Use Environment Variables for Secrets

```csharp
var config = new McpServerConfig
{
    Name = "secure-server",
    Command = "node",
    Arguments = new() { "server.js" },
    Environment = new()
    {
        { "API_KEY", Environment.GetEnvironmentVariable("API_KEY") ?? "" },
        { "DB_PASSWORD", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "" }
    }
};
```

## Limitations

- **Stdio Transport Only**: Currently only supports stdio-based MCP servers. HTTP-based servers coming soon.
- **Preview Package**: The ModelContextProtocol SDK is in preview and may have breaking changes.
- **Function Responses**: Complex return types are serialized to JSON automatically.

## Troubleshooting

### Server Won't Start

- Check that the command and arguments are correct
- Verify the executable is in PATH (e.g., `npx`, `python`)
- Increase `ConnectionTimeoutMs` for slow-starting servers
- Check server logs for errors

### Tools Not Appearing

- Call `RefreshToolsAsync()` to update the tool list
- Verify the MCP server is correctly exposing tools
- Check server implementation for errors

### Function Calls Failing

- Enable detailed errors: `options.IncludeDetailedErrors = true`
- Verify argument schema matches what the MCP tool expects
- Check the MCP server logs for errors
- Ensure the server hasn't crashed (check `IsConnected`)

## Contributing

Contributions are welcome! Please open issues or pull requests on GitHub.

## License

This integration is part of the Google_GenerativeAI.Tools package and follows the same license.

## Resources

- [MCP Official Documentation](https://modelcontextprotocol.io/docs)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [Google Generative AI SDK](https://github.com/gunpal5/Google_GenerativeAI)
