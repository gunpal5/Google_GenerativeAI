using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using GenerativeAI.Tests;
using Humanizer;
using Shouldly;

namespace GenerativeAI.Auth;

public class ServiceAccount_Tests:TestBase
{
    public ServiceAccount_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }

    [RunnableInDebugOnly]
    
    public async Task ShouldWorkWithServiceAccount()
    { 
        Assert.SkipWhen(SkipVertexAITests,VertextTestSkipMesaage);
        var authenticator = CreateAuthenticatorWithKey();
        var token = await authenticator.GetAccessTokenAsync().ConfigureAwait(false);

        token.AccessToken.ShouldNotBeNull();
    }

    private GoogleServiceAccountAuthenticator CreateAuthenticatorWithKey()
    {
        var email = Environment.GetEnvironmentVariable("Google_Service_Account_Email", EnvironmentVariableTarget.User);
        var key = Environment.GetEnvironmentVariable("Google_Service_Account_Key", EnvironmentVariableTarget.User);
        var password = Environment.GetEnvironmentVariable("Google_key_password", EnvironmentVariableTarget.User);

        return new GoogleServiceAccountAuthenticator(email, key, password);

    }

    [Fact]
    public async Task ShouldWorkWithServiceAccount_GenerateContent()
    {
        Assert.SkipWhen(SkipVertexAITests,VertextTestSkipMesaage);
        var authenticator = CreateAuthenticatorWithKey();
        
        var vertexAi = new VertexAIModel(authenticator:authenticator);
        var response = await vertexAi.GenerateContentAsync("write a poem about the sun").ConfigureAwait(false);
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine(text);
    }
    
    [Fact]
    public async Task ShouldWorkWithServiceAccount_Json_GenerateContent()
    {
        Assert.SkipWhen(SkipVertexAITests,VertextTestSkipMesaage);
        var authenticator = CreateAuthenticatorWithJsonFile();
        
        var vertexAi = new VertexAIModel(authenticator:authenticator);
        var response = await vertexAi.GenerateContentAsync("write a poem about the sun").ConfigureAwait(false);
        response.ShouldNotBeNull();
        var text = response.Text();
        text.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine(text);
    }

    private IGoogleAuthenticator? CreateAuthenticatorWithJsonFile()
    {
        var file = Environment.GetEnvironmentVariable("Google_Service_Account_Json", EnvironmentVariableTarget.User);
        Assert.SkipWhen(string.IsNullOrEmpty(file), "Please set the Google_Service_Account_Json environment variable to the path of the service account json file.");
        return new GoogleServiceAccountAuthenticator(file);
    }
}