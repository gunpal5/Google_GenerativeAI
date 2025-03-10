using CSharpToJsonSchema;
using GenerativeAI.Microsoft;
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

        var tools = new Tools([GetCurrentWeather]);
        chatOptions.Tools = tools.AsMeaiTools();
        var message = new ChatMessage(ChatRole.User, "What is the weather in New York in celsius?");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions).ConfigureAwait(false);

        Console.WriteLine(response.Choices.LastOrDefault().Text);
        response.Choices.LastOrDefault().Text.Contains("New York", StringComparison.InvariantCultureIgnoreCase);
    }
    
  
    public async Task ShouldWorkWith_BookStoreService()
    {
       
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);
        var chatClient = new GenerativeAIChatClient(apiKey);
        var chatOptions = new ChatOptions();
        
       
        var tools = new Tools([GetBookPageContentAsync]);
        chatOptions.Tools = tools.AsMeaiTools();
       
        var message = new ChatMessage(ChatRole.User, "what is written on page 96 in the book 'damdamadum'");
        var response = await chatClient.GetResponseAsync(message,options:chatOptions).ConfigureAwait(false);

        response.Choices.LastOrDefault().Text.ShouldContain("damdamadum",Case.Insensitive);
    }
    
    [FunctionTool(MeaiFunctionTool = true)]
    [System.ComponentModel.Description("Get book page content")]
    public static Task<string> GetBookPageContentAsync(string bookName, int bookPageNumber, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("this is a cool weather out there, and I am stuck at home.");
    }
    
    [FunctionTool(MeaiFunctionTool = true)]
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