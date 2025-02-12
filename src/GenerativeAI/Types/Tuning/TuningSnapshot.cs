using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Record for a single tuning step.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tuning#TuningSnapshot">See Official API Documentation</seealso>
public class TuningSnapshot
{
    /// <summary>
    /// Output only. The tuning step.
    /// </summary>
    [JsonPropertyName("step")]
    public int? Step { get; set; }

    /// <summary>
    /// Output only. The epoch this step was part of.
    /// </summary>
    [JsonPropertyName("epoch")]
    public int? Epoch { get; set; }

    /// <summary>
    /// Output only. The mean loss of the training examples for this step.
    /// </summary>
    [JsonPropertyName("meanLoss")]
    public double? MeanLoss { get; set; }

    /// <summary>
    /// Output only. The timestamp when this metric was computed.
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits. Offsets other than "Z" are also accepted. Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("computeTime")]
    public string? ComputeTime { get; set; }
}