using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning specification for open-source and third-party Partner models.
/// </summary>
public class PartnerModelTuningSpec
{
    /// <summary>
    /// Required. Cloud Storage path to file containing training dataset for tuning.
    /// The dataset must be formatted as a JSONL file.
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// Optional. Cloud Storage path to file containing validation dataset for tuning.
    /// The dataset must be formatted as a JSONL file.
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Optional. Hyperparameters for tuning.
    /// The accepted hyperparameters and their valid range of values will differ depending on the base model.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public JsonObject? HyperParameters { get; set; }
}
