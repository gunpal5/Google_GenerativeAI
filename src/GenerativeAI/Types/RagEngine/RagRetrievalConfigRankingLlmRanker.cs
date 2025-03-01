using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for LlmRanker.
/// </summary>
public class RagRetrievalConfigRankingLlmRanker
{
    /// <summary>
    /// Optional. The model name used for ranking. Format: `gemini-1.5-pro`
    /// </summary>
    [JsonPropertyName("modelName")]
    public string? ModelName { get; set; }
}