using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Model;

/// <summary>
/// Veo video generation was retired on the Gemini API; the model must fail fast there with guidance.
/// </summary>
public class VideoGenerationModel_GeminiApi_Tests
{
    [Fact]
    public async Task GenerateVideosAsync_OnGeminiApi_ThrowsWithGuidance()
    {
        var model = new VideoGenerationModel(new GoogleAIPlatformAdapter("test-key"), GoogleAIModels.Video.GeminiOmni12Flash);

        var ex = await Should.ThrowAsync<NotSupportedException>(() =>
            model.GenerateVideosAsync(new GenerateVideosRequest(), TestContext.Current.CancellationToken));

        ex.Message.ShouldContain("Interactions API");
    }
}
