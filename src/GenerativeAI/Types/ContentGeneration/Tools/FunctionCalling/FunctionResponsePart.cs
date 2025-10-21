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
}
