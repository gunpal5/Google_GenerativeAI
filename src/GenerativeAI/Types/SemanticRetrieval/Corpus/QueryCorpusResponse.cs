using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the response from querying a corpus.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#response-body_1">See Official API Documentation</seealso>
public class QueryCorpusResponse
{
    /// <summary>
    /// Gets or sets the list of relevant chunks.
    /// </summary>
    [JsonPropertyName("relevantChunks")]
    public List<RelevantChunk>? RelevantChunks { get; set; }
}