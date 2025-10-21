using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Parameters for connecting to the live API.
/// </summary>
public class LiveMusicConnectParameters
{
    /// <summary>
    /// The model's resource name.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
