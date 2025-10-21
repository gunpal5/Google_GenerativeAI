using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Safety rating for a piece of content.
/// The safety rating contains the category of harm and the harm probability level in that category
/// for a piece of content. Content is classified for safety across a number of harm categories
/// and the probability of the harm classification is included here.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#safetyrating">See Official API Documentation</seealso> 
public class SafetyRating
{
    /// <summary>
    /// Required. The category for this rating.
    /// </summary>
    [JsonPropertyName("category")]
    public HarmCategory Category { get; set; }

    /// <summary>
    /// Required. The probability of harm for this content.
    /// </summary>
    [JsonPropertyName("probability")]
    public HarmProbability Probability { get; set; }

    /// <summary>
    /// Was this content blocked because of this rating?
    /// </summary>
    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    /// <summary>
    /// Output only. The overwritten threshold for the safety category of Gemini 2.0 image out.
    /// If minors are detected in the output image, the threshold of each safety category will be
    /// overwritten if user sets a lower threshold.
    /// </summary>
    [JsonPropertyName("overwrittenThreshold")]
    public HarmBlockThreshold? OverwrittenThreshold { get; set; }

    /// <summary>
    /// Output only. Harm probability score.
    /// </summary>
    [JsonPropertyName("probabilityScore")]
    public float? ProbabilityScore { get; set; }

    /// <summary>
    /// Output only. Harm severity levels in the content.
    /// </summary>
    [JsonPropertyName("severity")]
    public HarmSeverity? Severity { get; set; }

    /// <summary>
    /// Output only. Harm severity score.
    /// </summary>
    [JsonPropertyName("severityScore")]
    public float? SeverityScore { get; set; }
}