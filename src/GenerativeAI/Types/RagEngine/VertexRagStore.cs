using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Retrieve from Vertex RAG Store for grounding.
/// </summary>
public class VertexRagStore
{
    /// <summary>
    /// Optional. The representation of the rag source. It can be used to specify corpus only or ragfiles. Currently only support one corpus or multiple files from one corpus. In the future we may open up multiple corpora support.
    /// </summary>
    [JsonPropertyName("ragResources")]
    public ICollection<VertexRagStoreRagResource>? RagResources { get; set; } 

    /// <summary>
    /// Optional. The retrieval config for the Rag query.
    /// </summary>
    [JsonPropertyName("ragRetrievalConfig")]
    public RagRetrievalConfig? RagRetrievalConfig { get; set; } 
    
    /// <summary>
    /// Optional. Deprecated. Please use rag_resources instead.
    /// </summary>
    [JsonPropertyName("ragCorpora")]
    public List<string>? RagCorpora { get; set; }
    /// <summary>
    /// Optional. Number of top k results to return from the selected corpora.
    /// </summary>
    [JsonPropertyName("similarityTopK")]
    public int? SimilarityTopK { get; set; }

    /// <summary>
    /// Optional. Only return results with vector distance smaller than the threshold.
    /// </summary>
    [JsonPropertyName("vectorDistanceThreshold")]
    public float? VectorDistanceThreshold { get; set; }
}