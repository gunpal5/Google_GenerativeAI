using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Dataset bucket used to create a histogram for the distribution given a population of values.
/// </summary>
public class SupervisedTuningDatasetDistributionDatasetBucket
{
    /// <summary>
    /// Output only. Number of values in the bucket.
    /// </summary>
    [JsonPropertyName("count")]
    public double? Count { get; set; }

    /// <summary>
    /// Output only. Left bound of the bucket (inclusive).
    /// </summary>
    [JsonPropertyName("left")]
    public double? Left { get; set; }

    /// <summary>
    /// Output only. Right bound of the bucket (exclusive).
    /// </summary>
    [JsonPropertyName("right")]
    public double? Right { get; set; }
}
