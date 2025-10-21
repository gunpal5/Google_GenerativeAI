using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// URI based data.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#FileData">See Official API Documentation</seealso>
public class FileData
{
    /// <summary>
    /// Optional. Display name of the file data. Used to provide a label or filename to distinguish file datas.
    /// It is not currently used in the Gemini GenerateContent calls.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional. The IANA standard MIME type of the source data.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Required. URI.
    /// </summary>
    [JsonPropertyName("fileUri")]
    public string FileUri { get; set; } = "";
}