using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Retrieve from Vertex AI Search datastore for grounding. See https://cloud.google.com/products/agent-builder
/// </summary>
public class VertexAISearch
{
    /// <summary>
    /// Required. Fully-qualified Vertex AI Search data store resource ID. Format: `projects/{project}/locations/{location}/collections/{collection}/dataStores/{dataStore}`
    /// </summary>
    [JsonPropertyName("datastore")]
    public string? Datastore { get; set; } 
}