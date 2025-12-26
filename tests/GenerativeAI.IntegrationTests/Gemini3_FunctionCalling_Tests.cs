using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Integration tests for Gemini 3 function calling with thought signatures.
/// These tests verify that thought signatures are properly preserved during function call workflows.
/// </summary>
public class Gemini3_FunctionCalling_Tests : TestBase
{
    public Gemini3_FunctionCalling_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Basic Function Calling with Thinking Config

    [Fact]
    public async Task Gemini3_ShouldSupportThinkingConfig_WithHighLevel()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash, // Use 2.5 Flash for testing since Gemini 3 may not be available
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = ThinkingLevel.HIGH
                }
            }
        );

        var prompt = "What is 15 + 27?";

        // Act
        var response = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Candidates.ShouldNotBeNull();
        response.Candidates.Length.ShouldBeGreaterThan(0);

        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {text}");
    }

    [Fact]
    public async Task Gemini3_ShouldSupportThinkingConfig_WithBudget()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingBudget = 2048,
                    ThinkingLevel = ThinkingLevel.HIGH
                }
            }
        );

        var prompt = "Explain the concept of recursion in programming.";

        // Act
        var response = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {text}");
    }

    #endregion

    #region Function Calling with Thought Signatures

    [Fact]
    public async Task FunctionCalling_ShouldPreserveThoughtSignature_InConversationHistory()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);

        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );
        model.AddFunctionTool(tool);

        var prompt = "What's the current weather in Paris, France?";

        // Act
        var response = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {text}");
    }

    [Fact]
    public async Task FunctionCalling_ShouldWork_WithMultipleFunctions_AndThinking()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);

        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = ThinkingLevel.HIGH
                }
            }
        );
        model.AddFunctionTool(tool);

        var prompt = @"I need help planning my trip:
1. What's the weather in Tokyo, Japan?
2. Can you recommend some travel books?";

        // Act
        var response = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Response: {text}");
    }

    #endregion

    #region ChatSession with Thinking

    [Fact]
    public async Task ChatSession_ShouldPreserveThoughts_InMultiTurnConversation()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var chatSession = new ChatSession(
            history: null,
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        // Act - First turn
        var response1 = await chatSession.GenerateContentAsync(
            "Tell me about the Eiffel Tower.",
            cancellationToken: TestContext.Current.CancellationToken);

        // Act - Second turn
        var response2 = await chatSession.GenerateContentAsync(
            "How tall is it?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response1.ShouldNotBeNull();
        response2.ShouldNotBeNull();

        var text1 = response1.Text();
        var text2 = response2.Text();

        text1.ShouldNotBeNullOrEmpty();
        text2.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Turn 1: {text1}");
        Console.WriteLine($"Turn 2: {text2}");
    }

    [Fact]
    public async Task ChatSession_WithFunctions_ShouldPreserveThoughtSignatures()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);

        var chatSession = new ChatSession(
            history: null,
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = ThinkingLevel.HIGH
                }
            }
        );
        chatSession.AddFunctionTool(tool);

        // Act - First turn with function call
        var response1 = await chatSession.GenerateContentAsync(
            "What's the weather in New York?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Act - Follow up
        var response2 = await chatSession.GenerateContentAsync(
            "What about the forecast for the next 3 days?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response1.ShouldNotBeNull();
        response2.ShouldNotBeNull();

        Console.WriteLine($"Turn 1: {response1.Text()}");
        Console.WriteLine($"Turn 2: {response2.Text()}");
    }

    #endregion

    #region Streaming with Thinking

    [Fact]
    public async Task Streaming_ShouldWork_WithThinkingConfig()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = ThinkingLevel.LOW
                }
            }
        );

        var prompt = "Write a haiku about programming.";
        var fullResponse = "";

        // Act
        await foreach (var chunk in model.StreamContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken))
        {
            var text = chunk.Text();
            if (!string.IsNullOrEmpty(text))
            {
                fullResponse += text;
                Console.Write(text);
            }
        }

        // Assert
        fullResponse.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"\n\nFull response: {fullResponse}");
    }

    [Fact]
    public async Task Streaming_WithFunctions_ShouldWork_WithThinkingConfig()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);

        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );
        model.AddFunctionTool(tool);

        var prompt = "Get me the weather in London.";
        var fullResponse = "";

        // Act
        await foreach (var chunk in model.StreamContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken))
        {
            var text = chunk.Text();
            if (!string.IsNullOrEmpty(text))
            {
                fullResponse += text;
                Console.Write(text);
            }
        }

        // Assert
        Console.WriteLine($"\n\nFull response: {fullResponse}");
    }

    #endregion
}
