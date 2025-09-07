using System.Text;
using GenerativeAI.Tests;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Platforms;

public class VertexAI_ImagePreview_IntegrationTests : TestBase
{
    public VertexAI_ImagePreview_IntegrationTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task ShouldUseGemini25FlashImagePreview_WithTextOnly()
    {
        // Skip test if VertexAI is not configured
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        
        // Arrange
        var vertexAI = new VertexAI(
            projectId: Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"),
            region: Environment.GetEnvironmentVariable("GOOGLE_REGION") ?? "us-central1"
        );
        
        var model = vertexAI.CreateGenerativeModel(VertexAIModels.Gemini.Gemini2Flash);

        // Act
        var result = await model.GenerateContentAsync(
            "Hello! Can you tell me what capabilities you have as the gemini-2.5-flash-image-preview model?",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Text().ShouldNotBeNullOrEmpty();
        Console?.WriteLine($"Model response: {result.Text()}");
        
        // Should mention image capabilities since this is the image preview model
        result.Text().ShouldContain("image", Case.Insensitive);
    }

    [Fact]
    public async Task ShouldUseGemini25FlashImagePreview_WithImageAnalysis()
    {
        // Skip test if VertexAI is not configured
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        
        // Arrange
        var vertexAI = new VertexAI(
            projectId: Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"),
            region: Environment.GetEnvironmentVariable("GOOGLE_REGION") ?? "us-central1"
        );
        
        var model = vertexAI.CreateGenerativeModel(VertexAIModels.Gemini.Gemini25FlashImagePreview);

        var request = new GenerateContentRequest();
        request.AddInlineFile("image.png", false);
        request.AddText("What do you see in this image?");

        // Act
        var result = await model.GenerateContentAsync(
            request,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Text().ShouldNotBeNullOrEmpty();
        Console?.WriteLine($"Image analysis response: {result.Text()}");
        
        // The model should respond about the image content
        var responseText = result.Text();
        responseText.ShouldNotContain("cannot", Case.Insensitive);
        responseText.ShouldNotContain("unable", Case.Insensitive);
    }

    [Fact]
    public async Task ShouldUseGemini25FlashImagePreview_WithStreamingResponse()
    {
        // Skip test if VertexAI is not configured  
        Assert.SkipUnless(!SkipVertexAITests, VertextTestSkipMesaage);
        
        // Arrange
        var vertexAI = new VertexAI(
            projectId: Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"),
            region: Environment.GetEnvironmentVariable("GOOGLE_REGION") ?? "us-central1"
        );
        
        var model = vertexAI.CreateGenerativeModel(VertexAIModels.Gemini.Gemini25FlashImagePreview);

        var responseText = new StringBuilder();

        // Act
        await foreach (var response in model.StreamContentAsync(
            "Write a short story about AI helping solve climate change. Keep it under 100 words.",
            cancellationToken: TestContext.Current.CancellationToken))
        {
            var text = response.Text();
            if (!string.IsNullOrEmpty(text))
            {
                responseText.Append(text);
                Console?.Write(text);
            }
        }

        // Assert
        var finalResponse = responseText.ToString();
        finalResponse.ShouldNotBeNullOrEmpty();
        Console?.WriteLine($"\nComplete streaming response received: {finalResponse.Length} characters");
        
        // Should contain story elements
        finalResponse.ShouldContain("AI", Case.Insensitive);
        finalResponse.ShouldContain("climate", Case.Insensitive);
    }

   

    [Fact]
    public void ShouldHaveCorrectModelConstant()
    {
        // Act & Assert
        VertexAIModels.Gemini.Gemini25FlashImagePreview.ShouldBe("gemini-2.5-flash-image-preview");
    }
}