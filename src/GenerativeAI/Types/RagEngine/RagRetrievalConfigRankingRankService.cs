using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for Rank Service.
/// </summary>
public class RagRetrievalConfigRankingRankService
{
    /// <summary>
    /// Optional. The model name of the rank service. Format: `semantic-ranker-512@latest`
    /// </summary>
    [JsonPropertyName("modelName")]
    public string? ModelName { get; set; }
}