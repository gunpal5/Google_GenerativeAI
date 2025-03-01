using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The config for the Pinecone.
/// </summary>
public class RagVectorDbConfigPinecone
{
    /// <summary>
    /// Pinecone index name. This value cannot be changed after it's set.
    /// </summary>
    [JsonPropertyName("indexName")]
    public string? IndexName { get; set; }
}