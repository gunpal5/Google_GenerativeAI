using System;
using System.Threading.Tasks;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using Xunit;

namespace GenerativeAI.IntegrationTests;

public class ParallelFunctionCallingTests : TestBase
{
    public ParallelFunctionCallingTests(ITestOutputHelper helper) : base(helper)
    {
    }
    
    [Fact]
    public async Task ShouldInvokeMultipleFunctions()
    {
        Assert.SkipUnless(IsGeminiApiKeySet, GeminiTestSkipMessage);
        
        // Set up the service with multiple functions
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);
        
        // Create and configure the model
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
        model.AddFunctionTool(tool);
        
        // Generate a prompt that should trigger multiple function calls
        string prompt = "I'm planning a trip to Paris, France and need some information. " +
                        "What's the current weather there? " +
                        "Also, could you recommend some books about French history? " +
                        "What's the weather forecast for the next few days in Paris?";
        
        // Execute the request
        var result = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);
        
        // Output the response
        Console.WriteLine(result.Text());
    }
    
    [Fact]
    public async Task ShouldInvokeMultipleFunctions_WithStreaming()
    {
        Assert.SkipUnless(IsGeminiApiKeySet, GeminiTestSkipMessage);
        
        // Set up the service with multiple functions
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);
        
        // Create and configure the model
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
        model.AddFunctionTool(tool);
        
        // Generate a prompt that should trigger multiple function calls
        string prompt = "I'm planning a trip to Paris, France and need some information. " +
                        "What's the current weather there? " +
                        "Also, could you recommend some books about French history? " +
                        "What's the weather forecast for the next few days in Paris?";
        
        // Execute the streaming request
        await foreach (var result in model.StreamContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken))
        {
            Console.WriteLine(result.Text());
        }
    }
    
    [Fact]
    public async Task ShouldInvokeMultipleFunctions_ComplexRequest()
    {
        Assert.SkipUnless(IsGeminiApiKeySet, GeminiTestSkipMessage);
        
        // Set up the service with multiple functions
        var service = new MultiService();
        var tools = service.AsTools();
        var calls = service.AsCalls();
        var tool = new GenericFunctionTool(tools, calls);
        
        // Create and configure the model
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
        model.AddFunctionTool(tool);
        
        // Complex prompt designed to trigger multiple function calls in parallel
        string prompt = @"I need comprehensive information for my upcoming vacation:
1. What's the current weather in New York, USA?
2. What's the current weather in Tokyo, Japan?
3. What's the weather forecast for New York for the next 5 days?
4. Can you recommend some science fiction books to read on my trip?
5. I also want to read some mystery books during my journey.

Please provide all this information organized in a clear way.";
        
        // Execute the request
        var result = await model.GenerateContentAsync(prompt, cancellationToken: TestContext.Current.CancellationToken);
        
        // Output the response
        Console.WriteLine(result.Text());
    }
}