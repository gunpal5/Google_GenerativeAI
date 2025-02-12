using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Raw media bytes.
/// Text should not be sent as raw bytes, use the 'text' field.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Blob">See Official API Documentation</seealso> 
public class Blob
{
    /// <summary>
    /// The IANA standard MIME type of the source data. Examples:
    /// - image/png
    /// - image/jpeg
    /// If an unsupported MIME type is provided, an error will be returned.
    /// For a complete list of supported types, see
    /// <see href="https://ai.google.dev/gemini-api/docs/prompting_with_media#supported_file_formats">Supported file formats</see>.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Raw bytes for media formats.
    /// A base64-encoded string.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
