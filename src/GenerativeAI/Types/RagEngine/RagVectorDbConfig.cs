using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for the Vector DB to use for RAG.
/// </summary>
public class RagVectorDbConfig
{
    /// <summary>
    /// Authentication config for the chosen Vector DB.
    /// </summary>
    [JsonPropertyName("apiAuth")]
    public ApiAuth? ApiAuth { get; set; }

    /// <summary>
    /// The config for the Pinecone.
    /// </summary>
    [JsonPropertyName("pinecone")]
    public RagVectorDbConfigPinecone? Pinecone { get; set; }

    /// <summary>
    /// Optional. Immutable. The embedding model config of the Vector DB.
    /// </summary>
    [JsonPropertyName("ragEmbeddingModelConfig")]
    public RagEmbeddingModelConfig? RagEmbeddingModelConfig { get; set; }

    /// <summary>
    /// The config for the RAG-managed Vector DB.
    /// </summary>
    [JsonPropertyName("ragManagedDb")]
    public RagVectorDbConfigRagManagedDb? RagManagedDb { get; set; }

    /// <summary>
    /// The config for the Vertex Feature Store.
    /// </summary>
    [JsonPropertyName("vertexFeatureStore")]
    public RagVectorDbConfigVertexFeatureStore? VertexFeatureStore { get; set; }

    /// <summary>
    /// The config for the Vertex Vector Search.
    /// </summary>
    [JsonPropertyName("vertexVectorSearch")]
    public RagVectorDbConfigVertexVectorSearch? VertexVectorSearch { get; set; }

    /// <summary>
    /// The config for the Weaviate.
    /// </summary>
    [JsonPropertyName("weaviate")]
    public RagVectorDbConfigWeaviate? Weaviate { get; set; }
}