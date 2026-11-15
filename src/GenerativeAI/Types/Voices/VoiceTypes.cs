using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A voice: a custom voice created with <c>POST /voices</c> (designed from a prompt or replicated from audio) or a
/// prebuilt system voice from Google's voice catalog (returned by <c>GET /voices</c>). Use its
/// <see cref="Key"/> / <see cref="Id"/> as the voice of a text-to-speech request.
/// </summary>
public sealed class Voice
{
    /// <summary>Required. The voice type (see <see cref="VoiceTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Output only. The voice ID.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Output only. The key used to reference this voice in speech generation requests.</summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>User-provided display name of a stored voice, or the catalog name of a prebuilt voice.</summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>Descriptive summary of vocal timbre, personality and tone.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Regional accent descriptor (e.g. "American", "British").</summary>
    [JsonPropertyName("accent")]
    public string? Accent { get; set; }

    /// <summary>Optimal usage context or domain (e.g. "Conversational", "Audiobook", "News").</summary>
    [JsonPropertyName("context")]
    public string? Context { get; set; }

    /// <summary>Perceived gender presentation (e.g. "female", "male", "neutral").</summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>Primary BCP-47 language tag (e.g. "en-US").</summary>
    [JsonPropertyName("language_code")]
    public string? LanguageCode { get; set; }

    /// <summary>ISO 3166-1 alpha-2 or UN M.49 region code (e.g. "US", "001").</summary>
    [JsonPropertyName("region_code")]
    public string? RegionCode { get; set; }

    /// <summary>
    /// The model used to design or replicate the voice. If omitted when creating a voice, the latest supported voice
    /// design model is used.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Intended persona or character archetype (e.g. "Warm, Friendly", "Narrator").</summary>
    [JsonPropertyName("persona")]
    public string? Persona { get; set; }

    /// <summary>Vocal pitch (see <see cref="VoicePitches"/>).</summary>
    [JsonPropertyName("pitch")]
    public string? Pitch { get; set; }

    /// <summary>Parameters of a prompted (designed) voice. Required when <see cref="Type"/> is <c>prompted</c>.</summary>
    [JsonPropertyName("prompted")]
    public PromptedVoice? Prompted { get; set; }

    /// <summary>
    /// Input only. Parameters of a replicated voice. Required when <see cref="Type"/> is <c>replicated</c>.
    /// </summary>
    [JsonPropertyName("replicated")]
    public ReplicatedVoice? Replicated { get; set; }

    /// <summary>A sample of the voice.</summary>
    [JsonPropertyName("sample_audio")]
    public VoiceAudioData? SampleAudio { get; set; }

    /// <summary>Output only. When a voice that was not stored expires (RFC 3339).</summary>
    [JsonPropertyName("expire_time")]
    public string? ExpireTime { get; set; }

    /// <summary>Output only. Token usage of the voice creation.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Parameters of a voice designed from a text prompt.</summary>
public sealed class PromptedVoice
{
    /// <summary>Required. The prompt describing the voice (or the remix instructions).</summary>
    [JsonPropertyName("input")]
    public string? Input { get; set; }

    /// <summary>Optional. The voice to remix (e.g. the key of an existing voice).</summary>
    [JsonPropertyName("base_voice")]
    public string? BaseVoice { get; set; }
}

/// <summary>Parameters of a voice replicated from recorded audio.</summary>
public sealed class ReplicatedVoice
{
    /// <summary>Required. Audio of the speaker consenting to the replication of their voice.</summary>
    [JsonPropertyName("consent_audio")]
    public VoiceAudioData? ConsentAudio { get; set; }

    /// <summary>Required. Audio of the voice to replicate.</summary>
    [JsonPropertyName("source_audio")]
    public VoiceAudioData? SourceAudio { get; set; }
}

/// <summary>Inline audio data.</summary>
public sealed class VoiceAudioData
{
    /// <summary>Required. The base64-encoded audio bytes.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>Required. The MIME type of the audio (e.g. <c>audio/wav</c>).</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Creates audio data from raw bytes.
    /// </summary>
    /// <param name="bytes">The audio bytes.</param>
    /// <param name="mimeType">The MIME type.</param>
    /// <returns>The audio data.</returns>
    public static VoiceAudioData FromBytes(byte[] bytes, string mimeType) =>
        new() { Data = Convert.ToBase64String(bytes ?? throw new ArgumentNullException(nameof(bytes))), MimeType = mimeType };
}

/// <summary>Request body of <c>POST /voices</c>.</summary>
public sealed class CreateVoiceRequest
{
    /// <summary>Required. The voice to create.</summary>
    [JsonPropertyName("voice")]
    public Voice? Voice { get; set; }

    /// <summary>Whether to store the voice (so it can be listed and reused); otherwise it expires.</summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }
}

/// <summary>Response of <c>GET /voices</c>.</summary>
public sealed class ListVoicesResponse
{
    /// <summary>The voices: custom voices first (newest first), then prebuilt voices.</summary>
    [JsonPropertyName("voices")]
    public List<Voice>? Voices { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>Response of <c>DELETE /voices/{id}</c>.</summary>
public sealed class DeleteVoiceResponse
{
    /// <summary>Fields returned by the API that this SDK does not model.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Filters of <c>GET /voices</c>. Each list filter matches any of its values (case-insensitive).</summary>
public sealed class ListVoicesOptions
{
    /// <summary>Filter by accent (e.g. "American").</summary>
    public IList<string>? Accent { get; set; }

    /// <summary>Filter by context or domain (sent as <c>context</c>).</summary>
    public IList<string>? Contexts { get; set; }

    /// <summary>Filter by gender presentation.</summary>
    public IList<string>? Gender { get; set; }

    /// <summary>Filter by BCP-47 language code.</summary>
    public IList<string>? LanguageCode { get; set; }

    /// <summary>Filter by persona.</summary>
    public IList<string>? Persona { get; set; }

    /// <summary>Filter by pitch (see <see cref="VoicePitches"/>).</summary>
    public IList<string>? Pitch { get; set; }

    /// <summary>Filter by region code.</summary>
    public IList<string>? RegionCode { get; set; }

    /// <summary>Filter by voice type (see <see cref="VoiceTypes"/>).</summary>
    public IList<string>? Type { get; set; }

    /// <summary>Free-text substring search over display name and description.</summary>
    public string? Search { get; set; }
}

/// <summary>Known voice types.</summary>
public static class VoiceTypes
{
    /// <summary>A voice replicated from recorded audio.</summary>
    public const string Replicated = "replicated";

    /// <summary>A voice designed (or remixed) from a text prompt.</summary>
    public const string Prompted = "prompted";

    /// <summary>A prebuilt system voice.</summary>
    public const string Prebuilt = "prebuilt";
}

/// <summary>Known voice pitches.</summary>
public static class VoicePitches
{
    /// <summary>Low pitch.</summary>
    public const string Low = "low";

    /// <summary>Medium pitch.</summary>
    public const string Medium = "medium";

    /// <summary>High pitch.</summary>
    public const string High = "high";
}
