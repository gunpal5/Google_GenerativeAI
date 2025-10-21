using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;
using System.Text.Json;

namespace GenerativeAI.Tests.Model;

public class VideoGeneationModel_Tests:TestBase
{
    public VideoGeneationModel_Tests(ITestOutputHelper helper) : base(helper)
    {
        Assert.SkipWhen(SkipVertexAITests, VertextTestSkipMesaage);
    }

    [RunnableInDebugOnly]
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
                DurationSeconds = 5,
                EnhancePrompt = true,
                Fps = 24,
                NumberOfVideos = 1,
                PersonGeneration = VideoPersonGeneration.AllowAdult,
                Resolution = VideoResolution.HD_720P
            }
        };
        var operation = await model.GenerateVideosAsync( request);
        
        var response = await model.AwaitForLongRunningOperation(operation.Name,(int) TimeSpan.FromMinutes(10).TotalMilliseconds);

        if (response.Done == true)
        {
            await File.WriteAllBytesAsync("generated.mp4", response.Result.GeneratedVideos[0].VideoBytes);
        }
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

    [Fact]
    public void GenerateVideosConfig_Should_Support_New_Properties()
    {
        // Arrange & Act
        var config = new GenerateVideosConfig
        {
            // Existing properties
            AspectRatio = VideoAspectRatio.LANDSCAPE_16_9,
            DurationSeconds = 5,
            Fps = 24,
            NumberOfVideos = 2,

            // NEW: Audio generation
            GenerateAudio = true,

            // NEW: Last frame reference
            LastFrame = new ImageSample
            {
                GcsUri = "gs://bucket/last-frame.jpg",
                MimeType = "image/jpeg"
            },

            // NEW: Reference images
            ReferenceImages = new List<VideoGenerationReferenceImage>
            {
                new VideoGenerationReferenceImage
                {
                    Image = new ImageSample
                    {
                        GcsUri = "gs://bucket/style-reference.jpg",
                        MimeType = "image/jpeg"
                    },
                    ReferenceType = VideoGenerationReferenceType.STYLE
                },
                new VideoGenerationReferenceImage
                {
                    Image = new ImageSample
                    {
                        ImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }, // PNG header
                        MimeType = "image/png"
                    },
                    ReferenceType = VideoGenerationReferenceType.ASSET
                }
            },

            // NEW: Mask for inpainting/outpainting
            Mask = new VideoGenerationMask
            {
                Image = new ImageSample
                {
                    GcsUri = "gs://bucket/mask.png",
                    MimeType = "image/png"
                },
                MaskMode = VideoGenerationMaskMode.INSERT
            },

            // NEW: Compression quality
            CompressionQuality = VideoCompressionQuality.LOSSLESS
        };

        // Assert
        config.GenerateAudio.ShouldBeTrue();
        config.LastFrame.ShouldNotBeNull();
        config.LastFrame.GcsUri.ShouldBe("gs://bucket/last-frame.jpg");

        config.ReferenceImages.ShouldNotBeNull();
        config.ReferenceImages.Count.ShouldBe(2);
        config.ReferenceImages[0].ReferenceType.ShouldBe(VideoGenerationReferenceType.STYLE);
        config.ReferenceImages[1].ReferenceType.ShouldBe(VideoGenerationReferenceType.ASSET);

        config.Mask.ShouldNotBeNull();
        config.Mask.MaskMode.ShouldBe(VideoGenerationMaskMode.INSERT);

        config.CompressionQuality.ShouldBe(VideoCompressionQuality.LOSSLESS);
    }

    [Fact]
    public void GenerateVideosResponse_Should_Have_Correct_Structure()
    {
        // Arrange
        var response = new GenerateVideosResponse
        {
            GeneratedVideos = new List<GeneratedVideo>
            {
                new GeneratedVideo
                {
                    Video = new Video
                    {
                        Uri = "gs://bucket/video1.mp4"
                    }
                },
                new GeneratedVideo
                {
                    Video = new Video
                    {
                        Uri = "gs://bucket/video2.mp4"
                    }
                }
            },
            RaiMediaFilteredCount = 1,
            RaiMediaFilteredReasons = new List<string> { "VIOLENCE" }
        };

        // Assert - Type should be List<GeneratedVideo> not List<Video>
        response.GeneratedVideos.ShouldNotBeNull();
        response.GeneratedVideos.ShouldBeOfType<List<GeneratedVideo>>();
        response.GeneratedVideos.Count.ShouldBe(2);

        // Assert - Each GeneratedVideo wraps a Video
        response.GeneratedVideos[0].Video.ShouldNotBeNull();
        response.GeneratedVideos[0].Video.Uri.ShouldBe("gs://bucket/video1.mp4");
        response.GeneratedVideos[1].Video.ShouldNotBeNull();
        response.GeneratedVideos[1].Video.Uri.ShouldBe("gs://bucket/video2.mp4");

        response.RaiMediaFilteredCount.ShouldBe(1);
        response.RaiMediaFilteredReasons.ShouldHaveSingleItem();
    }

    [Fact]
    public void GenerateVideosResponse_Serialization_Should_Use_Correct_JSON_Property_Names()
    {
        // Arrange
        var response = new GenerateVideosResponse
        {
            GeneratedVideos = new List<GeneratedVideo>
            {
                new GeneratedVideo
                {
                    Video = new Video
                    {
                        Uri = "gs://bucket/test.mp4"
                    }
                }
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Act
        var json = JsonSerializer.Serialize(response, options);

        // Assert - Should use "generatedVideos" not "videos"
        json.ShouldContain("\"generatedVideos\"");
        json.ShouldNotContain("\"videos\":");
    }

    [Fact]
    public void GenerateVideosResponse_Deserialization_Should_Work_With_Python_SDK_JSON()
    {
        // Arrange - JSON structure from Python SDK
        var pythonJson = @"{
            ""generatedVideos"": [
                {
                    ""video"": {
                        ""uri"": ""gs://my-bucket/output/video.mp4""
                    }
                }
            ],
            ""raiMediaFilteredCount"": 0,
            ""raiMediaFilteredReasons"": []
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var response = JsonSerializer.Deserialize<GenerateVideosResponse>(pythonJson, options);

        // Assert
        response.ShouldNotBeNull();
        response.GeneratedVideos.ShouldNotBeNull();
        response.GeneratedVideos.ShouldHaveSingleItem();
        response.GeneratedVideos[0].Video?.Uri.ShouldBe("gs://my-bucket/output/video.mp4");
        response.RaiMediaFilteredCount.ShouldBe(0);
        response.RaiMediaFilteredReasons.ShouldNotBeNull();
        response.RaiMediaFilteredReasons.ShouldBeEmpty();
    }

    [Fact]
    public void VideoGenerationMaskMode_Enum_Should_Have_All_Values()
    {
        // Assert - All mask modes are available
        VideoGenerationMaskMode.INSERT.ShouldBe(VideoGenerationMaskMode.INSERT);
        VideoGenerationMaskMode.REMOVE.ShouldBe(VideoGenerationMaskMode.REMOVE);
        VideoGenerationMaskMode.REMOVE_STATIC.ShouldBe(VideoGenerationMaskMode.REMOVE_STATIC);
        VideoGenerationMaskMode.OUTPAINT.ShouldBe(VideoGenerationMaskMode.OUTPAINT);
    }

    [Fact]
    public void VideoGenerationReferenceType_Enum_Should_Have_All_Values()
    {
        // Assert - All reference types are available
        VideoGenerationReferenceType.ASSET.ShouldBe(VideoGenerationReferenceType.ASSET);
        VideoGenerationReferenceType.STYLE.ShouldBe(VideoGenerationReferenceType.STYLE);
    }

    [Fact]
    public void VideoCompressionQuality_Enum_Should_Have_All_Values()
    {
        // Assert - All compression quality levels are available
        VideoCompressionQuality.OPTIMIZED.ShouldBe(VideoCompressionQuality.OPTIMIZED);
        VideoCompressionQuality.LOSSLESS.ShouldBe(VideoCompressionQuality.LOSSLESS);
    }

    [Fact]
    public void GenerateVideosConfig_Serialization_Should_Include_New_Properties()
    {
        // Arrange
        var config = new GenerateVideosConfig
        {
            GenerateAudio = true,
            LastFrame = new ImageSample { GcsUri = "gs://bucket/frame.jpg" },
            ReferenceImages = new List<VideoGenerationReferenceImage>
            {
                new VideoGenerationReferenceImage
                {
                    Image = new ImageSample { GcsUri = "gs://bucket/ref.jpg" },
                    ReferenceType = VideoGenerationReferenceType.STYLE
                }
            },
            Mask = new VideoGenerationMask
            {
                Image = new ImageSample { GcsUri = "gs://bucket/mask.png" },
                MaskMode = VideoGenerationMaskMode.INSERT
            },
            CompressionQuality = VideoCompressionQuality.LOSSLESS
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Act
        var json = JsonSerializer.Serialize(config, options);

        // Assert - All new properties should be in JSON
        json.ShouldContain("\"generateAudio\"");
        json.ShouldContain("\"lastFrame\"");
        json.ShouldContain("\"referenceImages\"");
        json.ShouldContain("\"mask\"");
        json.ShouldContain("\"compressionQuality\"");
        json.ShouldContain("\"LOSSLESS\"");
        json.ShouldContain("\"INSERT\"");
        json.ShouldContain("\"STYLE\"");
    }
}