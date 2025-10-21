using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Hyperparameters for Veo model tuning.
/// </summary>
public class VeoHyperParameters
{
    /// <summary>
    /// Optional. Number of complete passes the model makes over the entire training dataset during training.
    /// </summary>
    [JsonPropertyName("epochCount")]
    public int? EpochCount { get; set; }

    /// <summary>
    /// Optional. Multiplier for adjusting the default learning rate.
    /// </summary>
    [JsonPropertyName("learningRateMultiplier")]
    public double? LearningRateMultiplier { get; set; }

    /// <summary>
    /// Optional. The tuning task. Either I2V (Image-to-Video) or T2V (Text-to-Video).
    /// </summary>
    [JsonPropertyName("tuningTask")]
    public TuningTask? TuningTask { get; set; }
}
