using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The config for the Vertex Vector Search.
/// </summary>
public class RagVectorDbConfigVertexVectorSearch
{
    /// <summary>
    /// The resource name of the Index. Format: `projects/{project}/locations/{location}/indexes/{index}`
    /// </summary>
    [JsonPropertyName("index")]
    public string? Index { get; set; }

    /// <summary>
    /// The resource name of the Index Endpoint. Format: `projects/{project}/locations/{location}/indexEndpoints/{index_endpoint}`
    /// </summary>
    [JsonPropertyName("indexEndpoint")]
    public string? IndexEndpoint { get; set; }
}