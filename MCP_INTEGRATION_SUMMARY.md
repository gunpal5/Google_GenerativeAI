# MCP Integration Summary

## Overview

This document summarizes the comprehensive MCP (Model Context Protocol) integration added to the Google_GenerativeAI SDK. The integration uses the official C# SDK and supports **all MCP transport protocols**: stdio, HTTP/SSE, and any custom transports.

## What Was Added

### 1. Core Implementation (`src/GenerativeAI.Tools/Mcp/`)

#### McpServerConfig.cs
- `McpToolOptions` class for controlling McpTool behavior (timeouts, reconnection, error handling)
- `McpTransportFactory` static class providing factory methods for all transport types:
  - `CreateStdioTransport()` - For launching MCP servers as subprocesses
  - `CreateHttpTransport()` - For connecting to HTTP/SSE MCP servers
  - `CreateHttpTransportWithAuth()` - HTTP with Bearer authentication
  - `CreateHttpTransportWithHeaders()` - HTTP with custom headers

#### McpTool.cs
- Main integration class implementing `IFunctionTool` interface
- **Now accepts `IClientTransport` directly** - works with any transport from the MCP SDK
- Features:
  - Supports all MCP transport types (stdio, HTTP/SSE, custom)
  - Auto-discovery of MCP server tools
  - Automatic reconnection support with transport factories
  - Error handling with detailed error responses
  - Support for multiple concurrent MCP servers
  - Async/await pattern with proper disposal (IDisposable and IAsyncDisposable)
  - Converts MCP tools to Google's `FunctionDeclaration` format
  - Forwards function calls to MCP servers
  - Handles MCP responses and converts them to `FunctionResponse`

### 2. Example Project (`samples/McpIntegrationDemo/`)

#### Program.cs
Comprehensive demo showing:
1. **Stdio Transport** - Launch MCP server as subprocess
2. **HTTP/SSE Transport** - Connect to remote MCP server
3. **Multiple Servers** - Use multiple MCP servers with different transports simultaneously
4. **Custom Configuration** - Environment variables, headers, authentication
5. **Auto-Reconnection** - Using transport factories for automatic reconnection

#### McpIntegrationDemo.csproj
- .NET 8.0 project
- References to GenerativeAI and GenerativeAI.Tools

### 3. Documentation

#### samples/McpIntegrationDemo/README.md
- Quick start guide
- Usage examples for all transport types
- Configuration options reference
- List of available MCP servers
- API reference
- Troubleshooting guide

#### src/GenerativeAI.Tools/Mcp/README.md
- Comprehensive technical documentation
- Architecture overview
- Transport types and configurations
- Detailed usage examples for each transport
- Error handling patterns
- Best practices
- Limitations and troubleshooting

### 4. Dependencies

#### Updated GenerativeAI.Tools.csproj
- Added `ModelContextProtocol` NuGet package (v0.4.0-preview.3)
- Official C# SDK for Model Context Protocol

## Key Features

1. **All Transport Protocols**: stdio, HTTP/SSE, and custom transports
2. **Native SDK Integration**: Uses MCP SDK's transport classes directly
3. **Flexible Configuration**: Factory methods for common scenarios
4. **Auto-Discovery**: Automatically finds and exposes all tools
5. **Multiple Servers**: Connect to multiple MCP servers at once
6. **Robust**: Auto-reconnect, error handling, timeouts
7. **Type-Safe**: Fully typed with nullable reference types
8. **Well-Documented**: Comprehensive docs and examples
9. **IDisposable**: Proper resource management
10. **Async**: Full async/await support

## Usage Examples

### Stdio Transport (Launch Server as Subprocess)

```csharp
// Create transport
var transport = McpTransportFactory.CreateStdioTransport(
    "my-server",
    "npx",
    new[] { "-y", "@modelcontextprotocol/server-everything" }
);

// Create MCP tool
using var mcpTool = await McpTool.CreateAsync(transport);

// Add to Gemini model
var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
model.AddFunctionTool(mcpTool);
model.FunctionCallingBehaviour.AutoCallFunction = true;

// Use it!
var result = await model.GenerateContentAsync("Your query here");
```

### HTTP/SSE Transport (Connect to Remote Server)

```csharp
// Create HTTP transport
var transport = McpTransportFactory.CreateHttpTransport("http://localhost:8080");

// Or with authentication
var authTransport = McpTransportFactory.CreateHttpTransportWithAuth(
    "https://api.example.com",
    "your-auth-token"
);

// Or with custom headers
var customTransport = McpTransportFactory.CreateHttpTransportWithHeaders(
    "http://localhost:8080",
    new Dictionary<string, string>
    {
        { "X-API-Key", "your-key" },
        { "X-Custom-Header", "value" }
    }
);

// Use same as stdio
using var mcpTool = await McpTool.CreateAsync(transport);
// ... use with Gemini
```

### Multiple MCP Servers with Different Transports

```csharp
var transports = new List<IClientTransport>
{
    McpTransportFactory.CreateStdioTransport("server1", "npx", new[] { "..." }),
    McpTransportFactory.CreateHttpTransport("http://localhost:8080"),
    McpTransportFactory.CreateStdioTransport("server2", "python", new[] { "..." })
};

var mcpTools = await McpTool.CreateMultipleAsync(transports);

// Add all tools to model
var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
foreach (var tool in mcpTools)
{
    model.AddFunctionTool(tool);
}
```

### Auto-Reconnection with Transport Factory

```csharp
// Use factory for auto-reconnection
using var mcpTool = await McpTool.CreateAsync(
    transportFactory: () => McpTransportFactory.CreateStdioTransport(
        "server",
        "npx",
        new[] { "-y", "@modelcontextprotocol/server-everything" }
    ),
    options: new McpToolOptions
    {
        AutoReconnect = true,
        MaxReconnectAttempts = 3
    }
);
```

### Custom Transport (Any IClientTransport)

```csharp
// You can use any IClientTransport implementation from the MCP SDK
var customTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "custom",
    Command = "node",
    Arguments = new List<string> { "my-server.js", "--port", "3000" },
    EnvironmentVariables = new Dictionary<string, string>
    {
        { "NODE_ENV", "production" }
    },
    WorkingDirectory = "/path/to/server"
});

using var mcpTool = await McpTool.CreateAsync(customTransport);
```

## Architecture

```
Gemini Model (GenerativeAI)
    ↓ IFunctionTool interface
McpTool (GenerativeAI.Tools.Mcp)
    ↓ IClientTransport interface
MCP SDK Transports:
    ├─ StdioClientTransport (stdio protocol)
    ├─ HttpClientTransport (HTTP/SSE protocol)
    └─ Custom transports
        ↓
MCP Servers (Node.js, Python, C#, etc.)
```

## Supported Transport Protocols

### 1. **Stdio Transport**
- Launches MCP server as subprocess
- Communicates via stdin/stdout
- Best for local servers
- Automatic process management

### 2. **HTTP/SSE Transport** (Streamable HTTP)
- Connects to remote MCP servers via HTTP
- Supports Server-Sent Events for real-time updates
- Best for remote/cloud servers
- Supports authentication and custom headers

### 3. **Custom Transports**
- Any `IClientTransport` implementation from MCP SDK
- Extensible for future transport types

## Files Added/Modified

```
src/GenerativeAI.Tools/
├── GenerativeAI.Tools.csproj (modified - added ModelContextProtocol package)
└── Mcp/
    ├── McpServerConfig.cs (redesigned with McpTransportFactory)
    ├── McpTool.cs (updated to accept IClientTransport)
    └── README.md (updated with all transport types)

samples/McpIntegrationDemo/
├── McpIntegrationDemo.csproj (new)
├── Program.cs (updated with all transport examples)
└── README.md (updated)

MCP_INTEGRATION_SUMMARY.md (updated)
```

## Compatibility

- **.NET Target Frameworks**: net8.0, netstandard2.0, net9.0, net462
- **MCP SDK**: ModelContextProtocol v0.4.0-preview.3
- **MCP Protocol**: Supports both stdio and Streamable HTTP (2025-03-26 spec)
- **MCP Servers**: Any server implementing the Model Context Protocol specification

## What Changed from Initial Implementation

### Before (v1)
- Custom `McpServerConfig` class
- Only supported stdio transport
- Required custom configuration wrapper

### After (v2 - Current)
- Uses MCP SDK's native `IClientTransport` interface
- Supports all transport protocols (stdio, HTTP/SSE, custom)
- `McpTransportFactory` for common configurations
- Direct access to MCP SDK features
- More flexible and extensible

## Testing

The integration should be tested with:

1. **Unit Tests**: Test McpTool creation, connection, and function calling
2. **Integration Tests**: Test with actual MCP servers (stdio and HTTP)
3. **Examples**: Run the demo project with various MCP servers

To run the demo:
```bash
export GEMINI_API_KEY=your-api-key
cd samples/McpIntegrationDemo
dotnet run
```

## Future Enhancements

Potential improvements for future versions:

1. **More Factory Methods**: Additional helpers for common MCP server types
2. **Connection Pooling**: Reuse connections for better performance
3. **Caching**: Cache tool definitions to reduce refresh overhead
4. **Metrics**: Add telemetry and monitoring
5. **Testing**: Unit and integration tests
6. **Configuration Files**: JSON-based configuration file support
7. **OAuth Support**: Integration with MCP SDK's OAuth features

## Breaking Changes from v1

If you were using the initial implementation:

### Old Way (v1)
```csharp
var config = new McpServerConfig
{
    Name = "server",
    Command = "npx",
    Arguments = new List<string> { "..." }
};
using var mcpTool = await McpTool.CreateAsync(config);
```

### New Way (v2)
```csharp
var transport = McpTransportFactory.CreateStdioTransport(
    "server",
    "npx",
    new[] { "..." }
);
using var mcpTool = await McpTool.CreateAsync(transport);
```

## Benefits of New Architecture

1. ✅ **Full Protocol Support**: Not limited to stdio anymore
2. ✅ **Native SDK Integration**: Uses MCP SDK types directly
3. ✅ **Extensibility**: Easy to add new transport types
4. ✅ **Flexibility**: Direct access to all transport options
5. ✅ **Future-Proof**: Automatically supports new MCP SDK features
6. ✅ **Better Separation**: Clear separation between transport and tool logic

## References

- [MCP Official Website](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [MCP Transports Documentation](https://modelcontextprotocol.io/specification/2025-03-26/basic/transports)
- [MCP Servers Repository](https://github.com/modelcontextprotocol/servers)

## License

This integration follows the same license as the Google_GenerativeAI SDK.

## Contributing

Contributions, bug reports, and feature requests are welcome!
