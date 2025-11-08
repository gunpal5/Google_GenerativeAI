using GenerativeAI;
using GenerativeAI.Tools.Mcp;
using ModelContextProtocol.Transports;

namespace McpIntegrationDemo;

/// <summary>
/// Demonstrates how to integrate MCP (Model Context Protocol) servers with Google Generative AI.
/// Shows examples for all transport types: stdio, HTTP/SSE.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== MCP Integration Demo for Google Generative AI ===\n");
        Console.WriteLine("Supports all MCP transport protocols: stdio, HTTP/SSE\n");

        // Get API key from environment variable
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: GEMINI_API_KEY environment variable not set.");
            Console.WriteLine("Please set it with: export GEMINI_API_KEY=your-api-key");
            return;
        }

        try
        {
            // Example 1: Stdio Transport
            await StdioTransportExample(apiKey);

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 2: HTTP/SSE Transport
            await HttpTransportExample(apiKey);

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 3: Multiple MCP Servers with Different Transports
            await MultipleMcpServersExample(apiKey);

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 4: Custom Transport Configuration
            await CustomConfigurationExample(apiKey);

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 5: Auto-Reconnection with Transport Factory
            await AutoReconnectionExample(apiKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Example 1: Using stdio transport to launch an MCP server as a subprocess
    /// </summary>
    static async Task StdioTransportExample(string apiKey)
    {
        Console.WriteLine("Example 1: Stdio Transport (Launch MCP Server as Subprocess)\n");

        // Create stdio transport using the factory
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        Console.WriteLine("Connecting to MCP server via stdio...");

        // Create and connect to the MCP server
        using var mcpTool = await McpTool.CreateAsync(transport);

        Console.WriteLine($"Connected! Available functions: {string.Join(", ", mcpTool.GetAvailableFunctions())}");
        Console.WriteLine();

        // Create a Gemini model and add the MCP tool
        var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
        model.AddFunctionTool(mcpTool);

        // Enable automatic function calling
        model.FunctionCallingBehaviour.FunctionEnabled = true;
        model.FunctionCallingBehaviour.AutoCallFunction = true;
        model.FunctionCallingBehaviour.AutoReplyFunction = true;

        // Ask the model something that requires using the MCP tools
        Console.WriteLine("Asking model: 'Use the echo tool to say Hello from stdio!'\n");

        var result = await model.GenerateContentAsync("Use the echo tool to say 'Hello from stdio!'");
        Console.WriteLine($"Model response: {result.Text}");
    }

    /// <summary>
    /// Example 2: Using HTTP/SSE transport to connect to a remote MCP server
    /// </summary>
    static async Task HttpTransportExample(string apiKey)
    {
        Console.WriteLine("Example 2: HTTP/SSE Transport (Connect to Remote MCP Server)\n");

        // NOTE: This example requires a running MCP server on HTTP
        // You can skip this if you don't have an HTTP MCP server running
        Console.WriteLine("Checking if HTTP MCP server is available at http://localhost:8080...");

        try
        {
            // Create HTTP transport using the factory
            var transport = McpTransportFactory.CreateHttpTransport("http://localhost:8080");

            Console.WriteLine("Connecting to MCP server via HTTP/SSE...");

            // Create tool with a shorter timeout for this example
            var options = new McpToolOptions
            {
                ConnectionTimeoutMs = 5000 // 5 seconds
            };

            using var mcpTool = await McpTool.CreateAsync(transport, options);

            Console.WriteLine($"Connected! Available functions: {string.Join(", ", mcpTool.GetAvailableFunctions())}");
            Console.WriteLine();

            // Use with Gemini model
            var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
            model.AddFunctionTool(mcpTool);
            model.FunctionCallingBehaviour.AutoCallFunction = true;

            var result = await model.GenerateContentAsync("List the available tools");
            Console.WriteLine($"Model response: {result.Text}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Skipping HTTP example: {ex.Message}");
            Console.WriteLine("To run this example, start an MCP server with HTTP transport on port 8080");
        }
    }

    /// <summary>
    /// Example 3: Connecting to multiple MCP servers with different transports
    /// </summary>
    static async Task MultipleMcpServersExample(string apiKey)
    {
        Console.WriteLine("Example 3: Multiple MCP Servers with Different Transports\n");

        // Create transports for multiple servers
        var transports = new List<IClientTransport>
        {
            McpTransportFactory.CreateStdioTransport(
                "server-1",
                "npx",
                new[] { "-y", "@modelcontextprotocol/server-everything" }
            ),
            // You can mix different transport types
            // McpTransportFactory.CreateHttpTransport("http://localhost:8080"),
            // McpTransportFactory.CreateStdioTransport(
            //     "python-server",
            //     "python",
            //     new[] { "weather_mcp_server.py" }
            // )
        };

        Console.WriteLine("Connecting to multiple MCP servers...");

        // Create all MCP tools
        var mcpTools = await McpTool.CreateMultipleAsync(transports);

        foreach (var tool in mcpTools)
        {
            Console.WriteLine($"  - {tool.GetAvailableFunctions().Count} functions from MCP server");
        }
        Console.WriteLine();

        // Create a model and add all MCP tools
        var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
        foreach (var tool in mcpTools)
        {
            model.AddFunctionTool(tool);
        }

        model.FunctionCallingBehaviour.FunctionEnabled = true;
        model.FunctionCallingBehaviour.AutoCallFunction = true;
        model.FunctionCallingBehaviour.AutoReplyFunction = true;

        Console.WriteLine("Asking model to use available tools...\n");

        var result = await model.GenerateContentAsync(
            "List all the tools you have access to and give me a brief description of what they can do.");
        Console.WriteLine($"Model response:\n{result.Text}");

        // Cleanup
        foreach (var tool in mcpTools)
        {
            await tool.DisposeAsync();
        }
    }

    /// <summary>
    /// Example 4: Custom transport configuration with environment variables and headers
    /// </summary>
    static async Task CustomConfigurationExample(string apiKey)
    {
        Console.WriteLine("Example 4: Custom Transport Configuration\n");

        // Example 1: Stdio with environment variables
        Console.WriteLine("Creating stdio transport with custom environment variables...");

        var stdioTransport = McpTransportFactory.CreateStdioTransport(
            name: "custom-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" },
            environmentVariables: new Dictionary<string, string>
            {
                { "NODE_ENV", "production" },
                { "LOG_LEVEL", "debug" }
            },
            workingDirectory: null
        );

        // Example 2: HTTP with authentication
        Console.WriteLine("Example HTTP transport with authentication (not connecting):");
        Console.WriteLine("  var httpTransport = McpTransportFactory.CreateHttpTransportWithAuth(");
        Console.WriteLine("      \"http://api.example.com\",");
        Console.WriteLine("      \"your-auth-token\"");
        Console.WriteLine("  );");
        Console.WriteLine();

        // Custom tool options
        var toolOptions = new McpToolOptions
        {
            ConnectionTimeoutMs = 60000, // 60 seconds
            AutoReconnect = true,
            MaxReconnectAttempts = 5,
            ThrowOnToolCallFailure = false,
            IncludeDetailedErrors = true
        };

        Console.WriteLine("Connecting with custom options...");

        using var mcpTool = await McpTool.CreateAsync(stdioTransport, toolOptions);

        Console.WriteLine("Connected!");
        Console.WriteLine($"Is Connected: {mcpTool.IsConnected}");
        Console.WriteLine($"Available Functions: {mcpTool.GetAvailableFunctions().Count}");
        Console.WriteLine();

        // Display detailed information about each function
        Console.WriteLine("Function Details:");
        foreach (var functionName in mcpTool.GetAvailableFunctions())
        {
            var info = mcpTool.GetFunctionInfo(functionName);
            if (info != null)
            {
                Console.WriteLine($"  - {info.Name}");
                Console.WriteLine($"    Description: {info.Description ?? "N/A"}");
            }
        }
        Console.WriteLine();

        // Use the tool with a model
        var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
        model.AddFunctionTool(mcpTool);
        model.FunctionCallingBehaviour.AutoCallFunction = true;

        // You can also manually refresh tools if the MCP server updates
        Console.WriteLine("Refreshing tools from MCP server...");
        await mcpTool.RefreshToolsAsync();
        Console.WriteLine($"Tools refreshed. Count: {mcpTool.GetAvailableFunctions().Count}");
    }

    /// <summary>
    /// Example 5: Auto-reconnection using transport factory
    /// </summary>
    static async Task AutoReconnectionExample(string apiKey)
    {
        Console.WriteLine("Example 5: Auto-Reconnection with Transport Factory\n");

        // Use a factory function for auto-reconnection support
        // The factory will be called to create a new transport if reconnection is needed
        using var mcpTool = await McpTool.CreateAsync(
            transportFactory: () => McpTransportFactory.CreateStdioTransport(
                "reconnectable-server",
                "npx",
                new[] { "-y", "@modelcontextprotocol/server-everything" }
            ),
            options: new McpToolOptions
            {
                AutoReconnect = true,
                MaxReconnectAttempts = 3
            }
        );

        Console.WriteLine("Connected with auto-reconnection support!");
        Console.WriteLine($"Available functions: {mcpTool.GetAvailableFunctions().Count}");
        Console.WriteLine();
        Console.WriteLine("If the connection is lost, McpTool will automatically attempt to reconnect");
        Console.WriteLine("up to 3 times using the transport factory.\n");

        // Use with Gemini
        var model = new GenerativeModel(apiKey, "gemini-2.0-flash-exp");
        model.AddFunctionTool(mcpTool);
        model.FunctionCallingBehaviour.AutoCallFunction = true;

        var result = await model.GenerateContentAsync("Use echo to say 'Auto-reconnection enabled!'");
        Console.WriteLine($"Model response: {result.Text}");
    }
}
