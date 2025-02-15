
using GenerativeAI.Core;
using Xunit.Abstractions;

namespace GenerativeAI.Tests;

public abstract class TestBase
{
    protected ITestOutputHelper Console { get; }
    protected TestBase()
    {
        
    }

    protected TestBase(ITestOutputHelper testOutputHelper)
    {
        this.Console = testOutputHelper;
    }
    protected static IPlatformAdapter GetTestGooglePlatform()
    {
        //return GetTestVertexAIPlatform();
        var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
        return new GoogleAIPlatformAdapter(apiKey);
    }
    
    protected static IPlatformAdapter GetTestVertexAIPlatform()
    {
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.User);
        var projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID", EnvironmentVariableTarget.User);
        var region = Environment.GetEnvironmentVariable("GOOGLE_REGION", EnvironmentVariableTarget.User);

        return new VertextPlatformAdapter(projectId, region,false, apiKey);
    }
}