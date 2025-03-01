using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Configurations for hybrid search results ranking.
/// </summary>
public class RagQueryRanking
{
    /// <summary>
    /// Optional. Alpha value controls the weight between dense and sparse vector search results. The range is [0, 1], while 0 means sparse vector search only and 1 means dense vector search only. The default value is 0.5 which balances sparse and dense vector search equally.
    /// </summary>
    [JsonPropertyName("alpha")]
    public float? Alpha { get; set; }
}