using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// A content block of the Interactions API (text, image, audio, video or document), discriminated by <c>type</c>.
/// Unknown content types are read as <see cref="UnknownInteractionContent"/>.
/// </summary>
[JsonConverter(typeof(InteractionContentConverter))]
public abstract class InteractionContent
{
    /// <summary>
    /// Initializes a new content block with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionContent(string type)
    {
        Type = type;
    }

    /// <summary>
    /// The content type discriminator (see <see cref="InteractionContentTypes"/>).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    /// Fields returned by the API that this SDK does not model yet.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// Creates a text content block.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>A new <see cref="InteractionTextContent"/>.</returns>
    public static InteractionTextContent FromText(string text) => new() { Text = text };
}

/// <summary>
/// A text content block.
/// </summary>
public sealed class InteractionTextContent : InteractionContent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionTextContent"/> class.</summary>
    public InteractionTextContent() : base(InteractionContentTypes.Text)
    {
    }

    /// <summary>Required. The text content.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>Citation information for model-generated content.</summary>
    [JsonPropertyName("annotations")]
    public List<InteractionAnnotation>? Annotations { get; set; }
}

/// <summary>
/// Base class for media content blocks (image, audio, video, document), which carry either inline base64
/// <see cref="Data"/> or a <see cref="Uri"/>.
/// </summary>
public abstract class InteractionMediaContent : InteractionContent
{
    /// <summary>
    /// Initializes a new media content block with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionMediaContent(string type) : base(type)
    {
    }

    /// <summary>The base64-encoded media bytes (inline delivery).</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>The MIME type of the media.</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>The URI of the media (URI delivery, uploaded files, Cloud Storage, etc.).</summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    /// <summary>
    /// Decodes <see cref="Data"/>, or returns <c>null</c> when the media was delivered by <see cref="Uri"/>.
    /// </summary>
    /// <returns>The media bytes, or <c>null</c>.</returns>
    public byte[]? GetDataBytes() => string.IsNullOrEmpty(Data) ? null : Convert.FromBase64String(Data);
}

/// <summary>
/// An image content block.
/// </summary>
public sealed class InteractionImageContent : InteractionMediaContent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionImageContent"/> class.</summary>
    public InteractionImageContent() : base(InteractionContentTypes.Image)
    {
    }

    /// <summary>The media resolution (<c>low</c>, <c>medium</c>, <c>high</c>, <c>ultra_high</c>).</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

/// <summary>
/// An audio content block.
/// </summary>
public sealed class InteractionAudioContent : InteractionMediaContent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionAudioContent"/> class.</summary>
    public InteractionAudioContent() : base(InteractionContentTypes.Audio)
    {
    }

    /// <summary>The number of audio channels.</summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    /// <summary>The sample rate of the audio.</summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }
}

/// <summary>
/// A video content block (input for video understanding, or generated video output).
/// </summary>
public sealed class InteractionVideoContent : InteractionMediaContent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionVideoContent"/> class.</summary>
    public InteractionVideoContent() : base(InteractionContentTypes.Video)
    {
    }

    /// <summary>A user-defined name for this content block that the model can reference.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// How the model processes this video: either the string <c>"static"</c> / <c>"agentic"</c> or an object
    /// such as <c>{"type":"static","fps":1,"start_offset":"0s","end_offset":"10s"}</c>.
    /// </summary>
    [JsonPropertyName("processing")]
    public JsonNode? Processing { get; set; }

    /// <summary>The media resolution (<c>low</c>, <c>medium</c>, <c>high</c>, <c>ultra_high</c>).</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

/// <summary>
/// A document content block (PDF, CSV, Office documents, ...).
/// </summary>
public sealed class InteractionDocumentContent : InteractionMediaContent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionDocumentContent"/> class.</summary>
    public InteractionDocumentContent() : base(InteractionContentTypes.Document)
    {
    }
}

/// <summary>
/// A content block whose <c>type</c> this SDK does not know. <see cref="Raw"/> keeps the original JSON and is
/// written back unchanged.
/// </summary>
public sealed class UnknownInteractionContent : InteractionContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionContent"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionContent(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the content block.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>
/// A citation or other annotation attached to text content, discriminated by <c>type</c>.
/// </summary>
[JsonConverter(typeof(InteractionAnnotationConverter))]
public abstract class InteractionAnnotation
{
    /// <summary>
    /// Initializes a new annotation with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected InteractionAnnotation(string type)
    {
        Type = type;
    }

    /// <summary>The annotation type discriminator.</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Start of the segment of the response that is attributed to this annotation.</summary>
    [JsonPropertyName("start_index")]
    public int? StartIndex { get; set; }

    /// <summary>End of the attributed segment, exclusive.</summary>
    [JsonPropertyName("end_index")]
    public int? EndIndex { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>A URL citation (<c>url_citation</c>).</summary>
public sealed class InteractionUrlCitation : InteractionAnnotation
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUrlCitation"/> class.</summary>
    public InteractionUrlCitation() : base("url_citation")
    {
    }

    /// <summary>The title of the URL.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>The URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>A file citation (<c>file_citation</c>).</summary>
public sealed class InteractionFileCitation : InteractionAnnotation
{
    /// <summary>Initializes a new instance of the <see cref="InteractionFileCitation"/> class.</summary>
    public InteractionFileCitation() : base("file_citation")
    {
    }

    /// <summary>User provided metadata about the retrieved context.</summary>
    [JsonPropertyName("custom_metadata")]
    public JsonObject? CustomMetadata { get; set; }

    /// <summary>The URI of the file.</summary>
    [JsonPropertyName("document_uri")]
    public string? DocumentUri { get; set; }

    /// <summary>The name of the file.</summary>
    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }

    /// <summary>Media ID in case of image citations.</summary>
    [JsonPropertyName("media_id")]
    public string? MediaId { get; set; }

    /// <summary>Page number of the cited document.</summary>
    [JsonPropertyName("page_number")]
    public int? PageNumber { get; set; }

    /// <summary>Source attributed for a portion of the text.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

/// <summary>A place citation from Google Maps grounding (<c>place_citation</c>).</summary>
public sealed class InteractionPlaceCitation : InteractionAnnotation
{
    /// <summary>Initializes a new instance of the <see cref="InteractionPlaceCitation"/> class.</summary>
    public InteractionPlaceCitation() : base("place_citation")
    {
    }

    /// <summary>Title of the place.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The ID of the place, in <c>places/{place_id}</c> format.</summary>
    [JsonPropertyName("place_id")]
    public string? PlaceId { get; set; }

    /// <summary>Review snippets used to generate answers about the place.</summary>
    [JsonPropertyName("review_snippets")]
    public List<InteractionReviewSnippet>? ReviewSnippets { get; set; }

    /// <summary>URI reference of the place.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>Speech synthesis metadata for a text segment (<c>speech_metadata</c>).</summary>
public sealed class InteractionSpeechAnnotation : InteractionAnnotation
{
    /// <summary>Initializes a new instance of the <see cref="InteractionSpeechAnnotation"/> class.</summary>
    public InteractionSpeechAnnotation() : base("speech_metadata")
    {
    }

    /// <summary>The speaker to associate with this turn.</summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>Style instruction for the speech synthesis.</summary>
    [JsonPropertyName("style")]
    public string? Style { get; set; }
}

/// <summary>Word-level transcription information (<c>word_info</c>).</summary>
public sealed class InteractionWordInfo : InteractionAnnotation
{
    /// <summary>Initializes a new instance of the <see cref="InteractionWordInfo"/> class.</summary>
    public InteractionWordInfo() : base("word_info")
    {
    }

    /// <summary>Start offset in time of the word relative to the start of the audio.</summary>
    [JsonPropertyName("start_offset")]
    public string? StartOffset { get; set; }

    /// <summary>End offset in time of the word relative to the start of the audio.</summary>
    [JsonPropertyName("end_offset")]
    public string? EndOffset { get; set; }

    /// <summary>Speaker label for this word (e.g. <c>spk_1</c>).</summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>The transcribed word.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>An annotation whose <c>type</c> this SDK does not know.</summary>
public sealed class UnknownInteractionAnnotation : InteractionAnnotation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionAnnotation"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionAnnotation(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the annotation.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>A Google Maps review snippet.</summary>
public sealed class InteractionReviewSnippet
{
    /// <summary>The ID of the review snippet.</summary>
    [JsonPropertyName("review_id")]
    public string? ReviewId { get; set; }

    /// <summary>Title of the review.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>A link to the user review on Google Maps.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
