using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The configuration for the multi-speaker setup.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#MultiSpeakerVoiceConfig">See Official API Documentation</seealso>
public class MultiSpeakerVoiceConfig
{
    /// <summary>
    /// Required. All the enabled speaker voices.
    /// </summary>
    [JsonPropertyName("speakerVoiceConfigs")]
    public List<SpeakerVoiceConfig>? SpeakerVoiceConfigs { get; set; }
}