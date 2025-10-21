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
}
