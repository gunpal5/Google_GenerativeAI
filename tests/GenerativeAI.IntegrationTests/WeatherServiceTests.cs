using GenerativeAI.Tests;
using GenerativeAI.Tools;
using Xunit.Abstractions;

namespace GenerativeAI.IntegrationTests
{
    public class WeatherServiceTests:TestBase
    {
        public WeatherServiceTests(ITestOutputHelper helper):base(helper)   
        {
           
        }
        [Fact]
        public async Task ShouldInvokeWetherService()
        {
            WeatherService service = new WeatherService();
            var tools = service.AsTools();
            var calls = service.AsCalls();
            var tool = new GenericFunctionTool(tools, calls);
            
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            
            model.AddFunctionTool(tool);
           

            var result = await model.GenerateContentAsync("What is the weather in san francisco today?");
            
            Console.WriteLine(result.Text());
        }

        [Fact]
        public async Task ShouldInvokeWetherService2()
        {
            // WeatherService service = new WeatherService();
            // var functions = service.AsGoogleFunctions();
            // var calls = service.AsGoogleCalls();
            //
            // var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
            //
            // var model = new GenerativeModel(apiKey);
            // model.AddGlobalFunctions(functions,calls);
            // var result = await model.GenerateContentAsync("What is the weather in san francisco today?");
            //
            // Console.WriteLine(result);
        }
    }
}