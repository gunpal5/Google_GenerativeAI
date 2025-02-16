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
}