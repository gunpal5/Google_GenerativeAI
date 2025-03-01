using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config for the Vertex AI Search.
/// </summary>
public class VertexAiSearchConfig
{
    /// <summary>
    /// Vertex AI Search Serving Config resource full name. For example, `projects/{project}/locations/{location}/collections/{collection}/engines/{engine}/servingConfigs/{serving_config}` or `projects/{project}/locations/{location}/collections/{collection}/dataStores/{data_store}/servingConfigs/{serving_config}`.
    /// </summary>
    [JsonPropertyName("servingConfig")]
    public string? ServingConfig { get; set; } 
}