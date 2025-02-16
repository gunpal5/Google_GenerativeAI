using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Passage included inline with a grounding configuration.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#GroundingPassage">See Official API Documentation</seealso>
public class GroundingPassage
{
    /// <summary>
    /// Identifier for the passage for attributing this passage in grounded answers.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Content of the passage.
    /// </summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }
}