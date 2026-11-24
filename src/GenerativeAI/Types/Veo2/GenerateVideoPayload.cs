using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Internal representation of the payload for Vertex AI video generation requests.
/// </summary>
public class VertexGenerateVideosPayload
{
    /// <summary>
    /// Gets or sets the list of video instances to generate.
    /// </summary>
    [JsonPropertyName("instances")] public List<VideoInstance>? Instances { get; set; }

    /// <summary>
    /// Gets or sets the parameters for video generation.
    /// </summary>
    [JsonPropertyName("parameters")] public VideoParameters? Parameters { get; set; }

    /// <summary>
    /// Gets or sets user-specified labels to track billing usage.
    /// </summary>
    [JsonPropertyName("labels")] public Dictionary<string, string>? Labels { get; set; }
}

/// <summary>
/// Internal representation of a single instance within the Vertex AI payload.
/// </summary>
public class VideoInstance
{
    /// <summary>
    /// Gets or sets the text prompt for video generation.
    /// </summary>
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets the optional input image for video generation.
    /// </summary>
    [JsonPropertyName("image")] public ImageSample? Image { get; set; }

    /// <summary>
    /// Gets or sets the optional image to use as the last frame of the video.
    /// </summary>
    [JsonPropertyName("lastFrame")] public ImageSample? LastFrame { get; set; }

    /// <summary>
    /// Gets or sets optional asset or style reference images.
    /// </summary>
    [JsonPropertyName("referenceImages")] public List<VideoGenerationReferenceImage>? ReferenceImages { get; set; }

    /// <summary>
    /// Gets or sets an optional mask for inserting, removing or outpainting content.
    /// </summary>
    [JsonPropertyName("mask")] public VideoGenerationMask? Mask { get; set; }
}

/// <summary>
/// Internal representation of the parameters within the Vertex AI payload.
/// Mirrors GenerateVideosConfig but ensures correct JsonPropertyNames for serialization.
/// </summary>
public class VideoParameters
{
    /// <summary>
    /// Gets or sets the number of video samples to generate.
    /// </summary>
    [JsonPropertyName("sampleCount")] public int? SampleCount { get; set; }

    /// <summary>
    /// Gets or sets the storage URI where the generated video will be saved.
    /// </summary>
    [JsonPropertyName("storageUri")] public string? StorageUri { get; set; }

    /// <summary>
    /// Gets or sets the frames per second for the generated video.
    /// </summary>
    [JsonPropertyName("fps")] public int? Fps { get; set; }

    /// <summary>
    /// Gets or sets the duration of the generated video in seconds.
    /// </summary>
    [JsonPropertyName("durationSeconds")] public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the random seed for deterministic video generation.
    /// </summary>
    [JsonPropertyName("seed")] public int? Seed { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio for the generated video.
    /// </summary>
    [JsonPropertyName("aspectRatio")]
    [JsonConverter(typeof(VideoAspectRatioConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoAspectRatio? AspectRatio { get; set; }

    /// <summary>
    /// Gets or sets the resolution for the generated video.
    /// </summary>
    [JsonPropertyName("resolution")]
    [JsonConverter(typeof(VideoResolutionConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoResolution? Resolution { get; set; }

    /// <summary>
    /// Gets or sets the person generation settings for the video.
    /// </summary>
    [JsonPropertyName("personGeneration")]
    [JsonConverter(typeof(VideoPersonGenerationConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoPersonGeneration? PersonGeneration { get; set; }

    /// <summary>
    /// Gets or sets the negative prompt to avoid certain content in the generated video.
    /// </summary>
    [JsonPropertyName("negativePrompt")] public string? NegativePrompt { get; set; }

    /// <summary>
    /// Gets or sets whether to enhance the prompt for better video generation.
    /// </summary>
    [JsonPropertyName("enhancePrompt")] public bool? EnhancePrompt { get; set; }

    /// <summary>
    /// Gets or sets the Pub/Sub topic for receiving generation status updates.
    /// </summary>
    [JsonPropertyName("pubsubTopic")] public string? PubSubTopic { get; set; }

    /// <summary>
    /// Gets or sets whether to generate audio together with the video.
    /// </summary>
    [JsonPropertyName("generateAudio")] public bool? GenerateAudio { get; set; }

    /// <summary>
    /// Gets or sets the compression quality of the generated video.
    /// </summary>
    [JsonPropertyName("compressionQuality")] public VideoCompressionQuality? CompressionQuality { get; set; }

    /// <summary>
    /// Gets or sets the resize mode of the image input for video generation.
    /// </summary>
    [JsonPropertyName("resizeMode")] public ImageResizeMode? ResizeMode { get; set; }
}