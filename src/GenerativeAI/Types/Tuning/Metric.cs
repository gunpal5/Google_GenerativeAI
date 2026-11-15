using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The metric used for evaluation during model tuning.
/// </summary>
/// <remarks>
/// Set the convenience properties (<see cref="Name"/>, <see cref="PromptTemplate"/>,
/// <see cref="JudgeModelSystemInstruction"/>, <see cref="ReturnRawOutput"/>) and
/// <see cref="GenerativeAI.Clients.TuningClient"/> converts them to the wire format
/// (<see cref="ExactMatchSpec"/>, <see cref="BleuSpec"/>, <see cref="RougeSpec"/> or
/// <see cref="PointwiseMetricSpec"/> plus <see cref="AggregationMetrics"/>) the same way the
/// google-genai SDK does. The wire-format properties can also be set directly.
/// </remarks>
public class Metric
{
    /// <summary>
    /// The name of the metric (for example <c>exact_match</c>, <c>bleu</c>, <c>rouge_l_sum</c> or a custom name
    /// used together with <see cref="PromptTemplate"/>).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The prompt template for the metric.
    /// </summary>
    [JsonPropertyName("promptTemplate")]
    public string? PromptTemplate { get; set; }

    /// <summary>
    /// The system instruction for the judge model.
    /// </summary>
    [JsonPropertyName("judgeModelSystemInstruction")]
    public string? JudgeModelSystemInstruction { get; set; }

    /// <summary>
    /// Whether to return the raw output from the judge model.
    /// </summary>
    [JsonPropertyName("returnRawOutput")]
    public bool? ReturnRawOutput { get; set; }

    /// <summary>
    /// Wire format. The aggregation metrics to compute (for example <c>AVERAGE</c>, <c>STANDARD_DEVIATION</c>).
    /// </summary>
    [JsonPropertyName("aggregationMetrics")]
    public List<string>? AggregationMetrics { get; set; }

    /// <summary>
    /// Wire format. Spec for the exact match metric.
    /// </summary>
    [JsonPropertyName("exactMatchSpec")]
    public JsonObject? ExactMatchSpec { get; set; }

    /// <summary>
    /// Wire format. Spec for the BLEU metric.
    /// </summary>
    [JsonPropertyName("bleuSpec")]
    public JsonObject? BleuSpec { get; set; }

    /// <summary>
    /// Wire format. Spec for the ROUGE metric (for example <c>{"rougeType": "rougeLSum"}</c>).
    /// </summary>
    [JsonPropertyName("rougeSpec")]
    public JsonObject? RougeSpec { get; set; }

    /// <summary>
    /// Wire format. Spec for a pointwise (model-based) metric.
    /// </summary>
    [JsonPropertyName("pointwiseMetricSpec")]
    public JsonObject? PointwiseMetricSpec { get; set; }
}
