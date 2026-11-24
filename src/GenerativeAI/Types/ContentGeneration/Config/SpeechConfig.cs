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

    /// <summary>
    /// Optional. Controls text normalization before speech synthesis.
    /// </summary>
    [JsonPropertyName("textNormalization")]
    public SpeechConfigTextNormalization? TextNormalization { get; set; }
}

/// <summary>
/// Text normalization mode applied before speech synthesis.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SpeechConfigTextNormalization>))]
public enum SpeechConfigTextNormalization
{
    /// <summary>
    /// Unspecified text normalization mode. Defaults to <see cref="AUTO"/>.
    /// </summary>
    TEXT_NORMALIZATION_UNSPECIFIED = 0,

    /// <summary>
    /// Automatically determines whether to apply text normalization based on the model and speech mode.
    /// </summary>
    AUTO = 1,

    /// <summary>
    /// Enables text normalization for numbers, currencies, dates and units.
    /// </summary>
    ENABLED = 2,

    /// <summary>
    /// Disables text normalization for pre-normalized or phonetic inputs.
    /// </summary>
    DISABLED = 3
}