using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A mask for video generation.
/// </summary>
public class VideoGenerationMask
{
    /// <summary>
    /// The image mask to use for generating videos.
    /// </summary>
    [JsonPropertyName("image")]
    public ImageSample? Image { get; set; }

    /// <summary>
    /// Describes how the mask will be used. Inpainting masks must match the aspect ratio of the input video.
    /// Outpainting masks can be either 9:16 or 16:9.
    /// </summary>
    [JsonPropertyName("maskMode")]
    public VideoGenerationMaskMode? MaskMode { get; set; }
}
