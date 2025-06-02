using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Message to be sent in the first and only first client message. Contains configuration that will apply for the duration of the streaming session.
/// Clients should wait for a <see cref="BidiGenerateContentSetupComplete"/> message before sending any additional messages.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontentsetup">See Official API Documentation</seealso>
public class BidiGenerateContentSetup
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

    [JsonPropertyName("outputAudioTranscription")]
    public AudioTranscriptionConfig? OutputAudioTranscription { get; set; } 

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
/// instructions and BidiGenerateContentSetup.prefix_turns will not be
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
    /// </summary>
    [JsonPropertyName("transparent")]
    public bool? Transparent { get; set; }
}




/// <summary>
/// Configures the proactivity of the model.
/// </summary>
public class ProactivityConfig
{
    // Add properties for ProactivityConfig if available
    [JsonPropertyName("proactiveAudio")]
    public bool? ProactiveAudio { get; set; }
}


public class AudioTranscriptionConfig
{

}