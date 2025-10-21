using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Retrieval config for tool configuration.
/// </summary>
public class RetrievalConfig
{
    /// <summary>
    /// Optional. The location of the user.
    /// </summary>
    [JsonPropertyName("latLng")]
    public LatLng? LatLng { get; set; }

    /// <summary>
    /// The language code of the user.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; set; }
}
