using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// RagFile status.
/// </summary>
public class FileStatus
{
    /// <summary>
    /// Output only. Only when the `state` field is ERROR.
    /// </summary>
    [JsonPropertyName("errorStatus")]
    public string? ErrorStatus { get; set; } 

    /// <summary>
    /// Output only. RagFile state.
    /// </summary>
    [JsonPropertyName("state")]
   
    public FileStatusState? State { get; set; } 

    /// <summary>
    /// The status code. 0 means success and 1 means the operation was cancelled.
    /// </summary>
    [JsonPropertyName("code")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? Code { get; set; }

    /// <summary>
    /// A list of messages that carry the error details, using a common set of message types defined for APIs.
    /// </summary>
    [JsonPropertyName("details")]
    public List<Dictionary<string, JsonElement>>? Details { get; set; }

    /// <summary>
    /// A human-readable description of the error.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}