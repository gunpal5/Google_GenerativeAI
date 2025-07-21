using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Tools;

namespace AotTest;

public class WeatherServiceTests
{
      
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

       
    public async Task ShouldWorkWith_BookStoreService()
    {
        var service = new BookStoreService();
        var tool = new GenericFunctionTool(service.AsTools(), service.AsCalls());
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.DefaultGeminiModel);
        model.AddFunctionTool(tool);
        var result = await model.GenerateContentAsync("what is written on page 35 in the book 'abracadabra'");
        Console.WriteLine(result.Text());
    }
    
    public async Task ShouldWorkWith_ComplexDataTypes()
    {
        var service = new ComplexDataTypeService();
        var tool = new GenericFunctionTool(service.AsTools(), service.AsCalls());
        var model = new GenerativeModel(GetTestGooglePlatform(), GoogleAIModels.Gemini2Flash);
        model.AddFunctionTool(tool);
        var result = await model.GenerateContentAsync("how's Deepak Siwach is doing in Senior Grade for enrollment year 01-01-2024 to 01-01-2025");
        Console.WriteLine(result.Text());
    }

   
    protected virtual IPlatformAdapter GetTestGooglePlatform()
    {
        //return GetTestVertexAIPlatform();
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY", EnvironmentVariableTarget.User);

        return new GoogleAIPlatformAdapter(apiKey);
    }
}