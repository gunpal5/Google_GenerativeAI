using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Candidate for the logprobs token and score.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#Candidate">See Official API Documentation</seealso> 
public class LogprobsCandidate // Renamed from Candidate
{
    /// <summary>
    /// The candidate's token string value.
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    /// <summary>
    /// The candidate's token id value.
    /// </summary>
    [JsonPropertyName("tokenId")]
    public int? TokenId { get; set; }

    /// <summary>
    /// The candidate's log probability.
    /// </summary>
    [JsonPropertyName("logProbability")]
    public double? LogProbability { get; set; }
}