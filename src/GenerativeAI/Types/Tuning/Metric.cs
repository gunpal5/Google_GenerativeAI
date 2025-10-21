using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The metric used for evaluation during model tuning.
/// </summary>
public class Metric
{
    /// <summary>
    /// The name of the metric.
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
}
