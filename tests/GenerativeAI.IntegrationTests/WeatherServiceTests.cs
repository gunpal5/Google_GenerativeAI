using GenerativeAI.Tests;
using GenerativeAI.Tools;

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
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            WeatherService service = new WeatherService();
            var tools = service.AsTools();
            var calls = service.AsCalls();
            var tool = new GenericFunctionTool(tools, calls);
            
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            
            model.AddFunctionTool(tool);

            var result = await model.GenerateContentAsync("What is the weather in san francisco today?", cancellationToken: TestContext.Current.CancellationToken);
            
            Console.WriteLine(result.Text());
        }
        
        [Fact]
        public async Task ShouldInvokeWeatherService_WithStreaming()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            WeatherService service = new WeatherService();
            var tools = service.AsTools();
            var calls = service.AsCalls();
            var tool = new GenericFunctionTool(tools, calls);
            
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            
            model.AddFunctionTool(tool);

            await foreach (var result in model.StreamContentAsync("What is the weather in san francisco today?", cancellationToken: TestContext.Current.CancellationToken)
                               )
            {
                Console.WriteLine(result.Text());
            }
            //var result = await model.StreamContentAsync("What is the weather in san francisco today?");
            
           // Console.WriteLine(result.Text());
        }

        [Fact]
        public Task ShouldInvokeWetherService2()
        {
            // WeatherService service = new WeatherService();
            // var functions = service.AsGoogleFunctions();
            // var calls = service.AsGoogleCalls();
            //
            // var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY_TEST", EnvironmentVariableTarget.User);
            //
            // var model = new GenerativeModel(apiKey);
            // model.AddGlobalFunctions(functions,calls);
            // var result = await model.GenerateContentAsync("What is the weather in san francisco today?");
            //
            // Console.WriteLine(result);
            return Task.CompletedTask;
        }
        
        [Fact]
        public async Task ShouldWorkWith_BookStoreService()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            var service = new BookStoreService();
            var tool = new GenericFunctionTool(service.AsTools(), service.AsCalls());
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(tool);
            var result = await model.GenerateContentAsync("what is written on page 35 in the book 'abracadabra'", cancellationToken: TestContext.Current.CancellationToken);
            Console.WriteLine(result.Text());
        }
        
        [Fact]
        public async Task ShouldWorkWith_BookStoreService_with_streaming()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            var service = new BookStoreService();
            var tool = new GenericFunctionTool(service.AsTools(), service.AsCalls());
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(tool);
            await foreach (var result in model
                               .StreamContentAsync("what is written on page 35 in the book 'abracadabra'", cancellationToken: TestContext.Current.CancellationToken)
                               )
            {
                Console.WriteLine(result.Text());
            }
            
        }
        
        [Fact]
        public async Task ShouldWorkWithoutParameters_Interface()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            var service = new BookStoreService();
            var tool = new GenericFunctionTool(service.AsTools(), service.AsCalls());
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(tool);
            await foreach (var result in model
                               .StreamContentAsync("Give me the list of books", cancellationToken: TestContext.Current.CancellationToken)
                               )
            {
                Console.WriteLine(result.Text());
            }
        }
        
        
        [Fact]
        public async Task ShouldWorkWithoutParametersAsync_QuickTool()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            var service = new BookStoreService();
            var tool = new QuickTool(service.GetBookListAsync);
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(tool);
            await foreach (var result in model
                               .StreamContentAsync("Give me the list of books", cancellationToken: TestContext.Current.CancellationToken)
                               )
            {
                Console.WriteLine(result.Text());
            }
        }
        
        [Fact]
        public async Task ShouldWorkWithoutParameters_QuickTool()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            var service = new BookStoreService();
            var tool = new QuickTool(service.GetBookList);
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(tool);
            await foreach (var result in model
                               .StreamContentAsync("Give me the list of books", cancellationToken: TestContext.Current.CancellationToken)
                               )
            {
                Console.WriteLine(result.Text());
            }
        }
        
        [Fact]
        public async Task ShouldWorkWithoutParameters_Method()
        {
            Assert.SkipUnless(IsGoogleApiKeySet,GoogleTestSkipMessage);
            var service = new MethodTools();
            var tool = new Tools([service.GetBookList2]);
            var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
            model.AddFunctionTool(tool);
            await foreach (var result in model
                               .StreamContentAsync("Give me the list of books", cancellationToken: TestContext.Current.CancellationToken)
                               )
            {
                Console.WriteLine(result.Text());
            }
        }
    }
}