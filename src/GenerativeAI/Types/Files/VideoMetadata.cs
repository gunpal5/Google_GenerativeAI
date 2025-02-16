using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata for a video <c>File</c>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/files#VideoMetadata">See Official API Documentation</seealso> 
public class VideoMetadata
{
    /// <summary>
    /// Duration of the video.
    /// A duration in seconds with up to nine fractional digits, ending with '<c>s</c>'.
    /// Example: <c>"3.5s"</c>.
    /// </summary>
    [JsonPropertyName("videoDuration")]
    public Duration? VideoDuration { get; set; }
}