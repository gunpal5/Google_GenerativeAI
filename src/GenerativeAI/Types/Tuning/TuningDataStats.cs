using System.Text.Json.Nodes;
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

    /// <summary>
    /// Output only. Statistics for the distillation prompt dataset (raw JSON).
    /// </summary>
    [JsonPropertyName("distillationDataStats")]
    public JsonObject? DistillationDataStats { get; set; }

    /// <summary>
    /// Output only. Statistics for preference optimization (raw JSON).
    /// </summary>
    [JsonPropertyName("preferenceOptimizationDataStats")]
    public JsonObject? PreferenceOptimizationDataStats { get; set; }

    /// <summary>
    /// Output only. Statistics for reinforcement tuning (raw JSON).
    /// </summary>
    [JsonPropertyName("reinforcementTuningDataStats")]
    public JsonObject? ReinforcementTuningDataStats { get; set; }
}
