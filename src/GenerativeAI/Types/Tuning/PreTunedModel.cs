using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A pre-tuned model for continuous tuning.
/// Used as a base for further fine-tuning iterations.
/// </summary>
public class PreTunedModel
{
    /// <summary>
    /// Output only. The name of the base model this PreTunedModel was tuned from.
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string? BaseModel { get; set; }

    /// <summary>
    /// Optional. The source checkpoint id. If not specified, the default checkpoint will be used.
    /// </summary>
    [JsonPropertyName("checkpointId")]
    public string? CheckpointId { get; set; }

    /// <summary>
    /// The resource name of the Model.
    /// Format: projects/{project}/locations/{location}/models/{model}@{version_id}
    /// or projects/{project}/locations/{location}/models/{model}@{alias}
    /// or omit the version id to use the default version: projects/{project}/locations/{location}/models/{model}
    /// </summary>
    [JsonPropertyName("tunedModelName")]
    public string? TunedModelName { get; set; }
}
