using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Hyperparameters for Veo model tuning.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
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
    /// Obsolete. The API sends the Veo tuning task as an enum string; use <see cref="TuningTaskType"/> instead.
    /// </summary>
    [JsonIgnore]
    [Obsolete("The Veo tuning task is an enum on the wire. Use TuningTaskType instead.")]
    public TuningTask? TuningTask { get; set; }

    /// <summary>
    /// Optional. The tuning task. Either I2V (Image-to-Video), T2V (Text-to-Video) or R2V (Reference-to-Video).
    /// </summary>
    [JsonPropertyName("tuningTask")]
    public VeoTuningTask? TuningTaskType { get; set; }

    /// <summary>
    /// Optional. The ratio of Google internal dataset to use in the training mixture, in range of <c>[0, 1)</c>.
    /// </summary>
    [JsonPropertyName("veoDataMixtureRatio")]
    public double? VeoDataMixtureRatio { get; set; }

    /// <summary>
    /// Optional. The adapter size for LoRA tuning.
    /// </summary>
    [JsonPropertyName("adapterSize")]
    public AdapterSize? AdapterSize { get; set; }

    /// <summary>
    /// The speed of the tuning job. Only supported for Veo 3.0 models.
    /// </summary>
    [JsonPropertyName("tuningSpeed")]
    public TuningSpeed? TuningSpeed { get; set; }
}
