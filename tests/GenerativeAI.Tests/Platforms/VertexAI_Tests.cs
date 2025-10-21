using System.Security.Authentication;
using GenerativeAI.Core;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Platforms;

public class VertexAI_Tests:TestBase
{
    public VertexAI_Tests(ITestOutputHelper helper) : base(helper)
    {
        
    }

    [Fact(Skip = "Skip until we have a valid project id")]
    public async Task ShouldThrowException_WhenProjectIdsAreInvalid()
    {
        await Should.ThrowAsync<Exception>(async () =>
        {
            var model = new VertexAIModel();

            var response = await model.GenerateContentAsync("write a poem about the sun", cancellationToken: TestContext.Current.CancellationToken);

            response.ShouldNotBeNull();
            var text = response.Text();
            text.ShouldNotBeNullOrWhiteSpace();
            Console.WriteLine(text);
        });
    }


    [Fact]
    public async Task ShouldThrowException_WhenCredentialsAreInvalid_NoAuthencatorProvided()
    {
        var model = new GenerativeModel(new VertextPlatformAdapter(accessToken:"invalid", projectId:"lashkfdkl",region:"dlakfhkl"), VertexAIModels.Gemini.Gemini25Flash);

        await Should.ThrowAsync<AuthenticationException>(async () =>
        {
            var response = await model.GenerateContentAsync("write a poem about the sun", cancellationToken: TestContext.Current.CancellationToken);
        });

    }
    
    [RunnableInDebugOnly]
    public Task InitializeFromEnvironmentVariables()
    {
        var platform = new VertextPlatformAdapter();
        platform.Region.ShouldBe("test");
        platform.ProjectId.ShouldBe("test");
        //platform.Credentials.AccessToken.ShouldContain("test");
        platform.Credentials.ApiKey.ShouldContain("test");
        platform.CredentialFile.ShouldContain("gcloud");
        
        return Task.CompletedTask;
        // var model = new GenerativeModel(platform, VertexAIModels.Gemini.Gemini15Flash);
        //
        // var response = await model.GenerateContentAsync("write a poem about the sun");
        //
        // response.ShouldNotBeNull();
        // var text = response.Text();
        // text.ShouldNotBeNullOrWhiteSpace();
        // Console.WriteLine(text);
    }

   
}