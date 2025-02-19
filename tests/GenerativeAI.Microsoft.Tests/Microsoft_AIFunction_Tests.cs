using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Microsoft;

using ChatOptions = Microsoft.Extensions.AI.ChatOptions;
using GenerativeAI.Microsoft.Extensions;
using GenerativeAI.Tests;
using Json.More;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using AITool = Microsoft.Extensions.AI.AITool;

namespace GenerativeAI.IntegrationTests;

public class Microsoft_AIFunction_Tests:TestBase
{
    [Fact]
    public async Task ShouldWorkWithTools()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
        
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetCurrentWeather)};
        var message = new ChatMessage(ChatRole.User, "What is the weather in New York?");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions).ConfigureAwait(false);

        response.Choices.LastOrDefault().Text.Contains("New York", StringComparison.InvariantCultureIgnoreCase)
            .ShouldBeTrue();
    }
    
    [Fact]
    public async Task ShouldWorkWith_BookStoreService()
    {
        Assert.SkipUnless(IsGeminiApiKeySet,GeminiTestSkipMessage);
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
        
       
        chatOptions.Tools = new List<AITool>{AIFunctionFactory.Create(GetBookPageContentAsync,new AIFunctionFactoryOptions()
        {
            
        })};
        var message = new ChatMessage(ChatRole.User, "what is written on page 96 in the book 'damdamadum'");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions).ConfigureAwait(false);

        response.Choices.LastOrDefault().Text.ShouldContain("damdamadum",Case.Insensitive);
    }
    
    [System.ComponentModel.Description("Get book page content")]
    public static Task<string> GetBookPageContentAsync(string bookName, int bookPageNumber)
    {
        return Task.FromResult("this is a cool weather out there, and I am stuck at home.");
    }
    
    [System.ComponentModel.Description("Get the current weather in a given location")]
    public Weather GetCurrentWeather(string location, Unit unit = Unit.Celsius)
    {
        return new Weather
        {
            Location = location,
            Temperature = 30.0,
            Unit = unit,
            Description = "Sunny",
        };
    }
    
    public enum Unit
    {
        Celsius,
        Fahrenheit,
        Imperial
    }

    public class Weather
    {
        public string Location { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public Unit Unit { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}