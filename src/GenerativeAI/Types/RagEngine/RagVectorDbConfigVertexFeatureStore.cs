using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The config for the Vertex Feature Store.
/// </summary>
public class RagVectorDbConfigVertexFeatureStore
{
    /// <summary>
    /// The resource name of the FeatureView. Format: `projects/{project}/locations/{location}/featureOnlineStores/{feature_online_store}/featureViews/{feature_view}`
    /// </summary>
    [JsonPropertyName("featureViewResourceName")]
    public string? FeatureViewResourceName { get; set; }
}