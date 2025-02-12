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
    protected IPlatformAdapter GetTestGooglePlatform()
    {
        var apiKey = Environment.GetEnvironmentVariable("Gemini_API_Key", EnvironmentVariableTarget.User);
        return new GoogleAIPlatformAdapter(apiKey);
    }
}