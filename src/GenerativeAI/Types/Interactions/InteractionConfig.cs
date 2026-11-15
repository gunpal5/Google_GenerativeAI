using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Generation parameters for model interactions (<c>generation_config</c>). Not allowed together with an agent.
/// </summary>
public sealed class InteractionGenerationConfig
{
    /// <summary>The maximum number of tokens to include in the response.</summary>
    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>Seed used in decoding for reproducibility.</summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>A list of character sequences that will stop output.</summary>
    [JsonPropertyName("stop_sequences")]
    public List<string>? StopSequences { get; set; }

    /// <summary>Deprecated in the API. Controls the randomness of the output.</summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>Deprecated in the API. The maximum cumulative probability of tokens to consider when sampling.</summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>The thinking level (see <see cref="InteractionThinkingLevels"/>).</summary>
    [JsonPropertyName("thinking_level")]
    public string? ThinkingLevel { get; set; }

    /// <summary>Whether thought summaries are returned (see <see cref="InteractionThinkingSummaries"/>).</summary>
    [JsonPropertyName("thinking_summaries")]
    public string? ThinkingSummaries { get; set; }

    /// <summary>The tool choice configuration.</summary>
    [JsonPropertyName("tool_choice")]
    public InteractionToolChoice? ToolChoice { get; set; }

    /// <summary>
    /// Speech and multi-speaker configuration. The API accepts either an object
    /// <c>{"speakers":[{"speaker":..,"voice":..,"language":..}]}</c> or an array of
    /// <c>{"speaker":..,"voice":..,"language":..}</c> objects.
    /// </summary>
    [JsonPropertyName("speech_config")]
    public JsonNode? SpeechConfig { get; set; }

    /// <summary>Configuration for speech recognition (transcription).</summary>
    [JsonPropertyName("transcription_config")]
    public InteractionTranscriptionConfig? TranscriptionConfig { get; set; }

    /// <summary>Configuration options for video generation.</summary>
    [JsonPropertyName("video_config")]
    public InteractionVideoConfig? VideoConfig { get; set; }

    /// <summary>Deprecated in the API in favour of <see cref="InteractionImageResponseFormat"/>. Image generation configuration.</summary>
    [JsonPropertyName("image_config")]
    public InteractionImageConfig? ImageConfig { get; set; }
}

/// <summary>
/// Tool choice: either a <see cref="Mode"/> string (see <see cref="InteractionToolChoiceModes"/>) or an
/// <see cref="AllowedTools"/> configuration.
/// </summary>
[JsonConverter(typeof(InteractionToolChoiceConverter))]
public sealed class InteractionToolChoice
{
    /// <summary>The tool choice mode, written as a plain string when <see cref="AllowedTools"/> is not set.</summary>
    public string? Mode { get; set; }

    /// <summary>The allowed tools configuration, written as <c>{"allowed_tools": {...}}</c>.</summary>
    public InteractionAllowedTools? AllowedTools { get; set; }
}

/// <summary>Speech recognition (transcription) configuration.</summary>
public sealed class InteractionTranscriptionConfig
{
    /// <summary>Custom vocabulary phrases to bias the speech recognition model towards.</summary>
    [JsonPropertyName("custom_vocabulary")]
    public List<string>? CustomVocabulary { get; set; }

    /// <summary>BCP-47 language codes present in the audio.</summary>
    [JsonPropertyName("language_codes")]
    public List<string>? LanguageCodes { get; set; }

    /// <summary>
    /// The transcription mode: the string <c>"verbatim"</c> / <c>"smart"</c>, or an object such as
    /// <c>{"type":"smart","source_text":"..."}</c> or
    /// <c>{"type":"verbatim","diarization_mode":"speaker","timestamp_granularities":["word"]}</c>.
    /// </summary>
    [JsonPropertyName("mode")]
    public JsonNode? Mode { get; set; }

    /// <summary>Deprecated in the API. Phrases to bias the ASR model towards.</summary>
    [JsonPropertyName("adaptation_phrases")]
    public List<string>? AdaptationPhrases { get; set; }

    /// <summary>Deprecated in the API. Speaker diarization mode (<c>speaker</c>).</summary>
    [JsonPropertyName("diarization_mode")]
    public string? DiarizationMode { get; set; }

    /// <summary>Deprecated in the API; use <see cref="LanguageCodes"/>.</summary>
    [JsonPropertyName("language_hints")]
    public List<string>? LanguageHints { get; set; }

    /// <summary>Deprecated in the API. Granularity of timestamps in the transcription output.</summary>
    [JsonPropertyName("timestamp_granularities")]
    public List<string>? TimestampGranularities { get; set; }
}

/// <summary>Video generation configuration.</summary>
public sealed class InteractionVideoConfig
{
    /// <summary>
    /// Optional task mode: <c>text_to_video</c>, <c>image_to_video</c>, <c>reference_to_video</c>, <c>edit</c>,
    /// <c>extend</c> or <c>upscale</c>. If not specified, the model infers it from the input.
    /// </summary>
    [JsonPropertyName("task")]
    public string? Task { get; set; }
}

/// <summary>Image generation configuration (deprecated in the API).</summary>
public sealed class InteractionImageConfig
{
    /// <summary>The aspect ratio of the image (e.g. <c>16:9</c>).</summary>
    [JsonPropertyName("aspect_ratio")]
    public string? AspectRatio { get; set; }

    /// <summary>The image size (<c>512</c>, <c>1K</c>, <c>2K</c>, <c>4K</c>).</summary>
    [JsonPropertyName("image_size")]
    public string? ImageSize { get; set; }
}

/// <summary>
/// Requested output format (<c>response_format</c>), discriminated by <c>type</c>: text (structured output),
/// image, audio or video. Formats this SDK does not know are kept as <see cref="UnknownInteractionResponseFormat"/>.
/// </summary>
[JsonConverter(typeof(InteractionResponseFormatConverter))]
public abstract class InteractionResponseFormat
{
    /// <summary>
    /// Initializes a new response format with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionResponseFormat(string type)
    {
        Type = type;
    }

    /// <summary>The format type discriminator.</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Text output format, e.g. JSON with a schema (<c>type: text</c>).</summary>
public sealed class InteractionTextResponseFormat : InteractionResponseFormat
{
    /// <summary>Initializes a new instance of the <see cref="InteractionTextResponseFormat"/> class.</summary>
    public InteractionTextResponseFormat() : base("text")
    {
    }

    /// <summary>The MIME type of the text output (<c>application/json</c>, <c>text/plain</c>).</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>The JSON Schema the output should conform to (for <c>application/json</c>).</summary>
    [JsonPropertyName("schema")]
    public JsonNode? Schema { get; set; }
}

/// <summary>Image output format (<c>type: image</c>).</summary>
public sealed class InteractionImageResponseFormat : InteractionResponseFormat
{
    /// <summary>Initializes a new instance of the <see cref="InteractionImageResponseFormat"/> class.</summary>
    public InteractionImageResponseFormat() : base("image")
    {
    }

    /// <summary>The aspect ratio (e.g. <c>1:1</c>, <c>16:9</c>).</summary>
    [JsonPropertyName("aspect_ratio")]
    public string? AspectRatio { get; set; }

    /// <summary>The delivery mode (see <see cref="InteractionDeliveryModes"/>).</summary>
    [JsonPropertyName("delivery")]
    public string? Delivery { get; set; }

    /// <summary>The image size (<c>512</c>, <c>1K</c>, <c>2K</c>, <c>4K</c>).</summary>
    [JsonPropertyName("image_size")]
    public string? ImageSize { get; set; }

    /// <summary>The MIME type of the image output (<c>image/jpeg</c>).</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
}

/// <summary>Audio output format (<c>type: audio</c>).</summary>
public sealed class InteractionAudioResponseFormat : InteractionResponseFormat
{
    /// <summary>Initializes a new instance of the <see cref="InteractionAudioResponseFormat"/> class.</summary>
    public InteractionAudioResponseFormat() : base("audio")
    {
    }

    /// <summary>Bit rate in bits per second (compressed formats only).</summary>
    [JsonPropertyName("bit_rate")]
    public int? BitRate { get; set; }

    /// <summary>The delivery mode (see <see cref="InteractionDeliveryModes"/>).</summary>
    [JsonPropertyName("delivery")]
    public string? Delivery { get; set; }

    /// <summary>The MIME type (<c>audio/mp3</c>, <c>audio/ogg_opus</c>, <c>audio/l16</c>, <c>audio/wav</c>, <c>audio/alaw</c>, <c>audio/mulaw</c>).</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>Sample rate in Hz.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }
}

/// <summary>Video output format (<c>type: video</c>).</summary>
public sealed class InteractionVideoResponseFormat : InteractionResponseFormat
{
    /// <summary>Initializes a new instance of the <see cref="InteractionVideoResponseFormat"/> class.</summary>
    public InteractionVideoResponseFormat() : base("video")
    {
    }

    /// <summary>The aspect ratio (<c>16:9</c>, <c>9:16</c>).</summary>
    [JsonPropertyName("aspect_ratio")]
    public string? AspectRatio { get; set; }

    /// <summary>The delivery mode (see <see cref="InteractionDeliveryModes"/>).</summary>
    [JsonPropertyName("delivery")]
    public string? Delivery { get; set; }

    /// <summary>The duration of the video (e.g. <c>8s</c>).</summary>
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    /// <summary>The Cloud Storage URI to store the video (required on Vertex AI for URI delivery).</summary>
    [JsonPropertyName("gcs_uri")]
    public string? GcsUri { get; set; }

    /// <summary>The resolution (<c>360p</c>, <c>720p</c>, <c>1080p</c>, <c>4k</c>). Defaults to 720p.</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

/// <summary>A response format this SDK does not know, or a free-form JSON object.</summary>
public sealed class UnknownInteractionResponseFormat : InteractionResponseFormat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionResponseFormat"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionResponseFormat(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the response format.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>
/// Agent configuration (<c>agent_config</c>), discriminated by <c>type</c>.
/// </summary>
[JsonConverter(typeof(InteractionAgentConfigConverter))]
public abstract class InteractionAgentConfig
{
    /// <summary>
    /// Initializes a new agent configuration with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionAgentConfig(string type)
    {
        Type = type;
    }

    /// <summary>The agent configuration type (<c>deep-research</c>, <c>antigravity</c>, <c>code-mender</c>, <c>dynamic</c>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Additional fields (for <see cref="DynamicAgentConfig"/> these are the configuration itself).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Configuration of the Deep Research agent (<c>deep-research</c>).</summary>
public sealed class DeepResearchAgentConfig : InteractionAgentConfig
{
    /// <summary>Initializes a new instance of the <see cref="DeepResearchAgentConfig"/> class.</summary>
    public DeepResearchAgentConfig() : base("deep-research")
    {
    }

    /// <summary>Enables human-in-the-loop planning.</summary>
    [JsonPropertyName("collaborative_planning")]
    public bool? CollaborativePlanning { get; set; }

    /// <summary>Enables the BigQuery tool.</summary>
    [JsonPropertyName("enable_bigquery_tool")]
    public bool? EnableBigQueryTool { get; set; }

    /// <summary>Whether thought summaries are returned (see <see cref="InteractionThinkingSummaries"/>).</summary>
    [JsonPropertyName("thinking_summaries")]
    public string? ThinkingSummaries { get; set; }

    /// <summary>Whether to include visualizations in the response (<c>off</c>, <c>auto</c>).</summary>
    [JsonPropertyName("visualization")]
    public string? Visualization { get; set; }
}

/// <summary>Configuration of the Antigravity agent (<c>antigravity</c>).</summary>
public sealed class AntigravityAgentConfig : InteractionAgentConfig
{
    /// <summary>Initializes a new instance of the <see cref="AntigravityAgentConfig"/> class.</summary>
    public AntigravityAgentConfig() : base("antigravity")
    {
    }

    /// <summary>Max total tokens for the agent run (an int64, sent as a JSON string).</summary>
    [JsonPropertyName("max_total_tokens")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long? MaxTotalTokens { get; set; }

    /// <summary>The model to use for agent reasoning.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

/// <summary>A source file given to the CodeMender agent.</summary>
public sealed class CodeMenderSourceFile
{
    /// <summary>The UTF-8 text content of the file.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>The relative path of the file from the project root.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }
}

/// <summary>Parameters of a CodeMender FIND session.</summary>
public sealed class CodeMenderFindRequest
{
    /// <summary>Additional context or instructions.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The finding to verify.</summary>
    [JsonPropertyName("finding_id")]
    public string? FindingId { get; set; }

    /// <summary>The mode of the find session (<c>scan</c>, <c>verify</c>).</summary>
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    /// <summary>Source files to scan.</summary>
    [JsonPropertyName("source_files")]
    public List<CodeMenderSourceFile>? SourceFiles { get; set; }
}

/// <summary>Parameters of a CodeMender FIX session.</summary>
public sealed class CodeMenderFixRequest
{
    /// <summary>Additional context or instructions.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The finding to remediate.</summary>
    [JsonPropertyName("finding_id")]
    public string? FindingId { get; set; }

    /// <summary>Source files providing context for the remediation.</summary>
    [JsonPropertyName("source_files")]
    public List<CodeMenderSourceFile>? SourceFiles { get; set; }
}

/// <summary>CodeMender session configuration.</summary>
public sealed class CodeMenderSessionConfig
{
    /// <summary>The maximum number of interaction rounds.</summary>
    [JsonPropertyName("max_rounds")]
    public int? MaxRounds { get; set; }
}

/// <summary>Configuration of the CodeMender agent (<c>code-mender</c>).</summary>
public sealed class CodeMenderAgentConfig : InteractionAgentConfig
{
    /// <summary>Initializes a new instance of the <see cref="CodeMenderAgentConfig"/> class.</summary>
    public CodeMenderAgentConfig() : base("code-mender")
    {
    }

    /// <summary>Parameters of a FIND session.</summary>
    [JsonPropertyName("find_request")]
    public CodeMenderFindRequest? FindRequest { get; set; }

    /// <summary>Parameters of a FIX session.</summary>
    [JsonPropertyName("fix_request")]
    public CodeMenderFixRequest? FixRequest { get; set; }

    /// <summary>The model used by the agent.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>Session configuration.</summary>
    [JsonPropertyName("session_config")]
    public CodeMenderSessionConfig? SessionConfig { get; set; }

    /// <summary>Groups multiple interactions that belong to the same session.</summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}

/// <summary>
/// Free-form agent configuration (<c>dynamic</c>); put the settings in <see cref="InteractionAgentConfig.ExtensionData"/>.
/// </summary>
public sealed class DynamicAgentConfig : InteractionAgentConfig
{
    /// <summary>Initializes a new instance of the <see cref="DynamicAgentConfig"/> class.</summary>
    public DynamicAgentConfig() : base("dynamic")
    {
    }
}

/// <summary>An agent configuration whose <c>type</c> this SDK does not know.</summary>
public sealed class UnknownInteractionAgentConfig : InteractionAgentConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionAgentConfig"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionAgentConfig(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the agent configuration.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>A source mounted into a remote agent environment.</summary>
public sealed class InteractionEnvironmentSource
{
    /// <summary>The source type (<c>gcs</c>, <c>inline</c>, <c>repository</c>, <c>skill_registry</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The source of the environment (e.g. a bucket or repository URL).</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>Where the source should appear in the environment.</summary>
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    /// <summary>The inline content when <see cref="Type"/> is <c>inline</c>.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>Optional encoding of the inline content (e.g. <c>base64</c>).</summary>
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }
}

/// <summary>A remote sandbox environment configuration for agent interactions.</summary>
public sealed class InteractionEnvironment
{
    /// <summary>The environment type; always <c>remote</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "remote";

    /// <summary>Reuse an existing environment by ID.</summary>
    [JsonPropertyName("environment_id")]
    public string? EnvironmentId { get; set; }

    /// <summary>
    /// Environment variables. Each value is a string or an object <c>{"value": "..."}</c> /
    /// <c>{"credential": "credential-id"}</c>.
    /// </summary>
    [JsonPropertyName("env")]
    public Dictionary<string, JsonNode>? Env { get; set; }

    /// <summary>
    /// Network configuration: the string <c>"disabled"</c> or an object
    /// <c>{"allowlist": [{"domain": "...", "credential": "...", "transform": {...}}]}</c>.
    /// </summary>
    [JsonPropertyName("network")]
    public JsonNode? Network { get; set; }

    /// <summary>Sources to mount into the environment.</summary>
    [JsonPropertyName("sources")]
    public List<InteractionEnvironmentSource>? Sources { get; set; }
}

/// <summary>
/// The <c>environment</c> of an interaction: either an existing environment ID or an
/// <see cref="InteractionEnvironment"/> configuration.
/// </summary>
[JsonConverter(typeof(InteractionEnvironmentReferenceConverter))]
public sealed class InteractionEnvironmentReference
{
    /// <summary>
    /// Creates a reference to an existing environment by ID.
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    public InteractionEnvironmentReference(string environmentId)
    {
        EnvironmentId = environmentId;
    }

    /// <summary>
    /// Creates an environment reference from a configuration.
    /// </summary>
    /// <param name="environment">The environment configuration.</param>
    public InteractionEnvironmentReference(InteractionEnvironment environment)
    {
        Environment = environment;
    }

    /// <summary>The referenced environment ID, when the environment is given as a string.</summary>
    public string? EnvironmentId { get; }

    /// <summary>The environment configuration, when the environment is given as an object.</summary>
    public InteractionEnvironment? Environment { get; }

    /// <summary>Converts an environment ID to a reference.</summary>
    /// <param name="environmentId">The environment ID.</param>
    public static implicit operator InteractionEnvironmentReference(string environmentId) => new(environmentId);

    /// <summary>Converts an environment configuration to a reference.</summary>
    /// <param name="environment">The environment configuration.</param>
    public static implicit operator InteractionEnvironmentReference(InteractionEnvironment environment) => new(environment);

    /// <summary>Creates a reference to an existing environment by ID.</summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <returns>The reference.</returns>
    public static InteractionEnvironmentReference FromString(string environmentId) => new(environmentId);

    /// <summary>Creates an environment reference from a configuration.</summary>
    /// <param name="environment">The environment configuration.</param>
    /// <returns>The reference.</returns>
    public static InteractionEnvironmentReference FromInteractionEnvironment(InteractionEnvironment environment) => new(environment);
}

/// <summary>A safety setting for an interaction.</summary>
public sealed class InteractionSafetySetting
{
    /// <summary>The harm category (e.g. <c>hate_speech</c>, <c>dangerous_content</c>, <c>harassment</c>, <c>sexually_explicit</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The blocking threshold (<c>block_low_and_above</c>, <c>block_medium_and_above</c>, <c>block_only_high</c>, <c>block_none</c>, <c>off</c>).</summary>
    [JsonPropertyName("threshold")]
    public string? Threshold { get; set; }

    /// <summary>The blocking method (<c>severity</c>, <c>probability</c>).</summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }
}

/// <summary>Webhook configuration for an interaction.</summary>
public sealed class InteractionWebhookConfig
{
    /// <summary>Webhook URIs to use for this interaction's events instead of the registered webhooks.</summary>
    [JsonPropertyName("uris")]
    public List<string>? Uris { get; set; }

    /// <summary>User metadata returned on each event emission.</summary>
    [JsonPropertyName("user_metadata")]
    public JsonObject? UserMetadata { get; set; }
}

/// <summary>
/// The input of an interaction: a string, a single content block, a list of content blocks, or a list of steps
/// (for example the previous turns plus function results).
/// </summary>
[JsonConverter(typeof(InteractionInputConverter))]
public sealed class InteractionInput
{
    /// <summary>
    /// Creates a text input.
    /// </summary>
    /// <param name="text">The text.</param>
    public InteractionInput(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Creates an input made of a single content block.
    /// </summary>
    /// <param name="content">The content block.</param>
    public InteractionInput(InteractionContent content)
    {
        Content = content;
    }

    private InteractionInput()
    {
    }

    /// <summary>The input text, when the input is a string.</summary>
    public string? Text { get; private set; }

    /// <summary>The input content, when the input is a single content block.</summary>
    public InteractionContent? Content { get; private set; }

    /// <summary>The input contents, when the input is a list of content blocks.</summary>
    public List<InteractionContent>? Contents { get; private set; }

    /// <summary>The input steps, when the input is a list of steps.</summary>
    public List<InteractionStep>? Steps { get; private set; }

    /// <summary>Creates a text input.</summary>
    /// <param name="text">The text.</param>
    public static implicit operator InteractionInput(string text) => new(text);

    /// <summary>Creates a text input.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The input.</returns>
    public static InteractionInput FromText(string text) => new(text);

    /// <summary>Creates a text input (same as <see cref="FromText"/>).</summary>
    /// <param name="text">The text.</param>
    /// <returns>The input.</returns>
    public static InteractionInput FromString(string text) => new(text);

    /// <summary>Creates an input from content blocks (e.g. text plus an image).</summary>
    /// <param name="contents">The content blocks.</param>
    /// <returns>The input.</returns>
    public static InteractionInput FromContents(IEnumerable<InteractionContent> contents) =>
        new() { Contents = contents?.ToList() ?? throw new ArgumentNullException(nameof(contents)) };

    /// <summary>Creates an input from steps (e.g. function results).</summary>
    /// <param name="steps">The steps.</param>
    /// <returns>The input.</returns>
    public static InteractionInput FromSteps(IEnumerable<InteractionStep> steps) =>
        new() { Steps = steps?.ToList() ?? throw new ArgumentNullException(nameof(steps)) };
}
