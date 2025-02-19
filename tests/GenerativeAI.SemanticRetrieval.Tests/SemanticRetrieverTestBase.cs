using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using GenerativeAI.Tests;
using Xunit;

namespace GenerativeAI.SemanticRetrieval.Tests;

public abstract class SemanticRetrieverTestBase:TestBase
{
    public SemanticRetrieverTestBase() : base()
    {
        
    }

    public SemanticRetrieverTestBase(ITestOutputHelper helper) : base(helper)
    {
        
    }
    
    protected override IPlatformAdapter GetTestGooglePlatform()
    {
        var testServiceAccount = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT", EnvironmentVariableTarget.User);
        var file = Environment.GetEnvironmentVariable("Google_Service_Account_Json", EnvironmentVariableTarget.User);
        Assert.SkipWhen(string.IsNullOrEmpty(file), "Please set the Google_Service_Account_Json environment variable to the path of the service account json file.");

        var platform = base.GetTestGooglePlatform();
        platform.SetAuthenticator(new GoogleServiceAccountAuthenticator(file));
        
        //return new GoogleServiceAccountAuthenticator(file);
        return platform;
    }
}