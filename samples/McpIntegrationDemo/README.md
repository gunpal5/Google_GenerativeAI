# MCP Integration Demo

This sample demonstrates how to integrate **Model Context Protocol (MCP)** servers with **Google Generative AI** using the `McpTool` class.

## What is MCP?

Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to AI models. MCP servers expose tools, prompts, and resources that can be used by AI applications.

Learn more: https://modelcontextprotocol.io/

## Features

- ✅ **Easy Integration**: Connect to any MCP server with simple configuration
- ✅ **Auto-Discovery**: Automatically discovers and exposes all tools from MCP servers
- ✅ **Multiple Servers**: Connect to multiple MCP servers simultaneously
- ✅ **Auto-Reconnect**: Configurable automatic reconnection on connection loss
- ✅ **Error Handling**: Graceful error handling with detailed error responses
- ✅ **Type-Safe**: Fully typed with C# 12 and .NET 8

## Prerequisites

- .NET 8.0 or later
- Node.js and npm (for running MCP servers via npx)
- Google Gemini API key

## Quick Start

### 1. Set your API key

```bash
export GEMINI_API_KEY=your-api-key-here
```

### 2. Run the demo

```bash
cd samples/McpIntegrationDemo
dotnet run
```

## Usage Examples

### Basic Usage

```csharp
using GenerativeAI;
using GenerativeAI.Tools.Mcp;

// Configure MCP server
var config = new McpServerConfig
{
    Name = "my-server",
    Command = "npx",
    Arguments = new List<string> { "-y", "@modelcontextprotocol/server-everything" }
};

// Create MCP tool
using var mcpTool = await McpTool.CreateAsync(config);

// Create Gemini model and add MCP tool
var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
model.AddFunctionTool(mcpTool);
model.FunctionCallingBehaviour.AutoCallFunction = true;

// Use the model with MCP tools
var result = await model.GenerateContentAsync("Use the echo tool to say hello!");
Console.WriteLine(result.Text);
```

### Multiple MCP Servers

```csharp
var configs = new List<McpServerConfig>
{
    new() { Name = "server1", Command = "npx", Arguments = [...] },
    new() { Name = "server2", Command = "python", Arguments = [...] }
};

var mcpTools = await McpTool.CreateMultipleAsync(configs);

var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
foreach (var tool in mcpTools)
{
    model.AddFunctionTool(tool);
}
```

### Custom Configuration

```csharp
var config = new McpServerConfig
{
    Name = "custom-server",
    Command = "npx",
    Arguments = new List<string> { "-y", "@my/mcp-server" },
    ConnectionTimeoutMs = 60000,
    Environment = new Dictionary<string, string>
    {
        { "API_KEY", "secret" }
    },
    WorkingDirectory = "/path/to/dir"
};

var options = new McpToolOptions
{
    AutoReconnect = true,
    MaxReconnectAttempts = 5,
    ThrowOnToolCallFailure = false,
    IncludeDetailedErrors = true
};

using var mcpTool = await McpTool.CreateAsync(config, options);
```

## Configuration Options

### McpServerConfig

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `Name` | `string` | Server name for identification | Required |
| `Command` | `string` | Command to execute (e.g., "npx", "python") | Required |
| `Arguments` | `List<string>` | Command-line arguments | Required |
| `Environment` | `Dictionary<string, string>?` | Environment variables | `null` |
| `WorkingDirectory` | `string?` | Working directory for the process | `null` |
| `ConnectionTimeoutMs` | `int` | Connection timeout in milliseconds | `30000` |

### McpToolOptions

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `AutoReconnect` | `bool` | Auto-reconnect on connection loss | `true` |
| `MaxReconnectAttempts` | `int` | Maximum reconnection attempts | `3` |
| `ThrowOnToolCallFailure` | `bool` | Throw exceptions on failures | `false` |
| `IncludeDetailedErrors` | `bool` | Include detailed error information | `true` |

## Available MCP Servers

Here are some public MCP servers you can use:

### Official Demo Servers

```csharp
// Everything Server - demonstrates all MCP features
new McpServerConfig
{
    Name = "everything",
    Command = "npx",
    Arguments = new List<string> { "-y", "@modelcontextprotocol/server-everything" }
}

// Filesystem Server - file operations
new McpServerConfig
{
    Name = "filesystem",
    Command = "npx",
    Arguments = new List<string> { "-y", "@modelcontextprotocol/server-filesystem", "/path/to/allowed/directory" }
}

// GitHub Server - GitHub API operations
new McpServerConfig
{
    Name = "github",
    Command = "npx",
    Arguments = new List<string> { "-y", "@modelcontextprotocol/server-github" },
    Environment = new Dictionary<string, string>
    {
        { "GITHUB_TOKEN", "your-github-token" }
    }
}
```

### Community Servers

You can find more MCP servers at:
- https://github.com/modelcontextprotocol/servers
- https://github.com/topics/mcp-server

## API Reference

### McpTool Class

#### Static Methods

- `CreateAsync(config, options?, cancellationToken)` - Create and connect to a single MCP server
- `CreateMultipleAsync(configs, options?, cancellationToken)` - Create connections to multiple servers

#### Instance Methods

- `RefreshToolsAsync(cancellationToken)` - Refresh the list of available tools
- `GetAvailableFunctions()` - Get list of available function names
- `GetFunctionInfo(functionName)` - Get detailed info about a function
- `AsTool()` - Convert to Google's Tool format (IFunctionTool interface)
- `CallAsync(functionCall, cancellationToken)` - Execute a function call (IFunctionTool interface)
- `IsContainFunction(name)` - Check if function exists (IFunctionTool interface)

#### Properties

- `Client` - The underlying MCP client instance
- `IsConnected` - Whether currently connected to the server

## Troubleshooting

### Connection Issues

If you experience connection issues:

1. Ensure the MCP server command is correct and accessible
2. Check that Node.js/npm is installed if using npx
3. Increase `ConnectionTimeoutMs` if the server takes time to start
4. Check server logs for errors

### Function Call Failures

If function calls fail:

1. Enable detailed errors: `options.IncludeDetailedErrors = true`
2. Check the function arguments match the expected schema
3. Verify the MCP server is still running: `mcpTool.IsConnected`
4. Use `RefreshToolsAsync()` to update available tools

## Advanced Usage

### Manual Function Calling

Disable auto-calling to handle function calls manually:

```csharp
model.FunctionCallingBehaviour.AutoCallFunction = false;

var result = await model.GenerateContentAsync("Query");

if (result.Candidates?[0].Content?.Parts?.Any(p => p.FunctionCall != null) == true)
{
    foreach (var part in result.Candidates[0].Content.Parts)
    {
        if (part.FunctionCall != null)
        {
            var response = await mcpTool.CallAsync(part.FunctionCall);
            // Handle response
        }
    }
}
```

### Implementing Your Own MCP Server

You can create your own MCP server in any language. See:
- https://modelcontextprotocol.io/docs/getting-started
- https://github.com/modelcontextprotocol/csharp-sdk

## License

This sample is part of the Google_GenerativeAI SDK and is licensed under the same terms.

## Related Resources

- [MCP Official Website](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Google Generative AI SDK](https://github.com/gunpal5/Google_GenerativeAI)
- [MCP Servers Repository](https://github.com/modelcontextprotocol/servers)
