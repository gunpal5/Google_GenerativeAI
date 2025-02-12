using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The configuration for the prebuilt speaker to use.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#PrebuiltVoiceConfig">See Official API Documentation</seealso>
public class PrebuiltVoiceConfig
{
    /// <summary>
    /// The name of the preset voice to use.
    /// </summary>
    [JsonPropertyName("voiceName")]
    public string? VoiceName { get; set; }
}