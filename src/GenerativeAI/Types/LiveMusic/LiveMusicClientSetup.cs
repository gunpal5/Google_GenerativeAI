using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Message to be sent by the system when connecting to the API.
/// </summary>
public class LiveMusicClientSetup
{
    /// <summary>
    /// The model's resource name. Format: `models/{model}`.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
