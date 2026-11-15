using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Transcription mode used by <see cref="AudioTranscriptionConfig.Mode"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AudioTranscriptionConfigMode>))]
public enum AudioTranscriptionConfigMode
{
    /// <summary>
    /// Unspecified transcription mode.
    /// </summary>
    MODE_UNSPECIFIED = 0,

    /// <summary>
    /// Verbatim transcription mode.
    /// </summary>
    VERBATIM = 1,

    /// <summary>
    /// Smart transcription mode: disfluency removal, light grammatical cleanup and automatic formatting.
    /// </summary>
    SMART = 2
}

/// <summary>
/// Deprecated: Language auto-detection is now the default when <see cref="AudioTranscriptionConfig.LanguageCodes"/> is omitted.
/// </summary>
public class LanguageAuto
{
}

/// <summary>
/// Deprecated: Use <see cref="AudioTranscriptionConfig.LanguageCodes"/> instead.
/// </summary>
public class LanguageHints
{
    /// <summary>
    /// Deprecated. BCP-47 language codes.
    /// </summary>
    [JsonPropertyName("languageCodes")]
    public List<string>? LanguageCodes { get; set; }
}
