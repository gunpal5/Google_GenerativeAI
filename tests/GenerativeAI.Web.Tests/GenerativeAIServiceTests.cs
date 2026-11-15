using Microsoft.Extensions.Options;
using Shouldly;

namespace GenerativeAI.Web.Tests;

public class GenerativeAIServiceTests
{
    [Fact]
    public void CreateInstance_WithoutModelName_UsesConfiguredModel()
    {
        var service = new GenerativeAIService(Options.Create(new GenerativeAIOptions
        {
            Credentials = new GoogleAICredentials("test-key"),
            Model = GoogleAIModels.Gemini35FlashLite
        }));

        ((GenerativeModel)service.CreateInstance()).Model.ShouldBe(GoogleAIModels.Gemini35FlashLite);
        ((GenerativeModel)service.CreateInstance(GoogleAIModels.Gemini38Flash)).Model.ShouldBe(GoogleAIModels.Gemini38Flash);
    }

    [Fact]
    public void CreateInstance_WithoutConfiguredModel_UsesDefaultModel()
    {
        var service = new GenerativeAIService(Options.Create(new GenerativeAIOptions
        {
            Credentials = new GoogleAICredentials("test-key")
        }));

        ((GenerativeModel)service.CreateInstance()).Model.ShouldBe(GoogleAIModels.DefaultGeminiModel);
    }

    [Fact]
    public void VertexOptions_ApiVersion_IsAppliedToTheAdapter()
    {
        var service = new GenerativeAIService(Options.Create(new GenerativeAIOptions
        {
            IsVertex = true,
            ExpressMode = true,
            Credentials = new GoogleAICredentials("test-key"),
            ApiVersion = "v1"
        }));

        service.Platform.GetPlatformAdapter().GetApiVersion().ShouldBe("v1");
    }

    [Fact]
    public void GoogleAiOptions_ApiVersion_IsAppliedToTheAdapter()
    {
        var service = new GenerativeAIService(Options.Create(new GenerativeAIOptions
        {
            Credentials = new GoogleAICredentials("test-key"),
            ApiVersion = "v1"
        }));

        service.Platform.GetPlatformAdapter().GetApiVersion().ShouldBe("v1");
    }
}
