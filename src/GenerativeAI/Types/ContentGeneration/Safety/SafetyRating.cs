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
}