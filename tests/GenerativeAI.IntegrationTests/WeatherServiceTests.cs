using GenerativeAI.Models;
using TryAgiOpenAITests;
using Xunit.Abstractions;

namespace GenerativeAI.IntegrationTests
{
    public class WeatherServiceTests
    {
        private ITestOutputHelper Console;
        public WeatherServiceTests(ITestOutputHelper helper)
        {
            this.Console = helper;
        }
        [Fact]
        public async Task ShouldInvokeWetherService()
        {
            WeatherService service = new WeatherService();
            var functions = service.AsFunctions();
            var calls = service.AsCalls();
            
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(apiKey, functions:functions,calls:calls);

            var result = await model.GenerateContentAsync("What is the weather in san fransisco today?");
            
            Console.WriteLine(result);
        }
    }
}