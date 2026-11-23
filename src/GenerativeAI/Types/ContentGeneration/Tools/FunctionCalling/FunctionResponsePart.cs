using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A datatype containing media that is part of a FunctionResponse message.
/// A FunctionResponsePart consists of data which has an associated datatype.
/// A FunctionResponsePart can only contain one of the accepted types in FunctionResponsePart.data.
/// A FunctionResponsePart must have a fixed IANA MIME type identifying the type and subtype of the media
/// if the inline_data field is filled with raw bytes.
/// </summary>
public class FunctionResponsePart
{
    /// <summary>
    /// Optional. Inline media bytes.
    /// </summary>
    [JsonPropertyName("inlineData")]
    public FunctionResponseBlob? InlineData { get; set; }

    /// <summary>
    /// Optional. URI based data.
    /// </summary>
    [JsonPropertyName("fileData")]
    public FunctionResponseFileData? FileData { get; set; }

    /// <summary>
    /// Optional. How the model processes the media in this part for understanding. It applies only to video parts and
    /// other parts ignore it. This field is not supported in Vertex AI.
    /// </summary>
    [JsonPropertyName("mediaProcessing")]
    public MediaProcessing? MediaProcessing { get; set; }

    /// <summary>
    /// Metadata describing the audio content supplied in inline data or file data, used to configure audio
    /// preprocessing.
    /// </summary>
    [JsonPropertyName("audioMetadata")]
    public AudioMetadata? AudioMetadata { get; set; }

    /// <summary>
    /// Metadata describing the document content supplied in inline data or file data, used to configure document
    /// preprocessing.
    /// </summary>
    [JsonPropertyName("documentMetadata")]
    public DocumentMetadata? DocumentMetadata { get; set; }

    /// <summary>
    /// Media resolution to use for the input media in this part.
    /// </summary>
    [JsonPropertyName("mediaResolution")]
    public PartMediaResolution? MediaResolution { get; set; }

    /// <summary>
    /// Metadata describing the spreadsheet content supplied in inline data or file data, used to configure
    /// spreadsheet preprocessing.
    /// </summary>
    [JsonPropertyName("spreadsheetMetadata")]
    public SpreadsheetMetadata? SpreadsheetMetadata { get; set; }

    /// <summary>
    /// Metadata describing the video content supplied in inline data or file data, such as the start and end offsets
    /// and the frame rate.
    /// </summary>
    [JsonPropertyName("videoMetadata")]
    public VideoMetadataResult? VideoMetadata { get; set; }
}
