using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The response returned after a successful video generation request (often part of an Operation).
/// </summary>
public class GenerateVideosResponse
{
    /// <summary>
    /// A list containing the generated video(s).
    /// </summary>
    [JsonPropertyName("generatedVideos")]
    public List<Video>? GeneratedVideos { get; set; }

    /// <summary>
    /// Optional. Indicates the number of videos that were filtered due to Responsible AI (RAI) policies.
    /// </summary>
    [JsonPropertyName("raiMediaFilteredCount")]
    public int? RaiMediaFilteredCount { get; set; }

    /// <summary>
    /// Optional. Provides reasons why videos were filtered, if any occurred due to RAI policies.
    /// </summary>
    [JsonPropertyName("raiMediaFilteredReasons")]
    public List<string>? RaiMediaFilteredReasons { get; set; }
}