using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// An endpoint where you deploy models.
/// </summary>
public class Endpoint
{
    /// <summary>
    /// Resource name of the endpoint.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// ID of the model that's deployed to the endpoint.
    /// </summary>
    [JsonPropertyName("deployedModelId")]
    public string? DeployedModelId { get; set; }
}
