using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// URI based data for function response.
/// </summary>
public class FunctionResponseFileData
{
    /// <summary>
    /// Required. URI.
    /// </summary>
    [JsonPropertyName("fileUri")]
    public string? FileUri { get; set; }

    /// <summary>
    /// Required. The IANA standard MIME type of the source data.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Optional. A display name for the file data, used as a label or filename. It is returned for prompt management
    /// and in some server-side tool calls.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
