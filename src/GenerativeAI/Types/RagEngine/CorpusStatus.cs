using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// RagCorpus status.
/// </summary>
public class CorpusStatus
{
    /// <summary>
    /// Output only. Only when the `state` field is ERROR.
    /// </summary>
    [JsonPropertyName("errorStatus")]
    public string? ErrorStatus { get; set; } 

    /// <summary>
    /// Output only. RagCorpus life state.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CorpusStatusState? State { get; set; } 
}