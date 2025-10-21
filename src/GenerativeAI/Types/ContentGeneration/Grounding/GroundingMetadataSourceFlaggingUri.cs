using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Source content flagging uri for a place or review.
/// This is currently populated only for Google Maps grounding.
/// </summary>
public class GroundingMetadataSourceFlaggingUri
{
    /// <summary>
    /// A link where users can flag a problem with the source (place or review).
    /// </summary>
    [JsonPropertyName("flagContentUri")]
    public string? FlagContentUri { get; set; }

    /// <summary>
    /// Id of the place or review.
    /// </summary>
    [JsonPropertyName("sourceId")]
    public string? SourceId { get; set; }
}
