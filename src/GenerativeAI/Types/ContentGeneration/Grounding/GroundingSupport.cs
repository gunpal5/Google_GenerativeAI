using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Grounding support.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/GroundingSupport">See Official API Documentation</seealso> 
public class GroundingSupport
{
    /// <summary>
    /// A list of indices (into 'grounding_chunk') specifying the citations associated with the claim.
    /// For instance, means that grounding_chunk, grounding_chunk, grounding_chunk
    /// are the retrieved content attributed to the claim.
    /// </summary>
    [JsonPropertyName("groundingChunkIndices")]
    public List<int>? GroundingChunkIndices { get; set; }

    /// <summary>
    /// Confidence score of the support references. Ranges from 0 to 1. 1 is the most confident.
    /// This list must have the same size as the groundingChunkIndices.
    /// </summary>
    [JsonPropertyName("confidenceScores")]
    public List<double>? ConfidenceScores { get; set; }

    /// <summary>
    /// Segment of the content this support belongs to.
    /// </summary>
    [JsonPropertyName("segment")]
    public Segment? Segment { get; set; }
}