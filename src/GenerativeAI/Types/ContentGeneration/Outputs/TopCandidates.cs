using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Candidates with top log probabilities at each decoding step.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#TopCandidates">See Official API Documentation</seealso> 
public class TopCandidates
{
    /// <summary>
    /// Sorted by log probability in descending order.
    /// </summary>
    [JsonPropertyName("candidates")]
    public List<LogprobsCandidate>? Candidates { get; set; } 
}