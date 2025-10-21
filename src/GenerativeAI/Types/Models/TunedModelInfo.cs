using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Information about the tuned model from the base model.
/// </summary>
public class TunedModelInfo
{
    /// <summary>
    /// ID of the base model that you want to tune.
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string? BaseModel { get; set; }

    /// <summary>
    /// Date and time when the base model was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public DateTime? CreateTime { get; set; }

    /// <summary>
    /// Date and time when the base model was last updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public DateTime? UpdateTime { get; set; }
}
