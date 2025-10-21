using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Retrieve from Vertex AI Search datastore or engine for grounding.
/// Datastore and engine are mutually exclusive. See https://cloud.google.com/products/agent-builder
/// </summary>
public class VertexAISearch
{
    /// <summary>
    /// Optional. Fully-qualified Vertex AI Search data store resource ID.
    /// Format: `projects/{project}/locations/{location}/collections/{collection}/dataStores/{dataStore}`
    /// </summary>
    [JsonPropertyName("datastore")]
    public string? Datastore { get; set; }

    /// <summary>
    /// Optional. Fully-qualified Vertex AI Search engine resource ID.
    /// Format: `projects/{project}/locations/{location}/collections/{collection}/engines/{engine}`
    /// </summary>
    [JsonPropertyName("engine")]
    public string? Engine { get; set; }

    /// <summary>
    /// Specifications that define the specific DataStores to be searched, along with configurations for those data stores.
    /// This is only considered for Engines with multiple data stores. It should only be set if engine is used.
    /// </summary>
    [JsonPropertyName("dataStoreSpecs")]
    public List<VertexAISearchDataStoreSpec>? DataStoreSpecs { get; set; }

    /// <summary>
    /// Optional. Filter strings to be passed to the search API.
    /// </summary>
    [JsonPropertyName("filter")]
    public string? Filter { get; set; }

    /// <summary>
    /// Optional. Number of search results to return per query. The default value is 10. The maximum allowed value is 10.
    /// </summary>
    [JsonPropertyName("maxResults")]
    public int? MaxResults { get; set; }
}