using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Feedback related to the input data used to answer the question, as opposed to the
/// model-generated response to the question.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#InputFeedback">See Official API Documentation</seealso> 
public class InputFeedback
{
    /// <summary>
    /// Ratings for safety of the input. There is at most one rating per category.
    /// </summary>
    [JsonPropertyName("safetyRatings")]
    public List<SafetyRating>? SafetyRatings { get; set; }

    /// <summary>
    /// Optional. If set, the input was blocked and no candidates are returned. Rephrase the input.
    /// </summary>
    [JsonPropertyName("blockReason")]
    public BlockReason? BlockReason { get; set; }
}
