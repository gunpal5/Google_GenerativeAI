using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Autorater config used for model evaluation during tuning.
/// </summary>
public class AutoraterConfig
{
    /// <summary>
    /// Number of samples for each instance in the dataset.
    /// If not specified, the default is 4. Minimum value is 1, maximum value is 32.
    /// </summary>
    [JsonPropertyName("samplingCount")]
    public int? SamplingCount { get; set; }

    /// <summary>
    /// Optional. Default is true. Whether to flip the candidate and baseline responses.
    /// This is only applicable to the pairwise metric. If enabled, also provide
    /// PairwiseMetricSpec.candidate_response_field_name and PairwiseMetricSpec.baseline_response_field_name.
    /// When rendering PairwiseMetricSpec.metric_prompt_template, the candidate and baseline
    /// fields will be flipped for half of the samples to reduce bias.
    /// </summary>
    [JsonPropertyName("flipEnabled")]
    public bool? FlipEnabled { get; set; }

    /// <summary>
    /// The fully qualified name of the publisher model or tuned autorater endpoint to use.
    /// Publisher model format: `projects/{project}/locations/{location}/publishers/{publisher}/models/{model}`
    /// Tuned model format: `projects/{project}/locations/{location}/models/{model}`
    /// </summary>
    [JsonPropertyName("autoraterModel")]
    public string? AutoraterModel { get; set; }
}
