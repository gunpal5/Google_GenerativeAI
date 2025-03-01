using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Configuration for sparse emebdding generation.
/// </summary>
public class RagEmbeddingModelConfigSparseEmbeddingConfig
{
    /// <summary>
    /// Use BM25 scoring algorithm.
    /// </summary>
    [JsonPropertyName("bm25")]
    public RagEmbeddingModelConfigSparseEmbeddingConfigBm25? Bm25 { get; set; }
}