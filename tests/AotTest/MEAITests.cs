using System.Text.Json.Serialization;
using CSharpToJsonSchema;
using GenerativeAI;
using GenerativeAI.Microsoft;
using GenerativeAI.Tools;
using Microsoft.Extensions.AI;
using Shouldly;


namespace AotTest;

public class MEAITests
{
    public async Task ShouldWorkWithTools()
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();

        var tools = new Tools([GetCurrentWeatherAsync]);
        chatOptions.Tools = tools.AsMeaiTools();
        var message = new ChatMessage(ChatRole.User, "What is the weather in New York in celsius?");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions, cancellationToken: CancellationToken.None);

        Console.WriteLine(response.Text);
        response.Text.Contains("New York", StringComparison.InvariantCultureIgnoreCase);
    }
    
    public async Task QuickToolTest()
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
        DefaultSerializerOptions.CustomJsonTypeResolvers.Add(MeaiTestJsonSerializerContext.Default);
        chatOptions.Tools = [new QuickTool(GetCurrentWeatherAsync).AsMeaiTool()];
        
        var message = new ChatMessage(ChatRole.User, "What is the weather in New York in celsius?");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions, cancellationToken: CancellationToken.None);
        Console.WriteLine(response.Text);
    }
    
    public async Task ShouldWorkWith_BookStoreService()
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
       
        var tools = new Tools([GetBookPageContentAsync]);
        chatOptions.Tools = tools.AsMeaiTools();
       
        var message = new ChatMessage(ChatRole.User, "what is written on page 96 in the book 'damdamadum'");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions, cancellationToken: CancellationToken.None);

        response.Text.ShouldContain("damdamadum",Case.Insensitive);
    }
    
    [FunctionTool(MeaiFunctionTool = true)]
    [System.ComponentModel.Description("Get book page content")]
    public static Task<string> GetBookPageContentAsync(string bookName, int bookPageNumber, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("this is a cool weather out there, and I am stuck at home.");
    }
    
    [FunctionTool(MeaiFunctionTool = true)]
    [System.ComponentModel.Description("Get the current weather in a given location")]
    public Task<Weather> GetCurrentWeatherAsync(string location, Unit unit = Unit.Celsius, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Weather
        {
            Location = location,
            Temperature = 30.0,
            Unit = unit,
            Description = "Sunny",
        });
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

[JsonSerializable(typeof(MEAITests.Weather))]
[JsonSerializable(typeof(MEAITests.Unit))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
public partial class MeaiTestJsonSerializerContext : JsonSerializerContext
{
        
}