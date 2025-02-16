using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A repeated list of passages.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#GroundingPassages">See Official API Documentation</seealso>
public class GroundingPassages
{
    /// <summary>
    /// List of passages.
    /// </summary>
    [JsonPropertyName("passages")]
    public List<GroundingPassage> Passages { get; set; } = new List<GroundingPassage>();
}