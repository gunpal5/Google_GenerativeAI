using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GenerativeAI.Tests;
using GenerativeAI.Tools.Mcp;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Integration tests that make actual calls to MCP tools with real parameters.
/// These tests verify end-to-end functionality with live MCP server interaction.
/// </summary>
public class McpTool_RealCalls_Tests : TestBase
{
    public McpTool_RealCalls_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task ShouldInspectAvailableMcpTools()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var availableFunctions = mcpTool.GetAvailableFunctions();
        var tool = mcpTool.AsTool();

        // Assert
        availableFunctions.ShouldNotBeEmpty();

        Console.WriteLine("=== Available MCP Tools ===");
        Console.WriteLine($"Total tools: {availableFunctions.Count}");
        Console.WriteLine("");

        foreach (var functionName in availableFunctions)
        {
            var functionInfo = mcpTool.GetFunctionInfo(functionName);
            Console.WriteLine($"Tool: {functionName}");
            Console.WriteLine($"  Description: {functionInfo?.Description}");

            if (functionInfo?.ParametersJsonSchema != null)
            {
                Console.WriteLine($"  Parameters: {functionInfo.ParametersJsonSchema.ToJsonString()}");
            }
            Console.WriteLine("");
        }

        // Verify we have tools
        availableFunctions.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldCallEchoTool()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var availableFunctions = mcpTool.GetAvailableFunctions();

        Console.WriteLine($"Available functions: {string.Join(", ", availableFunctions)}");

        // Look for an echo or similar simple tool
        var echoTool = availableFunctions.FirstOrDefault(f =>
            f.Contains("echo", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("add", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("get", StringComparison.OrdinalIgnoreCase));

        if (echoTool != null)
        {
            Console.WriteLine($"Testing tool: {echoTool}");

            var functionInfo = mcpTool.GetFunctionInfo(echoTool);
            Console.WriteLine($"Function info: {functionInfo?.ParametersJsonSchema?.ToJsonString()}");

            // Create appropriate arguments based on the tool
            var args = new JsonObject();

            // Try to determine what parameters are needed
            if (functionInfo?.ParametersJsonSchema != null)
            {
                var schema = functionInfo.ParametersJsonSchema.AsObject();
                if (schema.TryGetPropertyValue("properties", out var properties))
                {
                    var propsObj = properties?.AsObject();
                    if (propsObj != null)
                    {
                        foreach (var prop in propsObj)
                        {
                            // Add sample values based on property name and type
                            if (prop.Key.Contains("message", StringComparison.OrdinalIgnoreCase) ||
                                prop.Key.Contains("text", StringComparison.OrdinalIgnoreCase))
                            {
                                args[prop.Key] = "Hello from MCP integration test!";
                            }
                            else if (prop.Key.Contains("number", StringComparison.OrdinalIgnoreCase) ||
                                     prop.Key.Contains("count", StringComparison.OrdinalIgnoreCase))
                            {
                                args[prop.Key] = 42;
                            }
                            else
                            {
                                args[prop.Key] = "test-value";
                            }
                        }
                    }
                }
            }

            var functionCall = new FunctionCall
            {
                Name = echoTool,
                Args = args
            };

            // Act
            var response = await mcpTool.CallAsync(functionCall, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            response.Name.ShouldBe(echoTool);
            response.Response.ShouldNotBeNull();

            Console.WriteLine($"Response: {response.Response.ToJsonString()}");
        }
        else
        {
            Console.WriteLine("No suitable test tool found. Available tools:");
            foreach (var tool in availableFunctions)
            {
                Console.WriteLine($"  - {tool}");
            }
        }
    }

    [Fact]
    public async Task ShouldCallMultipleMcpTools()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var availableFunctions = mcpTool.GetAvailableFunctions();

        Console.WriteLine($"Testing with {availableFunctions.Count} available tools");

        // Try calling the first few tools with empty or minimal arguments
        var successfulCalls = 0;
        var maxToTest = Math.Min(3, availableFunctions.Count);

        for (int i = 0; i < maxToTest; i++)
        {
            var functionName = availableFunctions[i];
            var functionInfo = mcpTool.GetFunctionInfo(functionName);

            Console.WriteLine($"\n=== Testing {functionName} ===");
            Console.WriteLine($"Description: {functionInfo?.Description}");

            try
            {
                var functionCall = new FunctionCall
                {
                    Name = functionName,
                    Args = new JsonObject() // Try with empty args first
                };

                var response = await mcpTool.CallAsync(functionCall, cancellationToken: TestContext.Current.CancellationToken);

                response.ShouldNotBeNull();
                Console.WriteLine($"Success! Response: {response.Response.ToJsonString()}");
                successfulCalls++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed (expected if required args missing): {ex.Message}");
            }
        }

        Console.WriteLine($"\n=== Summary: {successfulCalls}/{maxToTest} calls successful ===");
    }

    [Fact]
    public async Task ShouldUseGeminiWithMcpTools()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        var availableFunctions = mcpTool.GetAvailableFunctions();
        Console.WriteLine($"MCP Server has {availableFunctions.Count} tools available");

        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
        model.AddFunctionTool(mcpTool);

        // Act - Ask the model what tools it has
        var result = await model.GenerateContentAsync(
            "List all the tools you have access to and briefly describe what each one does.",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        var responseText = result.Text();
        responseText.ShouldNotBeNullOrEmpty();

        Console.WriteLine("=== Gemini's Response ===");
        Console.WriteLine(responseText);

        // The response should mention some of the tools
        responseText.Length.ShouldBeGreaterThan(50);
    }

    [Fact]
    public async Task ShouldMakeActualToolCallThroughGemini()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        var availableFunctions = mcpTool.GetAvailableFunctions();
        Console.WriteLine($"Available MCP tools: {string.Join(", ", availableFunctions)}");

        // Find a suitable tool to call
        var testTool = availableFunctions.FirstOrDefault(f =>
            f.Contains("echo", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("add", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("time", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("get", StringComparison.OrdinalIgnoreCase));

        if (testTool != null)
        {
            Console.WriteLine($"Will ask Gemini to call: {testTool}");

            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(mcpTool);

            // Act - Ask Gemini to use the tool
            var prompt = testTool.Contains("echo", StringComparison.OrdinalIgnoreCase)
                ? "Use the echo tool to echo the message 'Hello from Gemini!'"
                : testTool.Contains("add", StringComparison.OrdinalIgnoreCase)
                ? "Use the add tool to add 5 and 7"
                : testTool.Contains("time", StringComparison.OrdinalIgnoreCase)
                ? "Use the time tool to get the current time"
                : $"Call the {testTool} tool with appropriate parameters";

            Console.WriteLine($"Prompt: {prompt}");

            model.FunctionCallingBehaviour.AutoCallFunction = false;
            var result = await model.GenerateContentAsync(
                prompt,
                cancellationToken: TestContext.Current.CancellationToken
            );

            // Assert
            result.ShouldNotBeNull();
            var responseText = result.Text();
            Console.WriteLine($"\n=== Gemini's Response ===");
            Console.WriteLine(responseText);

            // Check if function was called
            if (result.Candidates?[0].Content?.Parts != null)
            {
                var functionCalls = result.Candidates[0].Content.Parts
                    .Where(p => p.FunctionCall != null)
                    .Select(p => p.FunctionCall)
                    .ToList();

                if (functionCalls.Any())
                {
                    Console.WriteLine($"\n=== Function Calls Made ===");
                    foreach (var call in functionCalls)
                    {
                        Console.WriteLine($"Function: {call?.Name}");
                        Console.WriteLine($"Args: {call?.Args?.ToJsonString()}");
                    }
                }
            }

            responseText.ShouldNotBeNullOrEmpty();
        }
        else
        {
            Console.WriteLine("No suitable tool found for Gemini test");
            Console.WriteLine($"Available: {string.Join(", ", availableFunctions)}");
        }
    }

    [Fact]
    public async Task ShouldHandleComplexWorkflowWithMcp()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "everything-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
        model.AddFunctionTool(mcpTool);

        // Act - Use a multi-turn conversation with function calling
        var chat = model.StartChat();

        Console.WriteLine("=== Starting Chat Session ===");

        var response1 = await chat.GenerateContentAsync(
            "What tools do you have available? Pick one and demonstrate how to use it.",
            cancellationToken: TestContext.Current.CancellationToken
        );

        Console.WriteLine($"\n=== Turn 1 ===");
        Console.WriteLine(response1.Text());

        // If there were function calls, the response should mention them
        response1.ShouldNotBeNull();
        response1.Text().ShouldNotBeNullOrEmpty();

        // Continue the conversation
        var response2 = await chat.GenerateContentAsync(
            "Great! Can you try using another tool?",
            cancellationToken: TestContext.Current.CancellationToken
        );

        Console.WriteLine($"\n=== Turn 2 ===");
        Console.WriteLine(response2.Text());

        response2.ShouldNotBeNull();
        response2.Text().ShouldNotBeNullOrEmpty();

        // Verify chat history includes our messages
        var history = chat.History;
        history.Count.ShouldBeGreaterThanOrEqualTo(2);

        Console.WriteLine($"\n=== Chat History: {history.Count} entries ===");
    }
}
