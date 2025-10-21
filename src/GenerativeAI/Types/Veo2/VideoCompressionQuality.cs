using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Enum that controls the compression quality of the generated videos.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VideoCompressionQuality>))]
public enum VideoCompressionQuality
{
    /// <summary>
    /// Optimized video compression quality. This will produce videos with a compressed, smaller file size.
    /// </summary>
    OPTIMIZED = 0,

    /// <summary>
    /// Lossless video compression quality. This will produce videos with a larger file size.
    /// </summary>
    LOSSLESS = 1
}
