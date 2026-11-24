using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Static processing options of a video (<c>{"type":"static", ...}</c>), the object form of
/// <see cref="InteractionVideoContent.Processing"/>. Mirrors python-genai's <c>StaticMediaProcessing</c>.
/// </summary>
public sealed class InteractionStaticMediaProcessing
{
    /// <summary>The processing type; always <c>static</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; } = InteractionMediaProcessingModes.Static;

    /// <summary>Video frame-rate sampling density.</summary>
    [JsonPropertyName("fps")]
    public double? Fps { get; set; }

    /// <summary>Segment start time, e.g. <c>"10.5s"</c>. Must be non-negative.</summary>
    [JsonPropertyName("start_offset")]
    public string? StartOffset { get; set; }

    /// <summary>Segment end time, e.g. <c>"30s"</c>. Must be greater than <see cref="StartOffset"/> when it is set.</summary>
    [JsonPropertyName("end_offset")]
    public string? EndOffset { get; set; }

    /// <summary>Content layers to extract from the video (see <see cref="InteractionVideoContentLayers"/>).</summary>
    [JsonPropertyName("content_layers")]
    public List<string>? ContentLayers { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// Serializes these options to a JSON node, for <see cref="InteractionVideoContent.Processing"/>.
    /// </summary>
    /// <returns>The JSON object.</returns>
    public JsonNode ToJsonNode() =>
        JsonSerializer.SerializeToNode(this, InteractionsSerializerContext.Default.InteractionStaticMediaProcessing)!;
}

/// <summary>
/// Static processing options of an audio input (<c>{"type":"static", ...}</c>). Mirrors python-genai's
/// <c>AudioStaticMediaProcessing</c> (alias <c>AudioMediaProcessing</c>).
/// </summary>
public sealed class InteractionAudioStaticMediaProcessing
{
    /// <summary>The processing type; always <c>static</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; } = InteractionMediaProcessingModes.Static;

    /// <summary>
    /// Segment start time, relative to the start of the audio stream, e.g. <c>"10.5s"</c>. Must be non-negative.
    /// </summary>
    [JsonPropertyName("start_offset")]
    public string? StartOffset { get; set; }

    /// <summary>
    /// Segment end time, relative to the start of the audio stream, e.g. <c>"30s"</c>. Must be positive and greater
    /// than <see cref="StartOffset"/> when it is set.
    /// </summary>
    [JsonPropertyName("end_offset")]
    public string? EndOffset { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>
/// Processing options for paginated documents, such as PDFs, Office documents and presentations.
/// </summary>
public sealed class InteractionPageProcessingOptions
{
    /// <summary>Content layers to extract from a paginated document (see <see cref="InteractionPageContentLayers"/>).</summary>
    [JsonPropertyName("content_layers")]
    public List<string>? ContentLayers { get; set; }

    /// <summary>1-indexed start page, inclusive.</summary>
    [JsonPropertyName("start_page")]
    public int? StartPage { get; set; }

    /// <summary>1-indexed end page, inclusive. Must be &gt;= <see cref="StartPage"/> when it is set.</summary>
    [JsonPropertyName("end_page")]
    public int? EndPage { get; set; }
}

/// <summary>
/// Processing options for spreadsheets.
/// </summary>
public sealed class InteractionSpreadsheetProcessingOptions
{
    /// <summary>Content layers to extract from a spreadsheet (see <see cref="InteractionSpreadsheetContentLayers"/>).</summary>
    [JsonPropertyName("content_layers")]
    public List<string>? ContentLayers { get; set; }

    /// <summary>The 1-indexed index of the sheet to extract, in workbook order. All sheets when not set.</summary>
    [JsonPropertyName("sheet_index")]
    public int? SheetIndex { get; set; }

    /// <summary>
    /// The range of cells to extract, in A1 notation (e.g. <c>"A1:D10"</c>). The full occupied area of the sheet
    /// when not set.
    /// </summary>
    [JsonPropertyName("visible_cells")]
    public string? VisibleCells { get; set; }
}

/// <summary>
/// Static processing options of a document input (<c>{"type":"static", ...}</c>). Mirrors python-genai's
/// <c>DocumentStaticMediaProcessing</c> (alias <c>DocumentMediaProcessing</c>).
/// </summary>
public sealed class InteractionDocumentStaticMediaProcessing
{
    /// <summary>The processing type; always <c>static</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; } = InteractionMediaProcessingModes.Static;

    /// <summary>Processing options for paginated documents (PDFs, Office documents, presentations).</summary>
    [JsonPropertyName("page_options")]
    public InteractionPageProcessingOptions? PageOptions { get; set; }

    /// <summary>Processing options for spreadsheets.</summary>
    [JsonPropertyName("spreadsheet_options")]
    public InteractionSpreadsheetProcessingOptions? SpreadsheetOptions { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>
/// The <c>processing</c> of an audio input (python-genai <c>AudioProcessing</c>): either a <see cref="Mode"/> string
/// (see <see cref="InteractionMediaProcessingModes"/>) or an <see cref="InteractionAudioStaticMediaProcessing"/> object.
/// </summary>
[JsonConverter(typeof(InteractionAudioProcessingConverter))]
public sealed class InteractionAudioProcessing
{
    /// <summary>Creates a processing mode (e.g. <c>"static"</c>).</summary>
    /// <param name="mode">The mode.</param>
    public InteractionAudioProcessing(string mode)
    {
        Mode = mode;
    }

    /// <summary>Creates processing options.</summary>
    /// <param name="options">The options.</param>
    public InteractionAudioProcessing(InteractionAudioStaticMediaProcessing options)
    {
        Options = options;
    }

    /// <summary>The processing mode, when the processing is given as a string.</summary>
    public string? Mode { get; }

    /// <summary>The processing options, when the processing is given as an object.</summary>
    public InteractionAudioStaticMediaProcessing? Options { get; }

    /// <summary>Converts a mode string to a processing value.</summary>
    /// <param name="mode">The mode.</param>
    public static implicit operator InteractionAudioProcessing(string mode) => new(mode);

    /// <summary>Converts processing options to a processing value.</summary>
    /// <param name="options">The options.</param>
    public static implicit operator InteractionAudioProcessing(InteractionAudioStaticMediaProcessing options) => new(options);

    /// <summary>Creates a processing mode value.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The processing value.</returns>
    public static InteractionAudioProcessing FromString(string mode) => new(mode);

    /// <summary>Creates a processing value from options.</summary>
    /// <param name="options">The options.</param>
    /// <returns>The processing value.</returns>
    public static InteractionAudioProcessing FromInteractionAudioStaticMediaProcessing(
        InteractionAudioStaticMediaProcessing options) => new(options);
}

/// <summary>
/// The <c>processing</c> of a document input (python-genai <c>DocumentProcessing</c>): either a <see cref="Mode"/>
/// string (see <see cref="InteractionMediaProcessingModes"/>) or an
/// <see cref="InteractionDocumentStaticMediaProcessing"/> object.
/// </summary>
[JsonConverter(typeof(InteractionDocumentProcessingConverter))]
public sealed class InteractionDocumentProcessing
{
    /// <summary>Creates a processing mode (e.g. <c>"static"</c>).</summary>
    /// <param name="mode">The mode.</param>
    public InteractionDocumentProcessing(string mode)
    {
        Mode = mode;
    }

    /// <summary>Creates processing options.</summary>
    /// <param name="options">The options.</param>
    public InteractionDocumentProcessing(InteractionDocumentStaticMediaProcessing options)
    {
        Options = options;
    }

    /// <summary>The processing mode, when the processing is given as a string.</summary>
    public string? Mode { get; }

    /// <summary>The processing options, when the processing is given as an object.</summary>
    public InteractionDocumentStaticMediaProcessing? Options { get; }

    /// <summary>Converts a mode string to a processing value.</summary>
    /// <param name="mode">The mode.</param>
    public static implicit operator InteractionDocumentProcessing(string mode) => new(mode);

    /// <summary>Converts processing options to a processing value.</summary>
    /// <param name="options">The options.</param>
    public static implicit operator InteractionDocumentProcessing(InteractionDocumentStaticMediaProcessing options) => new(options);

    /// <summary>Creates a processing mode value.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The processing value.</returns>
    public static InteractionDocumentProcessing FromString(string mode) => new(mode);

    /// <summary>Creates a processing value from options.</summary>
    /// <param name="options">The options.</param>
    /// <returns>The processing value.</returns>
    public static InteractionDocumentProcessing FromInteractionDocumentStaticMediaProcessing(
        InteractionDocumentStaticMediaProcessing options) => new(options);
}
