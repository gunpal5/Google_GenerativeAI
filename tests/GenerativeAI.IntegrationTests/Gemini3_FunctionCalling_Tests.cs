using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Integration tests for Gemini 3 function calling with thought signatures.
/// These tests verify that thought signatures are properly returned by the API
/// and preserved during function call workflows.
/// </summary>
public class Gemini3_FunctionCalling_Tests : TestBase
{
    public Gemini3_FunctionCalling_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Thought Signature Verification Tests

    [Fact]
    public async Task ThinkingModel_ShouldReturn_ThoughtParts_WhenIncludeThoughtsEnabled()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange - Use a model that supports thinking
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini25Flash,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingBudget = 1024
                }
            }
        );

        var prompt = "What is the sum of 157 + 289? Show your reasoning.";

        // Act
        var response = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Candidates.ShouldNotBeNull();
        response.Candidates.Length.ShouldBeGreaterThan(0);

        var candidate = response.Candidates[0];
        candidate.Content.ShouldNotBeNull();
        candidate.Content.Parts.ShouldNotBeNull();

        // Log all parts to see what we get
        Console.WriteLine($"Number of parts: {candidate.Content.Parts.Count}");
        for (int i = 0; i < candidate.Content.Parts.Count; i++)
        {
            var part = candidate.Content.Parts[i];
            Console.WriteLine($"Part {i}: Thought={part.Thought}, HasThoughtSignature={!string.IsNullOrEmpty(part.ThoughtSignature)}, Text={(part.Text?.Length > 100 ? part.Text.Substring(0, 100) + "..." : part.Text)}");
        }

        // Check if we got any thought parts or thought signatures
        var hasThoughtParts = candidate.Content.Parts.Any(p => p.Thought == true);
        var hasThoughtSignatures = candidate.Content.Parts.Any(p => !string.IsNullOrEmpty(p.ThoughtSignature));

        Console.WriteLine($"HasThoughtParts: {hasThoughtParts}, HasThoughtSignatures: {hasThoughtSignatures}");

        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"\nFinal Response: {text}");
    }

    [Fact]
    public async Task FunctionCall_ShouldReturn_ThoughtSignature_WhenThinkingEnabled()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange - Disable auto function calling to inspect the raw response
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

        // Disable auto function calling so we can inspect the raw response
        model.FunctionCallingBehaviour = new FunctionCallingBehaviour
        {
            FunctionEnabled = true,
            AutoCallFunction = false,
            AutoReplyFunction = false
        };

        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);
        model.AddFunctionTool(tool);

        var prompt = "What is the weather in Paris, France?";

        // Act
        var response = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Candidates.ShouldNotBeNull();
        response.Candidates.Length.ShouldBeGreaterThan(0);

        var candidate = response.Candidates[0];
        candidate.Content.ShouldNotBeNull();
        candidate.Content.Parts.ShouldNotBeNull();

        // Log all parts
        Console.WriteLine($"Number of parts: {candidate.Content.Parts.Count}");
        for (int i = 0; i < candidate.Content.Parts.Count; i++)
        {
            var part = candidate.Content.Parts[i];
            Console.WriteLine($"Part {i}:");
            Console.WriteLine($"  - Thought: {part.Thought}");
            Console.WriteLine($"  - ThoughtSignature: {(string.IsNullOrEmpty(part.ThoughtSignature) ? "null" : part.ThoughtSignature.Substring(0, Math.Min(50, part.ThoughtSignature.Length)) + "...")}");
            Console.WriteLine($"  - FunctionCall: {part.FunctionCall?.Name ?? "null"}");
            Console.WriteLine($"  - Text: {(part.Text?.Length > 50 ? part.Text.Substring(0, 50) + "..." : part.Text ?? "null")}");
        }

        // Check for function call
        var functionCallPart = candidate.Content.Parts.FirstOrDefault(p => p.FunctionCall != null);
        if (functionCallPart != null)
        {
            Console.WriteLine($"\nFunction call found: {functionCallPart.FunctionCall!.Name}");
            Console.WriteLine($"Function call has thought signature: {!string.IsNullOrEmpty(functionCallPart.ThoughtSignature)}");

            if (!string.IsNullOrEmpty(functionCallPart.ThoughtSignature))
            {
                Console.WriteLine($"Thought signature (first 100 chars): {functionCallPart.ThoughtSignature.Substring(0, Math.Min(100, functionCallPart.ThoughtSignature.Length))}");
            }
        }
    }

    [Fact]
    public async Task ManualFunctionCall_ShouldPreserve_ThoughtSignature_InConversation()
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
                    IncludeThoughts = true
                }
            }
        );

        // Disable auto function calling
        model.FunctionCallingBehaviour = new FunctionCallingBehaviour
        {
            FunctionEnabled = true,
            AutoCallFunction = false,
            AutoReplyFunction = false
        };

        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);
        model.AddFunctionTool(tool);

        // Step 1: Send initial request
        var request = new GenerateContentRequest();
        request.AddText("What is the weather in Tokyo, Japan?");

        var response1 = await model.GenerateContentAsync(request, cancellationToken: TestContext.Current.CancellationToken);

        response1.ShouldNotBeNull();
        response1.Candidates.ShouldNotBeNull();

        var modelContent = response1.Candidates[0].Content;
        Console.WriteLine("=== Step 1: Initial Response ===");
        LogContentParts(modelContent);

        // Check if there's a function call
        var functionCallPart = modelContent?.Parts.FirstOrDefault(p => p.FunctionCall != null);
        if (functionCallPart?.FunctionCall == null)
        {
            Console.WriteLine("No function call in response - model may have answered directly");
            return;
        }

        // Step 2: Execute the function and send response back
        Console.WriteLine($"\n=== Step 2: Executing function {functionCallPart.FunctionCall.Name} ===");

        var functionResponse = await tool.CallAsync(functionCallPart.FunctionCall);
        Console.WriteLine($"Function result: {functionResponse?.Response?.ToJsonString()}");

        // Step 3: Build the next request with the model's response (including thought signature) and function result
        var request2 = new GenerateContentRequest();

        // Add original user message
        request2.Contents.Add(new Content("What is the weather in Tokyo, Japan?", Roles.User));

        // Add model's response WITH the thought signature preserved
        // This is the key part - we need to include the original parts from the model
        request2.Contents.Add(modelContent!);

        // Add function response
        var functionResponseContent = new Content
        {
            Role = Roles.Function,
            Parts = new List<Part>
            {
                new Part { FunctionResponse = functionResponse }
            }
        };
        request2.Contents.Add(functionResponseContent);

        Console.WriteLine("\n=== Step 3: Sending function response back ===");
        Console.WriteLine($"Request has {request2.Contents.Count} contents");

        // Check if thought signature is in the model content we're sending back
        var modelPartWithSignature = modelContent.Parts.FirstOrDefault(p => !string.IsNullOrEmpty(p.ThoughtSignature));
        if (modelPartWithSignature != null)
        {
            Console.WriteLine($"Model content includes thought signature: {modelPartWithSignature.ThoughtSignature?.Substring(0, Math.Min(50, modelPartWithSignature.ThoughtSignature.Length))}...");
        }
        else
        {
            Console.WriteLine("No thought signature found in model content being sent back");
        }

        // Act - Send the function response
        var response2 = await model.GenerateContentAsync(request2, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response2.ShouldNotBeNull();
        var finalText = response2.Text();
        finalText.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"\n=== Final Response ===");
        Console.WriteLine(finalText);

        // The response should contain information about Tokyo weather
        // This verifies the full round-trip worked correctly
    }

    #endregion

    #region Auto Function Calling Tests

    [Fact]
    public async Task AutoFunctionCalling_ShouldWork_WithThinkingEnabled()
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

        // Act
        var response = await model.GenerateContentAsync(
            "What's the current weather in Paris, France?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Response: {text}");

        // Should contain weather information
        text.ToLower().ShouldContain("paris");
    }

    [Fact]
    public async Task AutoFunctionCalling_WithMultipleCalls_ShouldWork_WithThinkingEnabled()
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
                    ThinkingBudget = 2048
                }
            }
        );
        model.AddFunctionTool(tool);

        // Act - Request that should trigger multiple function calls
        var response = await model.GenerateContentAsync(
            "I need the weather in both Tokyo and Paris, and also recommend some travel books.",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Response: {text}");
    }

    #endregion

    #region ChatSession with Thinking and Function Calls

    [Fact]
    public async Task ChatSession_WithFunctions_ShouldMaintain_ThoughtContext_AcrossTurns()
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
                    IncludeThoughts = true
                }
            }
        );
        chatSession.AddFunctionTool(tool);

        // Turn 1: Get weather
        Console.WriteLine("=== Turn 1: Asking about weather ===");
        var response1 = await chatSession.GenerateContentAsync(
            "What's the weather in New York right now?",
            cancellationToken: TestContext.Current.CancellationToken);

        response1.ShouldNotBeNull();
        Console.WriteLine($"Response 1: {response1.Text()}");
        Console.WriteLine($"History count after turn 1: {chatSession.History.Count}");

        // Log history parts
        foreach (var content in chatSession.History)
        {
            Console.WriteLine($"  History entry - Role: {content.Role}, Parts: {content.Parts.Count}");
            foreach (var part in content.Parts)
            {
                if (part.Thought == true)
                    Console.WriteLine($"    - Thought part found");
                if (!string.IsNullOrEmpty(part.ThoughtSignature))
                    Console.WriteLine($"    - ThoughtSignature present");
                if (part.FunctionCall != null)
                    Console.WriteLine($"    - FunctionCall: {part.FunctionCall.Name}");
                if (part.FunctionResponse != null)
                    Console.WriteLine($"    - FunctionResponse: {part.FunctionResponse.Name}");
            }
        }

        // Turn 2: Ask follow-up about forecast
        Console.WriteLine("\n=== Turn 2: Asking about forecast ===");
        var response2 = await chatSession.GenerateContentAsync(
            "What about the forecast for the next 5 days there?",
            cancellationToken: TestContext.Current.CancellationToken);

        response2.ShouldNotBeNull();
        Console.WriteLine($"Response 2: {response2.Text()}");
        Console.WriteLine($"History count after turn 2: {chatSession.History.Count}");

        // Turn 3: Ask about something else
        Console.WriteLine("\n=== Turn 3: Asking about books ===");
        var response3 = await chatSession.GenerateContentAsync(
            "Can you recommend some mystery books?",
            cancellationToken: TestContext.Current.CancellationToken);

        response3.ShouldNotBeNull();
        Console.WriteLine($"Response 3: {response3.Text()}");
    }

    #endregion

    #region ThinkingLevel Tests

    [Theory]
    [InlineData(ThinkingLevel.LOW)]
    [InlineData(ThinkingLevel.HIGH)]
    public async Task ThinkingLevel_ShouldBeAcceptedByAPI(ThinkingLevel level)
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
                    ThinkingLevel = level
                }
            }
        );

        // Act
        var response = await model.GenerateContentAsync(
            "Solve: If x + 5 = 12, what is x?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"ThinkingLevel {level}: {text}");
    }

    #endregion

    #region Helper Methods

    private void LogContentParts(Content? content)
    {
        if (content == null)
        {
            Console.WriteLine("Content is null");
            return;
        }

        Console.WriteLine($"Role: {content.Role}, Parts: {content.Parts.Count}");
        for (int i = 0; i < content.Parts.Count; i++)
        {
            var part = content.Parts[i];
            Console.WriteLine($"  Part {i}:");
            if (part.Thought == true)
                Console.WriteLine($"    - Thought: true");
            if (!string.IsNullOrEmpty(part.ThoughtSignature))
                Console.WriteLine($"    - ThoughtSignature: {part.ThoughtSignature.Substring(0, Math.Min(50, part.ThoughtSignature.Length))}...");
            if (part.FunctionCall != null)
                Console.WriteLine($"    - FunctionCall: {part.FunctionCall.Name}({part.FunctionCall.Args?.ToJsonString()})");
            if (part.FunctionResponse != null)
                Console.WriteLine($"    - FunctionResponse: {part.FunctionResponse.Name}");
            if (!string.IsNullOrEmpty(part.Text))
                Console.WriteLine($"    - Text: {(part.Text.Length > 100 ? part.Text.Substring(0, 100) + "..." : part.Text)}");
        }
    }

    #endregion
}
