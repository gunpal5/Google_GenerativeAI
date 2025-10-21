using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// TunedModelCheckpoint for the Tuned Model of a Tuning Job.
/// Represents a specific checkpoint during the tuning process.
/// </summary>
public class TunedModelCheckpoint
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

    /// <summary>
    /// The Endpoint resource name that the checkpoint is deployed to.
    /// Format: `projects/{project}/locations/{location}/endpoints/{endpoint}`.
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }
}
