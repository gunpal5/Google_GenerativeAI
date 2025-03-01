using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for ranking and reranking.
/// </summary>
public class RagRetrievalConfigRanking
{
    /// <summary>
    /// Optional. Config for LlmRanker.
    /// </summary>
    [JsonPropertyName("llmRanker")]
    public RagRetrievalConfigRankingLlmRanker? LlmRanker { get; set; }

    /// <summary>
    /// Optional. Config for Rank Service.
    /// </summary>
    [JsonPropertyName("rankService")]
    public RagRetrievalConfigRankingRankService? RankService { get; set; }
}