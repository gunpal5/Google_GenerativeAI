using System.Text.Json.Nodes;
using GenerativeAI.Tests;
using GenerativeAI.Tools.Mcp;
using GenerativeAI.Types;
using ModelContextProtocol.Client;
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Integration tests for MCP (Model Context Protocol) tool integration.
/// These tests verify that McpTool correctly integrates MCP servers with Gemini models.
/// </summary>
public class McpTool_Tests : TestBase
{
    public McpTool_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void ShouldCreateStdioTransport()
    {
        // Arrange & Act
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "node",
            arguments: new[] { "server.js" },
            workingDirectory: "/test/path"
        );

        // Assert
        transport.ShouldNotBeNull();
        transport.ShouldBeOfType<StdioClientTransport>();
        transport.Name.ShouldBe("test-server");
    }

    [Fact]
    public void ShouldCreateHttpTransport()
    {
        // Arrange & Act
        var transport = McpTransportFactory.CreateHttpTransport(
            baseUrl: "http://localhost:8080"
        );

        // Assert
        transport.ShouldNotBeNull();
        transport.ShouldBeOfType<HttpClientTransport>();
    }

    [Fact]
    public void ShouldCreateHttpTransportWithAuth()
    {
        // Arrange & Act
        var transport = McpTransportFactory.CreateHttpTransportWithAuth(
            baseUrl: "http://localhost:8080",
            authToken: "test-token-123"
        );

        // Assert
        transport.ShouldNotBeNull();
        transport.ShouldBeOfType<HttpClientTransport>();
    }

    [Fact]
    public void ShouldCreateHttpTransportWithCustomHeaders()
    {
        // Arrange
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "CustomValue" },
            { "X-API-Version", "v1" }
        };

        // Act
        var transport = McpTransportFactory.CreateHttpTransportWithHeaders(
            baseUrl: "http://localhost:8080",
            headers: headers
        );

        // Assert
        transport.ShouldNotBeNull();
        transport.ShouldBeOfType<HttpClientTransport>();
    }

    [Fact]
    public void McpToolOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new McpToolOptions();

        // Assert
        options.ConnectionTimeoutMs.ShouldBe(30000);
        options.AutoReconnect.ShouldBeTrue();
        options.MaxReconnectAttempts.ShouldBe(3);
        options.ThrowOnToolCallFailure.ShouldBeFalse();
        options.IncludeDetailedErrors.ShouldBeTrue();
    }

    [Fact]
    public void McpToolOptions_ShouldAllowCustomization()
    {
        // Arrange & Act
        var options = new McpToolOptions
        {
            ConnectionTimeoutMs = 60000,
            AutoReconnect = false,
            MaxReconnectAttempts = 5,
            ThrowOnToolCallFailure = true,
            IncludeDetailedErrors = false
        };

        // Assert
        options.ConnectionTimeoutMs.ShouldBe(60000);
        options.AutoReconnect.ShouldBeFalse();
        options.MaxReconnectAttempts.ShouldBe(5);
        options.ThrowOnToolCallFailure.ShouldBeTrue();
        options.IncludeDetailedErrors.ShouldBeFalse();
    }

    // Note: The following tests require an actual MCP server running.
    // They will attempt to launch npx @modelcontextprotocol/server-everything

    [Fact]
    public async Task ShouldConnectToMcpServer_Stdio()
    {
        // This test requires a working MCP server that can be launched via stdio
        // Example: npx @modelcontextprotocol/server-everything

        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        // Act
        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        mcpTool.ShouldNotBeNull();
        mcpTool.IsConnected.ShouldBeTrue();
        mcpTool.Client.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldDiscoverToolsFromMcpServer()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        // Act
        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var availableFunctions = mcpTool.GetAvailableFunctions();

        // Assert
        availableFunctions.ShouldNotBeNull();
        availableFunctions.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldGenerateToolDeclarationsFromMcpServer()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        // Act
        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var tool = mcpTool.AsTool();

        // Assert
        tool.ShouldNotBeNull();
        tool.FunctionDeclarations.ShouldNotBeNull();
        tool.FunctionDeclarations.Count.ShouldBeGreaterThan(0);

        // Verify function declarations have required fields
        foreach (var declaration in tool.FunctionDeclarations)
        {
            declaration.Name.ShouldNotBeNullOrEmpty();
            declaration.Description.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task ShouldCallMcpToolFunction()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var availableFunctions = mcpTool.GetAvailableFunctions();

        // Assume the first function is available
        availableFunctions.Count.ShouldBeGreaterThan(0);
        var functionName = availableFunctions[0];

        var functionCall = new FunctionCall
        {
            Name = functionName,
            Args = new JsonObject() // Add appropriate args based on the function
        };

        // Act
        var response = await mcpTool.CallAsync(functionCall, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Name.ShouldBe(functionName);
        response.Response.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldIntegrateWithGeminiModel()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "filesystem-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);

        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
        model.AddFunctionTool(mcpTool);

        // Act
        var result = await model.GenerateContentAsync(
            "What tools are available to you?",
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        result.Text().ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Model response: {result.Text()}");
    }

    [Fact]
    public async Task ShouldRefreshToolsFromServer()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var initialFunctions = mcpTool.GetAvailableFunctions();

        // Act
        await mcpTool.RefreshToolsAsync(cancellationToken: TestContext.Current.CancellationToken);
        var refreshedFunctions = mcpTool.GetAvailableFunctions();

        // Assert
        refreshedFunctions.Count.ShouldBe(initialFunctions.Count);
    }

    [Fact]
    public async Task ShouldHandleConnectionWithTransportFactory()
    {
        // Arrange
        var factory = () => McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        // Act
        using var mcpTool = await McpTool.CreateAsync(factory, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        mcpTool.IsConnected.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateMultipleMcpToolsFromTransports()
    {
        // Arrange
        var transports = new List<IClientTransport>
        {
            McpTransportFactory.CreateStdioTransport(
                name: "server1",
                command: "npx",
                arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
            ),
            McpTransportFactory.CreateStdioTransport(
                name: "server2",
                command: "npx",
                arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
            )
        };

        // Act
        var mcpTools = await McpTool.CreateMultipleAsync(transports, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        mcpTools.Count.ShouldBe(2);
        mcpTools[0].IsConnected.ShouldBeTrue();
        mcpTools[1].IsConnected.ShouldBeTrue();

        // Cleanup
        foreach (var tool in mcpTools)
        {
            await tool.DisposeAsync();
        }
    }

    [Fact]
    public async Task ShouldGetFunctionInfo()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        using var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        var availableFunctions = mcpTool.GetAvailableFunctions();

        availableFunctions.Count.ShouldBeGreaterThan(0);
        var functionName = availableFunctions[0];

        // Act
        var functionInfo = mcpTool.GetFunctionInfo(functionName);

        // Assert
        functionInfo.ShouldNotBeNull();
        functionInfo.Name.ShouldBe(functionName);
        functionInfo.Description.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHandleErrorResponseGracefully()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        var options = new McpToolOptions
        {
            ThrowOnToolCallFailure = false,
            IncludeDetailedErrors = true
        };

        using var mcpTool = await McpTool.CreateAsync(transport, options, cancellationToken: TestContext.Current.CancellationToken);

        var invalidCall = new FunctionCall
        {
            Name = "nonexistent_function",
            Args = new JsonObject()
        };

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
        {
            await mcpTool.CallAsync(invalidCall, cancellationToken: TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task ShouldDisposeProperlyAsync()
    {
        // Arrange
        var transport = McpTransportFactory.CreateStdioTransport(
            name: "test-server",
            command: "npx",
            arguments: new[] { "-y", "@modelcontextprotocol/server-everything" }
        );

        var mcpTool = await McpTool.CreateAsync(transport, cancellationToken: TestContext.Current.CancellationToken);
        mcpTool.IsConnected.ShouldBeTrue();

        // Act
        await mcpTool.DisposeAsync();

        // Assert - after disposal, we can't really check IsConnected as client is disposed
        // Just verify no exception is thrown
    }

    [Fact]
    public void ShouldVerifyMcpToolOptionsDefaults()
    {
        // This is a unit test that doesn't require an MCP server
        var options = new McpToolOptions();

        options.ConnectionTimeoutMs.ShouldBe(30000);
        options.AutoReconnect.ShouldBeTrue();
        options.MaxReconnectAttempts.ShouldBe(3);
        options.ThrowOnToolCallFailure.ShouldBeFalse();
        options.IncludeDetailedErrors.ShouldBeTrue();
    }

    [Fact]
    public void ShouldCreateTransportFactoryMethods()
    {
        // Test that all factory methods create transports without throwing

        // Stdio transport
        var stdioTransport = McpTransportFactory.CreateStdioTransport(
            "test",
            "node",
            new[] { "server.js" }
        );
        stdioTransport.ShouldNotBeNull();

        // HTTP transport
        var httpTransport = McpTransportFactory.CreateHttpTransport("http://localhost:8080");
        httpTransport.ShouldNotBeNull();

        // HTTP with auth
        var authTransport = McpTransportFactory.CreateHttpTransportWithAuth(
            "http://localhost:8080",
            "token123"
        );
        authTransport.ShouldNotBeNull();

        // HTTP with headers
        var headersTransport = McpTransportFactory.CreateHttpTransportWithHeaders(
            "http://localhost:8080",
            new Dictionary<string, string> { { "X-Test", "value" } }
        );
        headersTransport.ShouldNotBeNull();
    }
}
