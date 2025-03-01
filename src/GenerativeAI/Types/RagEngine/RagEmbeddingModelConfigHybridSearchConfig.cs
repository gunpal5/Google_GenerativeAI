using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for hybrid search.
/// </summary>
public class RagEmbeddingModelConfigHybridSearchConfig
{
    /// <summary>
    /// Required. The Vertex AI Prediction Endpoint that hosts the embedding model for dense embedding generations.
    /// </summary>
    [JsonPropertyName("denseEmbeddingModelPredictionEndpoint")]
    public RagEmbeddingModelConfigVertexPredictionEndpoint? DenseEmbeddingModelPredictionEndpoint { get; set; }

    /// <summary>
    /// Optional. The configuration for sparse embedding generation. This field is optional the default behavior depends on the vector database choice on the RagCorpus.
    /// </summary>
    [JsonPropertyName("sparseEmbeddingConfig")]
    public RagEmbeddingModelConfigSparseEmbeddingConfig? SparseEmbeddingConfig { get; set; }
}