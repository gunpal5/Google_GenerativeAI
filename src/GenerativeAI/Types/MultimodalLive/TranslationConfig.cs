using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for stream translation. Sent on the wire as <c>setup.generationConfig.translationConfig</c>.
/// </summary>
public class TranslationConfig
{
    /// <summary>
    /// If true, the model will generate audio when the target language is spoken, essentially it will parrot
    /// the input. If false, no audio is produced for the target language.
    /// </summary>
    [JsonPropertyName("echoTargetLanguage")]
    public bool? EchoTargetLanguage { get; set; }

    /// <summary>
    /// The target language for translation. Supported values are BCP-47 language codes (e.g. "en", "es", "fr").
    /// </summary>
    [JsonPropertyName("targetLanguageCode")]
    public string? TargetLanguageCode { get; set; }
}
