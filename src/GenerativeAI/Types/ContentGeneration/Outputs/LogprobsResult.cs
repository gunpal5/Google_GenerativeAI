using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Logprobs Result.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#LogprobsResult">See Official API Documentation</seealso> 
public class LogprobsResult
{
    /// <summary>
    /// Length = total number of decoding steps.
    /// </summary>
    [JsonPropertyName("topCandidates")]
    public List<TopCandidates>? TopCandidates { get; set; }

    /// <summary>
    /// Length = total number of decoding steps. The chosen candidates may or may not be in TopCandidates.
    /// </summary>
    [JsonPropertyName("chosenCandidates")]
    public List<LogprobsCandidate>? ChosenCandidates { get; set; }
}