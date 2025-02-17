using System.Security.Authentication;
using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using Shouldly;
namespace GenerativeAI.Tests.Platforms;

public class GoogleAI_Tests:TestBase
{
    public GoogleAI_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }

    [Fact]
    
    public async Task ShouldThrowException_WhenProjectIdsAreInvalid()
    {
        Assert.SkipWhen(IsGoogleApiKeySet,"GOOGLE_API_KEY is set in environment variables. this test is not valid."); 
        Should.Throw<Exception>(() =>
        {
            var googleAi = new GoogleAi();
        });
        
        // var model = googleAi.CreateGenerativeModel(GoogleAIModels.Gemini15Flash);
        // var response = await model.GenerateContentAsync("write a poem about the sun");
        //
        // response.ShouldNotBeNull();
        // var text = response.Text();
        // text.ShouldNotBeNullOrWhiteSpace();
        // Console.WriteLine(text);
    }
    
    // [Fact]
    // public async Task ShouldNotThrowException_WhenCredentialsAreInvalid_AuthencatorProvided()
    // {
    //     var model = new GenerativeModel(new GoogleAIPlatformAdapter( accessToken:"invalid_token",authenticator:new GoogleCloudAdcAuthenticator()), VertexAIModels.Gemini.Gemini15Flash);
    //
    //     var response = await model.GenerateContentAsync("write a poem about the sun");
    //
    //     response.ShouldNotBeNull();
    //     var text = response.Text();
    //     text.ShouldNotBeNullOrWhiteSpace();
    //     Console.WriteLine(text);
    // }
    //
    // [Fact]
    // public async Task ShouldThrowException_WhenCredentialsAreInvalid_NoAuthencatorProvided()
    // {
    //     var model = new GenerativeModel(new GoogleAIPlatformAdapter(accessToken:"invalid"), VertexAIModels.Gemini.Gemini15Flash);
    //
    //     await Should.ThrowAsync<AuthenticationException>(async () =>
    //     {
    //         var response = await model.GenerateContentAsync("write a poem about the sun");
    //     });
    //
    // }
    //
    // [Fact]
    // public async Task InitializeFromEnvironmentVariables()
    // {
    //     var platform = new GoogleAIPlatformAdapter();
    //     platform.Region.ShouldBe("test");
    //     platform.ProjectId.ShouldBe("test");
    //     //platform.Credentials.AccessToken.ShouldContain("test");
    //     platform.Credentials.ApiKey.ShouldContain("test");
    //     platform.CredentialFile.ShouldContain("gcloud");
    //     
    //     // var model = new GenerativeModel(platform, VertexAIModels.Gemini.Gemini15Flash);
    //     //
    //     // var response = await model.GenerateContentAsync("write a poem about the sun");
    //     //
    //     // response.ShouldNotBeNull();
    //     // var text = response.Text();
    //     // text.ShouldNotBeNullOrWhiteSpace();
    //     // Console.WriteLine(text);
    // }

   
}