using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning tasks that create tuned models.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tuning#TuningTask">See Official API Documentation</seealso>
public class TuningTask
{
    /// <summary>
    /// Output only. The timestamp when tuning this model started.
    ///
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits.
    /// Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("startTime")]
    public Timestamp? StartTime { get; set; }

    /// <summary>
    /// Output only. The timestamp when tuning this model completed.
    ///
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits.
    /// Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("completeTime")]
    public Timestamp? CompleteTime { get; set; }

    /// <summary>
    /// Output only. Metrics collected during tuning.
    /// </summary>
    [JsonPropertyName("snapshots")]
    public TuningSnapshot[]? Snapshots { get; set; }

    /// <summary>
    /// Required. Input only. Immutable. The model training data.
    /// </summary>
    [JsonPropertyName("trainingData")]
    public Dataset? TrainingData { get; set; }

    /// <summary>
    /// Immutable. Hyperparameters controlling the tuning process. If not provided, default values will be used.
    /// </summary>
    [JsonPropertyName("hyperparameters")]
    public Hyperparameters? Hyperparameters { get; set; }
}