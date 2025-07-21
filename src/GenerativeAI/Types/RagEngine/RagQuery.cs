﻿using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// A query to retrieve relevant contexts.
/// </summary>
public class RagQuery
{
    /// <summary>
    /// Optional. The retrieval config for the query.
    /// </summary>
    [JsonPropertyName("ragRetrievalConfig")]
    public RagRetrievalConfig? RagRetrievalConfig { get; set; }

    /// <summary>
    /// Optional. Configurations for hybrid search results ranking.
    /// </summary>
    [JsonPropertyName("ranking")]
    [System.Obsolete("Use RagRetrievalConfig property instead. Ranking property will be removed in a future version.")]
    public RagQueryRanking? Ranking { get; set; }

    /// <summary>
    /// Optional. The number of contexts to retrieve.
    /// </summary>
    [JsonPropertyName("similarityTopK")]
    [System.Obsolete("Use RagRetrievalConfig property instead. SimilarityTopK property will be removed in a future version.")]
    public int? SimilarityTopK { get; set; }

    /// <summary>
    /// Optional. The query in text format to get relevant contexts.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}