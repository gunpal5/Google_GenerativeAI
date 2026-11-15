using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Message to be sent in the first and only first client message. Contains configuration that will apply for the duration of the streaming session.
/// Clients should wait for a <see cref="LiveServerSetupComplete"/> message before sending any additional messages.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontentsetup">See Official API Documentation</seealso>
public class LiveClientSetup
{
    /// <summary>
    /// Required. The model's resource name. This serves as an ID for the Model to use.
    /// Format: <c>models/{model}</c>
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Generation config.
    /// The following fields are not supported:
    /// - <c>responseLogprobs</c>
    /// - <c>responseMimeType</c>
    /// - <c>logprobs</c>
    /// - <c>responseSchema</c>
    /// - <c>stopSequence</c>
    /// - <c>routingConfig</c>
    /// - <c>audioTimestamp</c>
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// The user provided system instructions for the model.
    /// Note: Only text should be used in parts. Content in each part will be in a separate paragraph.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// A list of <see cref="Tool"/> the model may use to generate the next response.
    /// A <see cref="Tool"/> is a piece of code that enables the system to interact with external systems to perform an action, or set of actions, outside of knowledge and scope of the model.
    /// </summary>
    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }

    /// <summary>
    /// Configures output audio transcription settings.
    /// </summary>
    [JsonPropertyName("outputAudioTranscription")]
    public AudioTranscriptionConfig? OutputAudioTranscription { get; set; }

    /// <summary>
    /// Configures input audio transcription settings.
    /// </summary>
    [JsonPropertyName("inputAudioTranscription")]
    public AudioTranscriptionConfig? InputAudioTranscription { get; set; }
    /// <summary>
    /// Configures context window compression mechanism. If included, server will compress context window to fit into given length.
    /// </summary>
    [JsonPropertyName("contextWindowCompression")]
    public ContextWindowCompressionConfig? ContextWindowCompression { get; set; }
    
    /// <summary>
    /// Configures the proactivity of the model. This allows the model to respond proactively to the input and to ignore irrelevant input.
    /// </summary>
    [JsonPropertyName("proactivity")]
    public ProactivityConfig? Proactivity { get; set; }

    /// <summary>
    /// Configures session resumption mechanism. If included server will send SessionResumptionUpdate messages.
    /// </summary>
    [JsonPropertyName("sessionResumption")]
    public SessionResumptionConfig? SessionResumption { get; set; }

    /// <summary>
    /// Configures the realtime input behavior, such as automatic activity (voice) detection,
    /// what effect user activity has and which input is included in the user's turn.
    /// </summary>
    [JsonPropertyName("realtimeInputConfig")]
    public RealtimeInputConfig? RealtimeInputConfig { get; set; }

    /// <summary>
    /// Configures the exchange of history between the client and the server.
    /// </summary>
    [JsonPropertyName("historyConfig")]
    public HistoryConfig? HistoryConfig { get; set; }

    /// <summary>
    /// Configures the explicit VAD signal. If enabled, the client will send VAD signals to indicate
    /// the start and end of speech. This allows the server to process the audio more efficiently.
    /// Vertex AI only; not supported by the Gemini Developer API.
    /// </summary>
    [JsonPropertyName("explicitVadSignal")]
    public bool? ExplicitVadSignal { get; set; }

    /// <summary>
    /// Configures the avatar model behavior.
    /// </summary>
    [JsonPropertyName("avatarConfig")]
    public AvatarConfig? AvatarConfig { get; set; }

    /// <summary>
    /// Safety settings in the request to block unsafe content in the response.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    /// <summary>
    /// Optional. Labels with user-defined metadata for the request. Label keys must start with a letter;
    /// keys and values can be no longer than 63 characters and can only contain lowercase letters, numeric
    /// characters, underscores, and dashes. Use the key <c>safety_identifier</c> for safety identifiers from aggregators.
    /// Gemini Developer API only; not supported by Vertex AI.
    /// </summary>
    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }
}

/// <summary>
/// Configures context window compression mechanism. If included, server will compress context window to fit into given length.
/// </summary>
public class ContextWindowCompressionConfig
{
    /// <summary>
    /// Sliding window compression mechanism.
    /// </summary>
    [JsonPropertyName("slidingWindow")]
    public SlidingWindow? SlidingWindow { get; set; }

    /// <summary>
    /// Number of tokens (before running turn) that triggers context window compression mechanism.
    /// </summary>
    [JsonPropertyName("triggerTokens")]
    public int? TriggerTokens { get; set; }
}

/// <summary>
/// Context window will be truncated by keeping only suffix of it.
/// Context window will always be cut at start of USER role turn. System
/// instructions and LiveClientSetup.prefix_turns will not be
/// subject to the sliding window mechanism, they will always stay at the
/// beginning of context window.
/// </summary>
public class SlidingWindow
{
    /// <summary>
    /// Session reduction target -- how many tokens we should keep. Window shortening operation has some latency costs,
    /// so we should avoid running it on every turn. Should be &lt; trigger_tokens. If not set, trigger_tokens/2 is assumed.
    /// </summary>
    [JsonPropertyName("targetTokens")]
    public int? TargetTokens { get; set; }
}

/// <summary>
/// Configuration of session resumption mechanism.
/// Included in LiveConnectConfig.session_resumption. If included server
/// will send LiveServerSessionResumptionUpdate messages.
/// </summary>
public class SessionResumptionConfig
{
    /// <summary>
    /// Session resumption handle of previous session (session to restore).
    /// If not present new session will be started.
    /// </summary>
    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    /// <summary>
    /// If set the server will send last_consumed_client_message_index in the session_resumption_update messages
    /// to allow for transparent reconnections.
    /// Vertex AI only; not supported by the Gemini Developer API.
    /// </summary>
    [JsonPropertyName("transparent")]
    public bool? Transparent { get; set; }
}




/// <summary>
/// Configures the proactivity of the model.
/// </summary>
public class ProactivityConfig
{
    /// <summary>
    /// If enabled, the model can reject responding to the last prompt. For example, this allows the model
    /// to ignore out of context speech or to stay silent if the user did not make a request, yet.
    /// </summary>
    [JsonPropertyName("proactiveAudio")]
    public bool? ProactiveAudio { get; set; }

    /// <summary>
    /// If enabled, the system might trigger a generation on each received image. This requires that
    /// <see cref="ProactiveAudio"/> is enabled. The rate of generation depends on the rate of incoming
    /// images and can cause a significant increase in cost.
    /// </summary>
    [JsonPropertyName("proactiveVideo")]
    public bool? ProactiveVideo { get; set; }
}


/// <summary>
/// Configuration settings for audio transcription in multimodal live sessions.
/// </summary>
public class AudioTranscriptionConfig
{
    /// <summary>
    /// Optional. BCP-47 language codes providing hints about the languages present in the audio.
    /// If omitted or empty, defaults to automatic language detection.
    /// </summary>
    [JsonPropertyName("languageCodes")]
    public List<string>? LanguageCodes { get; set; }

    /// <summary>
    /// Deprecated: Auto-detection is now the default when <see cref="LanguageCodes"/> is omitted.
    /// </summary>
    [Obsolete("Auto-detection is now the default when LanguageCodes is omitted.")]
    [JsonPropertyName("languageAuto")]
    public LanguageAuto? LanguageAuto { get; set; }

    /// <summary>
    /// Deprecated: Use <see cref="LanguageCodes"/> instead.
    /// </summary>
    [Obsolete("Use LanguageCodes instead.")]
    [JsonPropertyName("languageHints")]
    public LanguageHints? LanguageHints { get; set; }

    /// <summary>
    /// A list of custom vocabulary phrases, which biases the ASR model to improve recognition of these specific terms.
    /// </summary>
    [JsonPropertyName("customVocabulary")]
    public List<string>? CustomVocabulary { get; set; }

    /// <summary>
    /// Deprecated. A list of phrases used for speech adaptation, which biases the ASR model to improve
    /// recognition of these specific terms. Use <see cref="CustomVocabulary"/> instead.
    /// </summary>
    [Obsolete("Use CustomVocabulary instead.")]
    [JsonPropertyName("adaptationPhrases")]
    public List<string>? AdaptationPhrases { get; set; }

    /// <summary>
    /// Configures word-level timestamp generation.
    /// </summary>
    [JsonPropertyName("wordTimestamp")]
    public bool? WordTimestamp { get; set; }

    /// <summary>
    /// Configures speaker diarization.
    /// </summary>
    [JsonPropertyName("diarization")]
    public bool? Diarization { get; set; }

    /// <summary>
    /// Optional. Configures transcription mode. If unspecified, defaults to <see cref="AudioTranscriptionConfigMode.VERBATIM"/>.
    /// In <see cref="AudioTranscriptionConfigMode.SMART"/> mode, the model performs disfluency removal, light grammatical
    /// cleanup, automatic formatting and minor user edits. Timestamps and diarization are incompatible with SMART mode.
    /// </summary>
    [JsonPropertyName("mode")]
    public AudioTranscriptionConfigMode? Mode { get; set; }

    /// <summary>
    /// Optional. The source text to be edited or rewritten via voice instructions. When provided in
    /// <see cref="AudioTranscriptionConfigMode.SMART"/> mode, the model evaluates the audio input against this text
    /// and emits the complete replacement text rather than appending to it.
    /// </summary>
    [JsonPropertyName("sourceText")]
    public string? SourceText { get; set; }
}