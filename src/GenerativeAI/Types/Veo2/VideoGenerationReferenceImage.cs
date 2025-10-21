using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A reference image for video generation.
/// </summary>
public class VideoGenerationReferenceImage
{
    /// <summary>
    /// The reference image.
    /// </summary>
    [JsonPropertyName("image")]
    public ImageSample? Image { get; set; }

    /// <summary>
    /// The type of the reference image, which defines how the reference image will be used to generate the video.
    /// </summary>
    [JsonPropertyName("referenceType")]
    public VideoGenerationReferenceType? ReferenceType { get; set; }
}
