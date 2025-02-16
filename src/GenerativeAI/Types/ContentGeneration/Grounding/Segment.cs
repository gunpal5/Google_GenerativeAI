using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Segment of the content.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#Segment">See Official API Documentation</seealso> 
public class Segment
{
    /// <summary>
    /// Output only. The index of a Part object within its parent Content object.
    /// </summary>
    [JsonPropertyName("partIndex")]
    public int? PartIndex { get; set; }

    /// <summary>
    /// Output only. Start index in the given Part, measured in bytes.
    /// Offset from the start of the Part, inclusive, starting at zero.
    /// </summary>
    [JsonPropertyName("startIndex")]
    public int? StartIndex { get; set; }

    /// <summary>
    /// Output only. End index in the given Part, measured in bytes.
    /// Offset from the start of the Part, exclusive, starting at zero.
    /// </summary>
    [JsonPropertyName("endIndex")]
    public int? EndIndex { get; set; }

    /// <summary>
    /// Output only. The text corresponding to the segment from the response.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}