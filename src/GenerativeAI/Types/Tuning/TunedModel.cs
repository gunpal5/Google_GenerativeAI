using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// TunedModel for the Tuned Model of a Tuning Job.
/// Represents the output model produced by a tuning job.
/// </summary>
public class TunedModel
{
    /// <summary>
    /// Output only. The resource name of the TunedModel.
    /// Format: `projects/{project}/locations/{location}/models/{model}@{version_id}`
    /// When tuning from a base model, the version_id will be 1.
    /// For continuous tuning, the version id will be incremented by 1 from the
    /// last version id in the parent model.
    /// E.g., `projects/{project}/locations/{location}/models/{model}@{last_version_id + 1}`
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Output only. A resource name of an Endpoint.
    /// Format: `projects/{project}/locations/{location}/endpoints/{endpoint}`.
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// The checkpoints associated with this TunedModel.
    /// This field is only populated for tuning jobs that enable intermediate checkpoints.
    /// </summary>
    [JsonPropertyName("checkpoints")]
    public List<TunedModelCheckpoint>? Checkpoints { get; set; }
}
