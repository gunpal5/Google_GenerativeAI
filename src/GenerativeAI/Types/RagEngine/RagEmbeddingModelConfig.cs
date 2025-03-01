using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for the embedding model to use for RAG.
/// </summary>
public class RagEmbeddingModelConfig
{
    /// <summary>
    /// Configuration for hybrid search.
    /// </summary>
    [JsonPropertyName("hybridSearchConfig")]
    public RagEmbeddingModelConfigHybridSearchConfig? HybridSearchConfig { get; set; }

    /// <summary>
    /// The Vertex AI Prediction Endpoint that either refers to a publisher model or an endpoint that is hosting a 1P fine-tuned text embedding model. Endpoints hosting non-1P fine-tuned text embedding models are currently not supported. This is used for dense vector search.
    /// </summary>
    [JsonPropertyName("vertexPredictionEndpoint")]
    public RagEmbeddingModelConfigVertexPredictionEndpoint? VertexPredictionEndpoint { get; set; }
}