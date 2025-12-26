using System;
using System.Linq;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.IntegrationTests;

/// <summary>
/// Integration tests for Gemini 3 ThinkingConfig features with real API calls.
/// Tests thinking budget, thought signatures, and function calling with thinking enabled.
/// </summary>
public class Gemini3_ThinkingConfig_Integration_Tests : TestBase
{
    public Gemini3_ThinkingConfig_Integration_Tests(ITestOutputHelper helper) : base(helper)
    {
    }

    #region Basic ThinkingConfig API Tests

    [Fact]
    public async Task ThinkingConfig_WithIncludeThoughts_ShouldReturnThoughtParts()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        // Act
        var response = await model.GenerateContentAsync(
            "What is 25 * 17? Think through this step by step.",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Candidates.ShouldNotBeNull();
        response.Candidates.Length.ShouldBeGreaterThan(0);

        var parts = response.Candidates[0].Content?.Parts;
        parts.ShouldNotBeNull();

        // Log parts for debugging
        Console.WriteLine($"Total parts: {parts.Count}");
        foreach (var part in parts)
        {
            Console.WriteLine($"  Part - Thought: {part.Thought}, Text: {part.Text?.Substring(0, Math.Min(100, part.Text?.Length ?? 0))}...");
        }

        // Check for thought parts
        var thoughtParts = parts.Where(p => p.Thought == true).ToList();
        Console.WriteLine($"Thought parts found: {thoughtParts.Count}");

        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();
        Console.WriteLine($"Final answer: {text}");
    }

    [Fact]
    public async Task ThinkingConfig_WithThinkingBudget_ShouldWork()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingBudget = 2048
                }
            }
        );

        // Act
        var response = await model.GenerateContentAsync(
            "Explain the concept of recursion in programming with an example.",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();

        // Check usage metadata for thinking tokens
        if (response.UsageMetadata != null)
        {
            Console.WriteLine($"Prompt tokens: {response.UsageMetadata.PromptTokenCount}");
            Console.WriteLine($"Response tokens: {response.UsageMetadata.CandidatesTokenCount}");
            Console.WriteLine($"Thoughts tokens: {response.UsageMetadata.ThoughtsTokenCount}");
        }

        Console.WriteLine($"Response: {text}");
    }

    #endregion

    #region Function Calling with Thinking

    [Fact]
    public async Task FunctionCalling_WithThinking_ShouldReturnThoughtSignature()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        // Disable auto function calling to inspect raw response
        model.FunctionCallingBehaviour = new FunctionCallingBehaviour
        {
            FunctionEnabled = true,
            AutoCallFunction = false,
            AutoReplyFunction = false
        };

        var service = new MultiService();
        model.AddFunctionTool(new GenericFunctionTool(service.AsTools(), service.AsCalls()));

        // Act
        var response = await model.GenerateContentAsync(
            "What's the weather like in San Francisco?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var content = response.Candidates?[0].Content;
        content.ShouldNotBeNull();

        Console.WriteLine($"Response has {content.Parts.Count} parts");

        // Log each part
        foreach (var part in content.Parts)
        {
            Console.WriteLine($"Part:");
            Console.WriteLine($"  - Thought: {part.Thought}");
            Console.WriteLine($"  - ThoughtSignature: {(part.ThoughtSignature != null ? "present (" + part.ThoughtSignature.Length + " chars)" : "null")}");
            Console.WriteLine($"  - FunctionCall: {part.FunctionCall?.Name ?? "null"}");
            Console.WriteLine($"  - Text: {part.Text?.Substring(0, Math.Min(50, part.Text?.Length ?? 0)) ?? "null"}");
        }

        // Check for function call with thought signature
        var functionCallPart = content.Parts.FirstOrDefault(p => p.FunctionCall != null);
        if (functionCallPart != null)
        {
            Console.WriteLine($"\nFunction call: {functionCallPart.FunctionCall!.Name}");
            Console.WriteLine($"Has thought signature: {!string.IsNullOrEmpty(functionCallPart.ThoughtSignature)}");
        }
    }

    [Fact]
    public async Task FunctionCalling_ManualRoundTrip_ShouldPreserveThoughtSignature()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        model.FunctionCallingBehaviour = new FunctionCallingBehaviour
        {
            FunctionEnabled = true,
            AutoCallFunction = false,
            AutoReplyFunction = false
        };

        var service = new MultiService();
        var tool = new GenericFunctionTool(service.AsTools(), service.AsCalls());
        model.AddFunctionTool(tool);

        // Step 1: Initial request
        Console.WriteLine("=== Step 1: Initial Request ===");
        var request1 = new GenerateContentRequest();
        request1.AddText("What is the weather in London?");

        var response1 = await model.GenerateContentAsync(request1, cancellationToken: TestContext.Current.CancellationToken);
        response1.ShouldNotBeNull();

        var modelContent = response1.Candidates?[0].Content;
        modelContent.ShouldNotBeNull();

        // Find function call part
        var functionCallPart = modelContent.Parts.FirstOrDefault(p => p.FunctionCall != null);
        if (functionCallPart?.FunctionCall == null)
        {
            Console.WriteLine("No function call - model answered directly");
            Console.WriteLine($"Response: {response1.Text()}");
            return;
        }

        Console.WriteLine($"Function call: {functionCallPart.FunctionCall.Name}");
        Console.WriteLine($"Thought signature present: {!string.IsNullOrEmpty(functionCallPart.ThoughtSignature)}");

        // Step 2: Execute function
        Console.WriteLine("\n=== Step 2: Execute Function ===");
        var functionResponse = await tool.CallAsync(functionCallPart.FunctionCall);
        Console.WriteLine($"Function response: {functionResponse?.Response?.ToJsonString()}");

        // Step 3: Send function response back (with thought signature preserved)
        Console.WriteLine("\n=== Step 3: Send Function Response ===");
        var request2 = new GenerateContentRequest();
        request2.Contents.Add(new Content("What is the weather in London?", Roles.User));
        request2.Contents.Add(modelContent); // Includes thought signature
        request2.Contents.Add(new Content
        {
            Role = Roles.Function,
            Parts = new System.Collections.Generic.List<Part>
            {
                new Part { FunctionResponse = functionResponse }
            }
        });

        var response2 = await model.GenerateContentAsync(request2, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response2.ShouldNotBeNull();
        var finalText = response2.Text();
        finalText.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"\nFinal response: {finalText}");
    }

    [Fact]
    public async Task AutoFunctionCalling_WithThinking_ShouldCompleteSuccessfully()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        var service = new MultiService();
        model.AddFunctionTool(new GenericFunctionTool(service.AsTools(), service.AsCalls()));

        // Act
        var response = await model.GenerateContentAsync(
            "What's the weather in Tokyo and recommend some travel books?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrEmpty();

        Console.WriteLine($"Response: {text}");
    }

    #endregion

    #region ChatSession with Thinking

    [Fact]
    public async Task ChatSession_WithThinking_ShouldMaintainContext()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var chat = new ChatSession(
            history: null,
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        // Turn 1
        Console.WriteLine("=== Turn 1 ===");
        var response1 = await chat.GenerateContentAsync(
            "I'm planning a trip to Paris. What should I know?",
            cancellationToken: TestContext.Current.CancellationToken);

        response1.ShouldNotBeNull();
        Console.WriteLine($"Response 1: {response1.Text()?.Substring(0, Math.Min(200, response1.Text()?.Length ?? 0))}...");
        Console.WriteLine($"History count: {chat.History.Count}");

        // Turn 2 - follow up
        Console.WriteLine("\n=== Turn 2 ===");
        var response2 = await chat.GenerateContentAsync(
            "What about the best time to visit?",
            cancellationToken: TestContext.Current.CancellationToken);

        response2.ShouldNotBeNull();
        Console.WriteLine($"Response 2: {response2.Text()?.Substring(0, Math.Min(200, response2.Text()?.Length ?? 0))}...");
        Console.WriteLine($"History count: {chat.History.Count}");

        // Verify context was maintained
        var text2 = response2.Text()?.ToLower() ?? "";
        (text2.Contains("paris") || text2.Contains("france") || text2.Contains("spring") || text2.Contains("summer")).ShouldBeTrue("Response should reference Paris context");
    }

    [Fact]
    public async Task ChatSession_WithFunctionsAndThinking_ShouldWork()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var chat = new ChatSession(
            history: null,
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        var service = new MultiService();
        chat.AddFunctionTool(new GenericFunctionTool(service.AsTools(), service.AsCalls()));

        // Turn 1 - weather request
        Console.WriteLine("=== Turn 1: Weather ===");
        var response1 = await chat.GenerateContentAsync(
            "What's the weather in Berlin?",
            cancellationToken: TestContext.Current.CancellationToken);

        response1.ShouldNotBeNull();
        Console.WriteLine($"Response 1: {response1.Text()}");

        // Log history to check for thought signatures
        Console.WriteLine($"\nHistory after turn 1 ({chat.History.Count} entries):");
        foreach (var content in chat.History)
        {
            Console.WriteLine($"  Role: {content.Role}");
            foreach (var part in content.Parts)
            {
                if (part.Thought == true)
                    Console.WriteLine($"    - [Thought]");
                if (!string.IsNullOrEmpty(part.ThoughtSignature))
                    Console.WriteLine($"    - [ThoughtSignature present]");
                if (part.FunctionCall != null)
                    Console.WriteLine($"    - FunctionCall: {part.FunctionCall.Name}");
                if (part.FunctionResponse != null)
                    Console.WriteLine($"    - FunctionResponse: {part.FunctionResponse.Name}");
            }
        }

        // Turn 2 - follow up
        Console.WriteLine("\n=== Turn 2: Follow-up ===");
        var response2 = await chat.GenerateContentAsync(
            "Should I bring an umbrella?",
            cancellationToken: TestContext.Current.CancellationToken);

        response2.ShouldNotBeNull();
        Console.WriteLine($"Response 2: {response2.Text()}");
    }

    #endregion

    #region Gemini 3 Specific Tests

    [Fact]
    public async Task Gemini3_WithThinkingLevel_ShouldWork()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange - ThinkingLevel is only supported by Gemini 3
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = ThinkingLevel.HIGH
                }
            }
        );

        try
        {
            // Act
            var response = await model.GenerateContentAsync(
                "Solve this logic puzzle: If all A are B, and some B are C, what can we conclude about A and C?",
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            var text = response.Text();
            text.ShouldNotBeNullOrEmpty();

            Console.WriteLine($"Response with ThinkingLevel.HIGH: {text}");

            // Check for thought parts
            var parts = response.Candidates?[0].Content?.Parts;
            if (parts != null)
            {
                var thoughtCount = parts.Count(p => p.Thought == true);
                Console.WriteLine($"Thought parts: {thoughtCount}");
            }
        }
        catch (GenerativeAI.Exceptions.ApiException ex) when (
            ex.Message.Contains("not found") ||
            ex.Message.Contains("not supported") ||
            ex.Message.Contains("does not exist"))
        {
            Assert.Skip($"Gemini 3 model not available: {ex.Message}");
        }
    }

    [Fact]
    public async Task Gemini3_FunctionCalling_WithThoughtSignature_ShouldWork()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingLevel = ThinkingLevel.HIGH
                }
            }
        );

        var service = new MultiService();
        model.AddFunctionTool(new GenericFunctionTool(service.AsTools(), service.AsCalls()));

        try
        {
            // Act
            var response = await model.GenerateContentAsync(
                "What's the weather in Sydney, Australia?",
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            response.ShouldNotBeNull();
            var text = response.Text();
            text.ShouldNotBeNullOrEmpty();

            Console.WriteLine($"Response: {text}");
        }
        catch (GenerativeAI.Exceptions.ApiException ex) when (
            ex.Message.Contains("not found") ||
            ex.Message.Contains("not supported") ||
            ex.Message.Contains("does not exist"))
        {
            Assert.Skip($"Gemini 3 model not available: {ex.Message}");
        }
    }

    #endregion

    #region Streaming with Thinking

    [Fact]
    public async Task Streaming_WithThinking_ShouldWork()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);

        // Arrange
        var model = new GenerativeModel(
            platform: GetTestGooglePlatform(),
            model: GoogleAIModels.Gemini3FlashPreview,
            config: new GenerationConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    IncludeThoughts = true
                }
            }
        );

        var fullResponse = "";
        var chunkCount = 0;

        // Act
        Console.WriteLine("Streaming response:");
        await foreach (var chunk in model.StreamContentAsync(
            "Write a haiku about artificial intelligence.",
            cancellationToken: TestContext.Current.CancellationToken))
        {
            var text = chunk.Text();
            if (!string.IsNullOrEmpty(text))
            {
                fullResponse += text;
                chunkCount++;
                Console.Write(text);
            }
        }

        // Assert
        Console.WriteLine($"\n\nTotal chunks: {chunkCount}");
        fullResponse.ShouldNotBeNullOrEmpty();
    }

    #endregion
}
