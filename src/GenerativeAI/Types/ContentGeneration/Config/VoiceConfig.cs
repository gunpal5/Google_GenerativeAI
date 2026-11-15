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

    /// <summary>
    /// Name of a custom voice resource to speak with.
    /// </summary>
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    /// <summary>
    /// Speak with a voice replicated from an audio sample.
    /// </summary>
    [JsonPropertyName("replicatedVoiceConfig")]
    public ReplicatedVoiceConfig? ReplicatedVoiceConfig { get; set; }
}