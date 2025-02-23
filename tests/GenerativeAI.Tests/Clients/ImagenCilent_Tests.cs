using GenerativeAI.Authenticators;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

public class ImageModel_Tests:TestBase
{
    public ImageModel_Tests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        
    }

    [Fact]

    public async Task ShouldGenerateImage_VertexAI()
    {
        var request = new GenerateImageRequest();
        request.Instances = new List<ImageGenerationInstance>();
        request.Instances.Add(new ImageGenerationInstance()
        {
            Prompt = "A photo of a cat and a dog fighting over a ball.",
        });
        var model = "imagen-3.0-generate-002";
        var client = new ImagenModel(GetTestVertexAIPlatform(),model);

       
        
        var images = await client.GenerateImagesAsync(request);
        images.ShouldNotBeNull();
        images.Predictions.ShouldNotBeNull();
        images.Predictions.Count.ShouldBeGreaterThan(0);
    }
    
    // [Fact]
    //
    // public async Task ShouldGenerateImage_GoogleAI()
    // {
    //     var platform = GetTestGooglePlatform();
    //     var request = new GenerateImageRequest();
    //     request.Instances = new List<ImageGenerationInstance>();
    //     request.Instances.Add(new ImageGenerationInstance()
    //     {
    //         Prompt = "A photo of a cat and a dog fighting over a ball.",
    //     });
    //     var model = "imagen-3.0-generate-002";
    //     var client = new ImagenModel(platform,model);
    //     
    //     var images = await client.GenerateImagesAsync(request);
    //     images.ShouldNotBeNull();
    //     images.Predictions.ShouldNotBeNull();
    //     images.Predictions.Count.ShouldBeGreaterThan(0);
    // }
    
    [Fact]

    public async Task ShouldGenerateCaptions_VertexAI()
    {
        var request = new ImageCaptioningRequest();
        request.Instances = new List<ImageInstance>();
        request.Instances.Add(new ImageInstance()
        {
            Image = new ImageData()
            {
                BytesBase64Encoded = Convert.ToBase64String(File.ReadAllBytes("image2.jpg")),
                MimeType = "image/jpeg"
            }
        });

        var client = new ImageTextModel(GetTestVertexAIPlatform());

        var model = "imagen-3.0-generate-002";
        
        var images = await client.GenerateImageCaptionAsync(request);
        images.ShouldNotBeNull();
        images.Predictions.ShouldNotBeNull();
        images.Predictions.Count.ShouldBeGreaterThan(0);
    }
    
    [Fact]

    public async Task ShouldGenerateVQA_VertexAI()
    {
        var request = new VqaRequest();
        request.Instances = new List<VqaInstance>();
        request.Instances.Add(new VqaInstance()
        {
            Image = new VqaImage()
            {
                BytesBase64Encoded = Convert.ToBase64String(File.ReadAllBytes("image.png")),
                MimeType = "image/png"
            },
            Prompt = "what do you think about this image?"
        });
        var model = "imagen-3.0-generate-002";
        var client = new ImageTextModel(GetTestVertexAIPlatform());

        
        
        var images = await client.VisualQuestionAnsweringAsync(request);
        images.ShouldNotBeNull();
        images.Predictions.ShouldNotBeNull();
        images.Predictions.Count.ShouldBeGreaterThan(0);
    }
    
    protected override IPlatformAdapter GetTestGooglePlatform()
    {
       Assert.SkipWhen(IsGeminiApiKeySet, GeminiTestSkipMessage);
       return base.GetTestGooglePlatform();
    }
    protected override IPlatformAdapter GetTestVertexAIPlatform()
    {
        Assert.SkipWhen(SkipVertexAITests, VertextTestSkipMesaage);
        var testServiceAccount = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT", EnvironmentVariableTarget.User);
        var file = Environment.GetEnvironmentVariable("Google_Service_Account_Json", EnvironmentVariableTarget.User);
        Assert.SkipWhen(string.IsNullOrEmpty(file), "Please set the Google_Service_Account_Json environment variable to the path of the service account json file.");

        var platform = base.GetTestVertexAIPlatform();
        platform.SetAuthenticator(new GoogleServiceAccountAuthenticator(file));
        return platform;
    }
}