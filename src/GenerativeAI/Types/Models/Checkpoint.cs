using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Describes the machine learning model version checkpoint.
/// </summary>
public class Checkpoint
{
    /// <summary>
    /// The ID of the checkpoint.
    /// </summary>
    [JsonPropertyName("checkpointId")]
    public string? CheckpointId { get; set; }

    /// <summary>
    /// The epoch of the checkpoint.
    /// </summary>
    [JsonPropertyName("epoch")]
    public int? Epoch { get; set; }

    /// <summary>
    /// The step of the checkpoint.
    /// </summary>
    [JsonPropertyName("step")]
    public int? Step { get; set; }
}
