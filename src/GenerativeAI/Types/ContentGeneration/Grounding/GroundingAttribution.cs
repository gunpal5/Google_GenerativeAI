using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Attribution for a source that contributed to an answer.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#GroundingAttribution">See Official API Documentation</seealso> 
public class GroundingAttribution
{
    /// <summary>
    /// Output only. Identifier for the source contributing to this attribution.
    /// </summary>
    [JsonPropertyName("sourceId")]
    public AttributionSourceId? SourceId { get; set; }

    /// <summary>
    /// Grounding source content that makes up this attribution.
    /// </summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }
}