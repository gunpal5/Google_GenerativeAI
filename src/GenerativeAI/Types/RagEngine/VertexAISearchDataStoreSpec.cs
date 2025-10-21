using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Define data stores within engine to filter on in a search call and configurations for those data stores.
/// For more information, see https://cloud.google.com/generative-ai-app-builder/docs/reference/rpc/google.cloud.discoveryengine.v1#datastorespec
/// </summary>
public class VertexAISearchDataStoreSpec
{
    /// <summary>
    /// Full resource name of DataStore.
    /// Format: `projects/{project}/locations/{location}/collections/{collection}/dataStores/{dataStore}`
    /// </summary>
    [JsonPropertyName("dataStore")]
    public string? DataStore { get; set; }

    /// <summary>
    /// Optional. Filter specification to filter documents in the data store specified by dataStore field.
    /// For more information on filtering, see https://cloud.google.com/generative-ai-app-builder/docs/filter-search-metadata
    /// </summary>
    [JsonPropertyName("filter")]
    public string? Filter { get; set; }
}
