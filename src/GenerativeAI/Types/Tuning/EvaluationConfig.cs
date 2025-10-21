using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Evaluation configuration for model tuning.
/// Defines how the tuned model should be evaluated during and after training.
/// </summary>
public class EvaluationConfig
{
    /// <summary>
    /// The metrics used for evaluation.
    /// </summary>
    [JsonPropertyName("metrics")]
    public List<Metric>? Metrics { get; set; }

    /// <summary>
    /// Config for evaluation output.
    /// </summary>
    [JsonPropertyName("outputConfig")]
    public OutputConfig? OutputConfig { get; set; }

    /// <summary>
    /// Autorater config for evaluation.
    /// </summary>
    [JsonPropertyName("autoraterConfig")]
    public AutoraterConfig? AutoraterConfig { get; set; }
}
