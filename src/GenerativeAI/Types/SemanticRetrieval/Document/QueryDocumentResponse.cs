using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from <c>documents.query</c> containing a list of relevant chunks.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#response-body_1">See Official API Documentation</seealso>
public class QueryDocumentResponse
{
    /// <summary>
    /// The returned relevant chunks.
    /// </summary>
    [JsonPropertyName("relevantChunks")]
    public List<RelevantChunk>? RelevantChunks { get; set; }
}