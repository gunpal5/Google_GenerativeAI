using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The speech generation config.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#SpeechConfig">See Official API Documentation</seealso>
public class SpeechConfig
{
    /// <summary>
    /// The configuration for the speaker to use.
    /// </summary>
    [JsonPropertyName("voiceConfig")]
    public VoiceConfig? VoiceConfig { get; set; }

    /// <summary>
    /// Optional. The configuration for the multi-speaker setup.
    /// It is mutually exclusive with the <see cref="VoiceConfig"/> field.
    /// </summary>
    [JsonPropertyName("multiSpeakerVoiceConfig")]
    public MultiSpeakerVoiceConfig? MultiSpeakerVoiceConfig { get; set; }

    /// <summary>
    /// Optional. Language code (in BCP 47 format, e.g. "en-US") for speech synthesis.
    /// Valid values are shown at the above URI.
    /// </summary>
    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; set; }
}