using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tool to support Google Maps in Model.
/// </summary>
public class GoogleMaps
{
    /// <summary>
    /// Optional. Auth config for the Google Maps tool.
    /// </summary>
    [JsonPropertyName("authConfig")]
    public AuthConfig? AuthConfig { get; set; }

    /// <summary>
    /// Optional. If true, include the widget context token in the response.
    /// </summary>
    [JsonPropertyName("enableWidget")]
    public bool? EnableWidget { get; set; }

    /// <summary>
    /// The kinds of Maps grounding to use (places and/or routing). Vertex AI only.
    /// </summary>
    [JsonPropertyName("groundingTypes")]
    public GoogleMapsGroundingTypes? GroundingTypes { get; set; }
}
