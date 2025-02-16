using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// URI based data.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#FileData">See Official API Documentation</seealso> 
public class FileData
{
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