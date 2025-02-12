using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A set of the feedback metadata the prompt specified in <see cref="GenerateContentRequest.Contents"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#PromptFeedback">See Official API Documentation</seealso>
public class PromptFeedback
{
    /// <summary>
    /// Optional. If set, the prompt was blocked and no candidates are returned. Rephrase the prompt.
    /// </summary>
    [JsonPropertyName("blockReason")]
    public BlockReason? BlockReason { get; set; }

    /// <summary>
    /// Ratings for safety of the prompt. There is at most one rating per category.
    /// </summary>
    [JsonPropertyName("safetyRatings")]
    public List<SafetyRating>? SafetyRatings { get; set; }
}