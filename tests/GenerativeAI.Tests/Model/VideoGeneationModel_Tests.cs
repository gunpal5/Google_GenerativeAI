using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI.Tests.Model;

public class VideoGeneationModel_Tests:TestBase
{
    public VideoGeneationModel_Tests(ITestOutputHelper helper) : base(helper)
    {
        Assert.SkipWhen(SkipVertexAITests, VertextTestSkipMesaage);
    }

    [Fact]
    public async Task ShouldGenerateVideos()
    {
        var model = new VideoGenerationModel(GetTestVertexAIPlatform(),VertexAIModels.Video.Veo2Generate001);

        var request = new GenerateVideosRequest()
        {
            Model = "veo2-generate-001",
            Prompt = "A dog catching a ball",
            Config = new GenerateVideosConfig()
            {
                AspectRatio = VideoAspectRatio.LANDSCAPE_16_9,
                DurationSeconds = 2,
                EnhancePrompt = true,
                Fps = 24,
                NumberOfVideos = 1,
                PersonGeneration = VideoPersonGeneration.AllowAdult,
                Resolution = VideoResolution.HD_720P
            }
        };
        var response = await model.GenerateVideosAsync( request);
    }
    
    protected override IPlatformAdapter GetTestVertexAIPlatform()
    {
        var testServiceAccount = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT", EnvironmentVariableTarget.User);
        var file = Environment.GetEnvironmentVariable("Google_Service_Account_Json", EnvironmentVariableTarget.User);
        Assert.SkipWhen(string.IsNullOrEmpty(file), "Please set the Google_Service_Account_Json environment variable to the path of the service account json file.");

        var platform = base.GetTestVertexAIPlatform();
        platform.SetAuthenticator(new GoogleServiceAccountAuthenticator(file));
        
        //return new GoogleServiceAccountAuthenticator(file);
        return platform;
    }
}