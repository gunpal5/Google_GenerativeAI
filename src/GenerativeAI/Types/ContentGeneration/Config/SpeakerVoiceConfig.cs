using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The configuration for a single speaker in a multi speaker setup.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#SpeakerVoiceConfig">See Official API Documentation</seealso>
public class SpeakerVoiceConfig
{
    /// <summary>
    /// Required. The name of the speaker to use. Should be the same as in the prompt.
    /// </summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>
    /// Required. The configuration for the voice to use.
    /// </summary>
    [JsonPropertyName("voiceConfig")]
    public VoiceConfig? VoiceConfig { get; set; }
}