using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A response candidate generated from the model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/Candidate">See Official API Documentation</seealso> 
public class Candidate
{
    /// <summary>
    /// Output only. Generated content returned from the model.
    /// </summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    /// <summary>
    /// Optional. Output only. The reason why the model stopped generating tokens.
    /// If empty, the model has not stopped generating tokens.
    /// </summary>
    [JsonPropertyName("finishReason")]
    public FinishReason? FinishReason { get; set; }

    /// <summary>
    /// List of ratings for the safety of a response candidate.
    /// There is at most one rating per category.
    /// </summary>
    [JsonPropertyName("safetyRatings")]
    public List<SafetyRating>? SafetyRatings { get; set; }

    /// <summary>
    /// Output only. Citation information for model-generated candidate.
    /// This field may be populated with recitation information for any text included in the
    /// <see cref="Content"/>. These are passages that are "recited" from copyrighted
    /// material in the foundational LLM's training data.
    /// </summary>
    [JsonPropertyName("citationMetadata")]
    public CitationMetadata? CitationMetadata { get; set; }

    /// <summary>
    /// Output only. Token count for this candidate.
    /// </summary>
    [JsonPropertyName("tokenCount")]
    public int? TokenCount { get; set; }

    /// <summary>
    /// Output only. Attribution information for sources that contributed to a grounded answer.
    /// This field is populated for <c>GenerateAnswer</c> calls.
    /// </summary>
    [JsonPropertyName("groundingAttributions")]
    public List<GroundingAttribution>? GroundingAttributions { get; set; }

    /// <summary>
    /// Output only. Grounding metadata for the candidate.
    /// This field is populated for <c>GenerateContent</c> calls.
    /// </summary>
    [JsonPropertyName("groundingMetadata")]
    public GroundingMetadata? GroundingMetadata { get; set; }

    /// <summary>
    /// Output only. Average log probability score of the candidate.
    /// </summary>
    [JsonPropertyName("avgLogprobs")]
    public double? AvgLogprobs { get; set; }

    /// <summary>
    /// Output only. Log-likelihood scores for the response tokens and top tokens
    /// </summary>
    [JsonPropertyName("logprobsResult")]
    public LogprobsResult? LogprobsResult { get; set; }

    /// <summary>
    /// Output only. Index of the candidate in the list of response candidates.
    /// </summary>
    [JsonPropertyName("index")]
    public int? Index { get; set; }
}