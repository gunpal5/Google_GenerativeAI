using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for speaking with a voice replicated from an audio sample.
/// </summary>
public class ReplicatedVoiceConfig
{
    /// <summary>
    /// MIME type of <see cref="VoiceSampleAudio"/> and <see cref="ConsentAudio"/>.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Base64-encoded audio sample of the voice to replicate.
    /// </summary>
    [JsonPropertyName("voiceSampleAudio")]
    public string? VoiceSampleAudio { get; set; }

    /// <summary>
    /// Base64-encoded recording of the speaker consenting to replication of their voice.
    /// </summary>
    [JsonPropertyName("consentAudio")]
    public string? ConsentAudio { get; set; }

    /// <summary>
    /// Signature returned by a previous request that already verified consent, so the consent audio need not be resent.
    /// </summary>
    [JsonPropertyName("voiceConsentSignature")]
    public VoiceConsentSignature? VoiceConsentSignature { get; set; }
}

/// <summary>
/// Proof that voice-replication consent was verified.
/// </summary>
public class VoiceConsentSignature
{
    /// <summary>
    /// The signature value.
    /// </summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}
