using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The tuning data statistics associated with the tuning job.
/// </summary>
public class TuningDataStats
{
    /// <summary>
    /// Output only. Statistics for supervised tuning.
    /// </summary>
    [JsonPropertyName("supervisedTuningDataStats")]
    public SupervisedTuningDataStats? SupervisedTuningDataStats { get; set; }
}
