using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Hyperparameters for Supervised Fine-Tuning (SFT).
/// </summary>
public class SupervisedHyperParameters
{
    /// <summary>
    /// Optional. Adapter size for tuning.
    /// Used when tuning in PEFT adapter mode.
    /// </summary>
    [JsonPropertyName("adapterSize")]
    public AdapterSize? AdapterSize { get; set; }

    /// <summary>
    /// Optional. Batch size for tuning.
    /// This feature is only available for open source models.
    /// </summary>
    [JsonPropertyName("batchSize")]
    public int? BatchSize { get; set; }

    /// <summary>
    /// Optional. Number of complete passes the model makes over the entire training dataset during training.
    /// </summary>
    [JsonPropertyName("epochCount")]
    public int? EpochCount { get; set; }

    /// <summary>
    /// Optional. Learning rate for tuning.
    /// Mutually exclusive with learningRateMultiplier.
    /// This feature is only available for open source models.
    /// </summary>
    [JsonPropertyName("learningRate")]
    public double? LearningRate { get; set; }

    /// <summary>
    /// Optional. Multiplier for adjusting the default learning rate.
    /// Mutually exclusive with learningRate.
    /// This feature is only available for first-party models.
    /// </summary>
    [JsonPropertyName("learningRateMultiplier")]
    public double? LearningRateMultiplier { get; set; }
}
