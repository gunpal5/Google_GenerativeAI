using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a single generated video as part of a video generation response.
/// </summary>
public class GeneratedVideo
{
    /// <summary>
    /// The generated video content.
    /// </summary>
    [JsonPropertyName("video")]
    public Video? Video { get; set; }
}