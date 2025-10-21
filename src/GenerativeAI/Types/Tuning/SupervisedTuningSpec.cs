using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning specification for Supervised Fine-Tuning for first-party models.
/// </summary>
public class SupervisedTuningSpec
{
    /// <summary>
    /// Required. Training dataset used for tuning.
    /// The dataset can be specified as either a Cloud Storage path to a JSONL file
    /// or as the resource name of a Vertex Multimodal Dataset.
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// Optional. Validation dataset used for tuning.
    /// The dataset can be specified as either a Cloud Storage path to a JSONL file
    /// or as the resource name of a Vertex Multimodal Dataset.
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Optional. Hyperparameters for SFT.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public SupervisedHyperParameters? HyperParameters { get; set; }

    /// <summary>
    /// Optional. Tuning mode (FULL or PEFT_ADAPTER).
    /// </summary>
    [JsonPropertyName("tuningMode")]
    public TuningMode? TuningMode { get; set; }

    /// <summary>
    /// Optional. If set to true, disable intermediate checkpoints for SFT and only the last checkpoint will be exported.
    /// Otherwise, enable intermediate checkpoints for SFT. Default is false.
    /// </summary>
    [JsonPropertyName("exportLastCheckpointOnly")]
    public bool? ExportLastCheckpointOnly { get; set; }
}
