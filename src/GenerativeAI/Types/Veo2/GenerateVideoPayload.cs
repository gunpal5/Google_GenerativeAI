using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Internal representation of the payload for Vertex AI video generation requests.
/// </summary>
public class VertexGenerateVideosPayload
{
    [JsonPropertyName("instances")] public List<VideoInstance>? Instances { get; set; }

    [JsonPropertyName("parameters")] public VideoParameters? Parameters { get; set; }
}

/// <summary>
/// Internal representation of a single instance within the Vertex AI payload.
/// </summary>
public class VideoInstance
{
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }

    [JsonPropertyName("image")] public ImageSample? Image { get; set; }
}

/// <summary>
/// Internal representation of the parameters within the Vertex AI payload.
/// Mirrors GenerateVideosConfig but ensures correct JsonPropertyNames for serialization.
/// </summary>
public class VideoParameters
{
    [JsonPropertyName("sampleCount")] public int? SampleCount { get; set; }

    [JsonPropertyName("storageUri")] public string? StorageUri { get; set; }

    [JsonPropertyName("fps")] public int? Fps { get; set; }

    [JsonPropertyName("durationSeconds")] public int? DurationSeconds { get; set; }

    [JsonPropertyName("seed")] public int? Seed { get; set; }

    [JsonPropertyName("aspectRatio")]
    [JsonConverter(typeof(VideoAspectRatioConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoAspectRatio? AspectRatio { get; set; }

    [JsonPropertyName("resolution")]
    [JsonConverter(typeof(VideoResolutionConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoResolution? Resolution { get; set; }

    [JsonPropertyName("personGeneration")]
    [JsonConverter(typeof(VideoPersonGenerationConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoPersonGeneration? PersonGeneration { get; set; }

    [JsonPropertyName("negativePrompt")] public string? NegativePrompt { get; set; }

    [JsonPropertyName("enhancePrompt")] public bool? EnhancePrompt { get; set; }

    [JsonPropertyName("pubsubTopic")] public string? PubSubTopic { get; set; }
}