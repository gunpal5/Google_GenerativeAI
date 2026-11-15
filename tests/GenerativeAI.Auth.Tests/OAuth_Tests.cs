using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using GenerativeAI.Tests;
using Shouldly;

namespace GenerativeAI.Auth;

public class OAuth_Tests:TestBase
{
    public OAuth_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }
    
    [Fact]
    public async Task ShouldWorkWithOAuth_Json_GenerateContent()
    {
        Assert.SkipWhen(SkipVertexAITests,VertextTestSkipMesaage);
        var authenticator = CreateAuthenticatorWithJsonFile();
        
        var vertexAi = new VertexAIModel(authenticator:authenticator);
        var response = await vertexAi.GenerateContentAsync("write a poem about the sun", cancellationToken: TestContext.Current.CancellationToken);
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine(text);
    }

    private IGoogleAuthenticator? CreateAuthenticatorWithJsonFile()
    {
        var file = Environment.GetEnvironmentVariable("Google_Client_Secret", EnvironmentVariableTarget.User);
        Assert.SkipWhen(string.IsNullOrWhiteSpace(file) || !File.Exists(file),
            "Set the Google_Client_Secret user environment variable to an existing OAuth client secret JSON file.");
        return new GoogleOAuthAuthenticator(file);
    }
}