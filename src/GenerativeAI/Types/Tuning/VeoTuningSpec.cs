using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning specification for Veo video generation model tuning.
/// </summary>
public class VeoTuningSpec
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
    /// Optional. Hyperparameters for Veo tuning.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public VeoHyperParameters? HyperParameters { get; set; }
}
