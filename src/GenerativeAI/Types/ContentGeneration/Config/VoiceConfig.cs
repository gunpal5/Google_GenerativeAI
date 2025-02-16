using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The configuration for the voice to use.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#VoiceConfig">See Official API Documentation</seealso>
public class VoiceConfig
{
    /// <summary>
    /// The configuration for the prebuilt voice to use.
    /// </summary>
    [JsonPropertyName("prebuiltVoiceConfig")]
    public PrebuiltVoiceConfig? PrebuiltVoiceConfig { get; set; }
}