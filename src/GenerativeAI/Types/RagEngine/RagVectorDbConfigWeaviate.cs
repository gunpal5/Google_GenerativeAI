using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The config for the Weaviate.
/// </summary>
public class RagVectorDbConfigWeaviate
{
    /// <summary>
    /// The corresponding collection this corpus maps to. This value cannot be changed after it's set.
    /// </summary>
    [JsonPropertyName("collectionName")]
    public string? CollectionName { get; set; }

    /// <summary>
    /// Weaviate DB instance HTTP endpoint. e.g. 34.56.78.90:8080 Vertex RAG only supports HTTP connection to Weaviate. This value cannot be changed after it's set.
    /// </summary>
    [JsonPropertyName("httpEndpoint")]
    public string? HttpEndpoint { get; set; }

   

}