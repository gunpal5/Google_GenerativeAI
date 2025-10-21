using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Raw media bytes for function response.
/// Text should not be sent as raw bytes, use the FunctionResponse.response field.
/// </summary>
public class FunctionResponseBlob
{
    /// <summary>
    /// Required. The IANA standard MIME type of the source data.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Required. Inline media bytes.
    /// A base64-encoded string.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
