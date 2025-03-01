using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Config representing a model hosted on Vertex Prediction Endpoint.
/// </summary>
public class RagEmbeddingModelConfigVertexPredictionEndpoint
{
    /// <summary>
    /// Required. The endpoint resource name. Format: `projects/{project}/locations/{location}/publishers/{publisher}/models/{model}` or `projects/{project}/locations/{location}/endpoints/{endpoint}`
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Output only. The resource name of the model that is deployed on the endpoint. Present only when the endpoint is not a publisher model. Pattern: `projects/{project}/locations/{location}/models/{model}`
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Output only. Version ID of the model that is deployed on the endpoint. Present only when the endpoint is not a publisher model.
    /// </summary>
    [JsonPropertyName("modelVersionId")]
    public string? ModelVersionId { get; set; }

   

}