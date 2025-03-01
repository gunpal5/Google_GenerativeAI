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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FileStatusState? State { get; set; } 
}