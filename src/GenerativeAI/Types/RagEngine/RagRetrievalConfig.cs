using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the context retrieval config.
/// </summary>
public class RagRetrievalConfig
{
    /// <summary>
    /// Optional. Config for filters.
    /// </summary>
    [JsonPropertyName("filter")]
    public RagRetrievalConfigFilter? Filter { get; set; }

    /// <summary>
    /// Optional. Config for Hybrid Search.
    /// </summary>
    [JsonPropertyName("hybridSearch")]
    public RagRetrievalConfigHybridSearch? HybridSearch { get; set; }

    /// <summary>
    /// Optional. Config for ranking and reranking.
    /// </summary>
    [JsonPropertyName("ranking")]
    public RagRetrievalConfigRanking? Ranking { get; set; }

    /// <summary>
    /// Optional. The number of contexts to retrieve.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}