using GenerativeAI.Models;
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
            var functions = service.AsGoogleFunctions();
            var calls = service.AsGoogleCalls();
            
            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(apiKey, functions:functions,calls:calls);

            var result = await model.GenerateContentAsync("What is the weather in san francisco today?");
            
            Console.WriteLine(result);
        }

        [Fact]
        public async Task ShouldInvokeWetherService2()
        {
            WeatherService service = new WeatherService();
            var functions = service.AsGoogleFunctions();
            var calls = service.AsGoogleCalls();

            var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);

            var model = new GenerativeModel(apiKey);
            model.AddGlobalFunctions(functions,calls);
            var result = await model.GenerateContentAsync("What is the weather in san francisco today?");

            Console.WriteLine(result);
        }
    }
}