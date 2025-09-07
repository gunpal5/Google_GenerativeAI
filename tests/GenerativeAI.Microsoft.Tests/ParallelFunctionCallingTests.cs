using System.ComponentModel;
using System.Text;
using System.Text.Json;
using GenerativeAI.Microsoft;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests;
using GenerativeAI.Tools;
using Microsoft.Extensions.AI;
using Shouldly;
using Xunit;
using AITool = Microsoft.Extensions.AI.AITool;
using ChatOptions = Microsoft.Extensions.AI.ChatOptions;

namespace GenerativeAI.Microsoft.Tests;

public class ParallelFunctionCallingTests : TestBase
{
    public ParallelFunctionCallingTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task ShouldCallMultipleFunctionsInParallel()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, GoogleAIModels.Gemini2Flash);
        var chatOptions = new ChatOptions();

        chatOptions.Tools = new List<AITool>
        {
            AIFunctionFactory.Create(GetWeatherInfo),
            AIFunctionFactory.Create(GetTimeInfo)
        };

        var message = new ChatMessage(ChatRole.User, "What's the weather and time in New York?");
        var response = await chatClient.GetResponseAsync(message, options: chatOptions, cancellationToken: TestContext.Current.CancellationToken);

        response.Text.ShouldContain("weather", Case.Insensitive);
        response.Text.ShouldContain("time", Case.Insensitive);
        response.Text.ShouldContain("New York", Case.Insensitive);
    }

    [Fact]
    public async Task ShouldCallMultipleFunctionsInParallelWithStreaming()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, GoogleAIModels.Gemini2Flash);
        var chatOptions = new ChatOptions();

        chatOptions.Tools = new List<AITool>
        {
            AIFunctionFactory.Create(GetWeatherInfo),
            AIFunctionFactory.Create(GetTimeInfo)
        };

        var message = new ChatMessage(ChatRole.User, "What's the weather and time in New York?");
        var responseText = new StringBuilder();
        
        await foreach (var response in chatClient.GetStreamingResponseAsync(message, options: chatOptions, cancellationToken: TestContext.Current.CancellationToken))
        {
            Console.WriteLine(response.Text);
            responseText.Append(response.Text);
        }

        responseText.ToString().ShouldContain("weather", Case.Insensitive);
        responseText.ToString().ShouldContain("time", Case.Insensitive);
    }

    [Fact]
    public async Task ShouldCallComplexParallelFunctions()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, GoogleAIModels.Gemini2Flash);
        var chatOptions = new ChatOptions();

        chatOptions.Tools = new List<AITool>
        {
            AIFunctionFactory.Create(GetWeatherInfo),
            AIFunctionFactory.Create(GetTimeInfo),
            AIFunctionFactory.Create(GetStockPrice)
        };

        var message = new ChatMessage(ChatRole.User, "What's the weather and time in New York, and what's the current price of AAPL stock?");
        var response = await chatClient.GetResponseAsync(message, options: chatOptions, cancellationToken: TestContext.Current.CancellationToken);

        response.Text.ShouldContain("weather", Case.Insensitive);
        response.Text.ShouldContain("time", Case.Insensitive);
//        response.Text.ShouldContain("stock", Case.Insensitive);
    }

    [Fact]
    public async Task ShouldCombineParallelFunctionResults()
    {
        Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey, GoogleAIModels.Gemini2Flash);
        var chatOptions = new ChatOptions();

        chatOptions.Tools = new List<AITool>
        {
            AIFunctionFactory.Create(GetFlightInfo),
            AIFunctionFactory.Create(GetHotelInfo)
        };

        var message = new ChatMessage(ChatRole.User, "I want to plan a trip to Paris. What flights and hotels are available?");
        var response = await chatClient.GetResponseAsync(message, options: chatOptions, cancellationToken: TestContext.Current.CancellationToken);

        response.Text.ShouldContain("flight", Case.Insensitive);
        response.Text.ShouldContain("hotel", Case.Insensitive);
        response.Text.ShouldContain("Paris", Case.Insensitive);
    }

    // [Fact]
    // public async Task ShouldHandleParallelFunctionErrorsGracefully()
    // {
    //     Assert.SkipUnless(IsGoogleApiKeySet, GoogleTestSkipMessage);
    //     var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);
    //     var chatClient = new GenerativeAIChatClient(apiKey, GoogleAIModels.Gemini2Flash);
    //     var chatOptions = new ChatOptions();
    //
    //     chatOptions.Tools = new List<AITool>
    //     {
    //         AIFunctionFactory.Create(GetWeatherInfo),
    //         AIFunctionFactory.Create(GetErrorFunction)
    //     };
    //
    //     var message = new ChatMessage(ChatRole.User, "What's the weather in New York and can you also trigger an error?");
    //     var response = await chatClient.GetResponseAsync(message, options: chatOptions);
    //
    //     response.Text.ShouldContain("weather", Case.Insensitive);
    //     response.Text.ShouldContain("New York", Case.Insensitive);
    // }

    [Description("Get weather information for a location")]
    public string GetWeatherInfo(string location)
    {
        return $"The weather in {location} is sunny and 75�F.";
    }

    [Description("Get current time for a location")]
    public string GetTimeInfo(string location)
    {
        return $"The current time in {location} is {DateTime.Now.ToString("h:mm tt")}.";
    }

    [Description("Get stock price information")]
    public string GetStockPrice(string symbol)
    {
        var prices = new Dictionary<string, decimal>
        {
            { "AAPL", 180.95m },
            { "MSFT", 410.34m },
            { "GOOG", 170.25m }
        };

        return prices.TryGetValue(symbol.ToUpper(), out var price)
            ? $"The current price of {symbol} is ${price}."
            : $"Could not find price information for {symbol}.";
    }

    [Description("Get flight information to a destination")]
    public string GetFlightInfo(string destination)
    {
        return $"There are 5 available flights to {destination} starting at $450.";
    }

    [Description("Get hotel information at a location")]
    public string GetHotelInfo(string location)
    {
        return $"There are 10 hotels available in {location} starting at $120 per night.";
    }

    [Description("Function that always throws an error")]
    public string GetErrorFunction(string input)
    {
        throw new Exception("This function always fails");
    }
}