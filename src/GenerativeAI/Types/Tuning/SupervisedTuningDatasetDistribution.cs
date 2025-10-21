using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Dataset distribution for Supervised Tuning.
/// Provides statistical analysis of the training dataset.
/// </summary>
public class SupervisedTuningDatasetDistribution
{
    /// <summary>
    /// Output only. Sum of a given population of values that are billable.
    /// </summary>
    [JsonPropertyName("billableSum")]
    public long? BillableSum { get; set; }

    /// <summary>
    /// Output only. Defines the histogram buckets for the distribution.
    /// </summary>
    [JsonPropertyName("buckets")]
    public List<SupervisedTuningDatasetDistributionDatasetBucket>? Buckets { get; set; }

    /// <summary>
    /// Output only. The minimum of the population values.
    /// </summary>
    [JsonPropertyName("min")]
    public double? Min { get; set; }

    /// <summary>
    /// Output only. The maximum of the population values.
    /// </summary>
    [JsonPropertyName("max")]
    public double? Max { get; set; }

    /// <summary>
    /// Output only. The arithmetic mean of the values in the population.
    /// </summary>
    [JsonPropertyName("mean")]
    public double? Mean { get; set; }

    /// <summary>
    /// Output only. The median of the values in the population.
    /// </summary>
    [JsonPropertyName("median")]
    public double? Median { get; set; }

    /// <summary>
    /// Output only. The 5th percentile of the values in the population.
    /// </summary>
    [JsonPropertyName("p5")]
    public double? P5 { get; set; }

    /// <summary>
    /// Output only. The 95th percentile of the values in the population.
    /// </summary>
    [JsonPropertyName("p95")]
    public double? P95 { get; set; }

    /// <summary>
    /// Output only. Sum of a given population of values.
    /// </summary>
    [JsonPropertyName("sum")]
    public long? Sum { get; set; }
}
