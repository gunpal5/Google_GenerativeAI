using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration settings for a video generation request.
/// </summary>
public class GenerateVideosConfig
{
    /// <summary>
    /// Optional. The number of video variations to generate.
    /// </summary>
    [JsonPropertyName("numberOfVideos")]
    public int? NumberOfVideos { get; set; }

    /// <summary>
    /// Optional. The Google Cloud Storage URI where the generated videos should be saved.
    /// Example: "gs://your-bucket/output-folder/".
    /// </summary>
    [JsonPropertyName("outputGcsUri")]
    public string? OutputGcsUri { get; set; }

    /// <summary>
    /// Optional. The desired frames per second (FPS) for the generated video.
    /// </summary>
    [JsonPropertyName("fps")]
    public int? Fps { get; set; }

    /// <summary>
    /// Optional. The desired duration of the generated video clip in seconds.
    /// </summary>
    [JsonPropertyName("durationSeconds")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Optional. A seed value for the random number generator to ensure reproducibility.
    /// If the seed is the same for identical inputs, the results should be consistent.
    /// Otherwise, a random seed is used.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Optional. The desired aspect ratio for the generated video (e.g., "16:9", "9:16").
    /// </summary>
    [JsonPropertyName("aspectRatio")]
    [JsonConverter(typeof(VideoAspectRatioConverter))]
    public VideoAspectRatio? AspectRatio { get; set; }


    /// <summary>
    /// Optional. The desired resolution for the generated video (e.g., "1280x720", "1920x1080").
    /// </summary>
    [JsonPropertyName("resolution")]
    [JsonConverter(typeof(VideoResolutionConverter))]
    public VideoResolution? Resolution { get; set; }

    /// <summary>
    /// Optional. Controls whether videos featuring people can be generated and restricts generation based on age representation.
    /// Supported values might include "dontAllow", "allowAdult". Check API documentation for exact values.
    /// </summary>
    [JsonPropertyName("personGeneration")]
    [JsonConverter(typeof(VideoPersonGenerationConverter))]
    public VideoPersonGeneration? PersonGeneration { get; set; }

    /// <summary>
    /// Optional. A Google Cloud Pub/Sub topic where notifications about the video generation progress will be published.
    /// </summary>
    [JsonPropertyName("pubsubTopic")]
    public string? PubsubTopic { get; set; }

    /// <summary>
    /// Optional. A text prompt describing elements or styles to avoid in the generated video.
    /// </summary>
    [JsonPropertyName("negativePrompt")]
    public string? NegativePrompt { get; set; }

    /// <summary>
    /// Optional. Whether to enable internal prompt enhancement logic to potentially improve results.
    /// </summary>
    [JsonPropertyName("enhancePrompt")]
    public bool? EnhancePrompt { get; set; }
}