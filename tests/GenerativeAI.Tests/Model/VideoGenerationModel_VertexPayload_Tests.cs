using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Model;

/// <summary>
/// Offline tests for the Vertex AI predictLongRunning payload built by <see cref="VideoGenerationModel"/>.
/// The HTTP call is captured locally, so no video is generated.
/// </summary>
public class VideoGenerationModel_VertexPayload_Tests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{ "name": "operations/123", "done": false }""", Encoding.UTF8,
                    "application/json")
            };
        }
    }

    [Fact]
    public async Task GenerateVideosAsync_OnVertex_SendsAllConfigFields()
    {
        var handler = new CapturingHandler();
        var platform = new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token",
            validateAccessToken: false);
        var model = new VideoGenerationModel(platform, VertexAIModels.Video.Veo2Generate001, new HttpClient(handler));

        await model.GenerateVideosAsync(new GenerateVideosRequest
        {
            Prompt = "A dog catching a ball",
            Config = new GenerateVideosConfig
            {
                DurationSeconds = 5,
                GenerateAudio = true,
                CompressionQuality = VideoCompressionQuality.LOSSLESS,
                LastFrame = new ImageSample { GcsUri = "gs://bucket/last.png", MimeType = "image/png" },
                ReferenceImages = new List<VideoGenerationReferenceImage>
                {
                    new()
                    {
                        Image = new ImageSample { GcsUri = "gs://bucket/style.png", MimeType = "image/png" },
                        ReferenceType = VideoGenerationReferenceType.STYLE
                    }
                },
                Mask = new VideoGenerationMask
                {
                    Image = new ImageSample { GcsUri = "gs://bucket/mask.png", MimeType = "image/png" },
                    MaskMode = VideoGenerationMaskMode.INSERT
                }
            }
        }, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Body!)!.AsObject();
        var instance = body["instances"]![0]!.AsObject();
        instance["prompt"]!.GetValue<string>().ShouldBe("A dog catching a ball");
        instance["lastFrame"]!["gcsUri"]!.GetValue<string>().ShouldBe("gs://bucket/last.png");
        instance["referenceImages"]![0]!["referenceType"]!.GetValue<string>().ShouldBe("STYLE");
        instance["mask"]!["maskMode"]!.GetValue<string>().ShouldBe("INSERT");

        var parameters = body["parameters"]!.AsObject();
        parameters["durationSeconds"]!.GetValue<int>().ShouldBe(5);
        parameters["generateAudio"]!.GetValue<bool>().ShouldBeTrue();
        parameters["compressionQuality"]!.GetValue<string>().ShouldBe("LOSSLESS");
    }
}
