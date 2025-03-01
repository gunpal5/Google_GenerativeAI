using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for filters.
/// </summary>
public class RagRetrievalConfigFilter
{
    /// <summary>
    /// Optional. String for metadata filtering.
    /// </summary>
    [JsonPropertyName("metadataFilter")]
    public string? MetadataFilter { get; set; }

    /// <summary>
    /// Optional. Only returns contexts with vector distance smaller than the threshold.
    /// </summary>
    [JsonPropertyName("vectorDistanceThreshold")]
    public double? VectorDistanceThreshold { get; set; }

    /// <summary>
    /// Optional. Only returns contexts with vector similarity larger than the threshold.
    /// </summary>
    [JsonPropertyName("vectorSimilarityThreshold")]
    public double? VectorSimilarityThreshold { get; set; }
}