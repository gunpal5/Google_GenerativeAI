using GenerativeAI;
using GenerativeAI.Tools.Mcp;

namespace McpIntegrationDemo;

/// <summary>
/// Demonstrates how to integrate MCP (Model Context Protocol) servers with Google Generative AI.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== MCP Integration Demo for Google Generative AI ===\n");

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
            // Example 1: Single MCP Server
            await SingleMcpServerExample(apiKey);

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 2: Multiple MCP Servers
            await MultipleMcpServersExample(apiKey);

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Example 3: Custom MCP Server Configuration
            await CustomConfigurationExample(apiKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Example 1: Connecting to a single MCP server
    /// </summary>
    static async Task SingleMcpServerExample(string apiKey)
    {
        Console.WriteLine("Example 1: Single MCP Server Integration\n");

        // Configure the MCP server
        // This example uses the official MCP "everything" demo server
        var mcpConfig = new McpServerConfig
        {
            Name = "everything-server",
            Command = "npx",
            Arguments = new List<string> { "-y", "@modelcontextprotocol/server-everything" }
        };

        Console.WriteLine($"Connecting to MCP server: {mcpConfig.Name}...");

        // Create and connect to the MCP server
        using var mcpTool = await McpTool.CreateAsync(mcpConfig);

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
        Console.WriteLine("Asking model: 'Use the echo tool to say Hello from MCP!'\n");

        var result = await model.GenerateContentAsync("Use the echo tool to say 'Hello from MCP!'");
        Console.WriteLine($"Model response: {result.Text}");
    }

    /// <summary>
    /// Example 2: Connecting to multiple MCP servers simultaneously
    /// </summary>
    static async Task MultipleMcpServersExample(string apiKey)
    {
        Console.WriteLine("Example 2: Multiple MCP Servers\n");

        // Configure multiple MCP servers
        var configs = new List<McpServerConfig>
        {
            new McpServerConfig
            {
                Name = "server-1",
                Command = "npx",
                Arguments = new List<string> { "-y", "@modelcontextprotocol/server-everything" }
            },
            // You can add more servers here
            // new McpServerConfig
            // {
            //     Name = "weather-server",
            //     Command = "python",
            //     Arguments = new List<string> { "weather_mcp_server.py" }
            // }
        };

        Console.WriteLine("Connecting to multiple MCP servers...");

        // Create all MCP tools
        var mcpTools = await McpTool.CreateMultipleAsync(configs);

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
    /// Example 3: Custom MCP server configuration with options
    /// </summary>
    static async Task CustomConfigurationExample(string apiKey)
    {
        Console.WriteLine("Example 3: Custom Configuration\n");

        // Advanced MCP server configuration
        var mcpConfig = new McpServerConfig
        {
            Name = "custom-server",
            Command = "npx",
            Arguments = new List<string> { "-y", "@modelcontextprotocol/server-everything" },
            ConnectionTimeoutMs = 60000, // 60 seconds
            Environment = new Dictionary<string, string>
            {
                // Add custom environment variables if needed
                // { "CUSTOM_VAR", "value" }
            }
        };

        // Custom tool options
        var toolOptions = new McpToolOptions
        {
            AutoReconnect = true,
            MaxReconnectAttempts = 5,
            ThrowOnToolCallFailure = false,
            IncludeDetailedErrors = true
        };

        Console.WriteLine("Creating MCP tool with custom configuration...");

        using var mcpTool = await McpTool.CreateAsync(mcpConfig, toolOptions);

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
        model.FunctionCallingBehaviour.FunctionEnabled = true;
        model.FunctionCallingBehaviour.AutoCallFunction = true;
        model.FunctionCallingBehaviour.AutoReplyFunction = true;

        // You can also manually refresh tools if the MCP server updates
        Console.WriteLine("Refreshing tools from MCP server...");
        await mcpTool.RefreshToolsAsync();
        Console.WriteLine($"Tools refreshed. Count: {mcpTool.GetAvailableFunctions().Count}");
    }
}
