using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Identifier for a <see cref="Chunk"/> retrieved via Semantic Retriever specified in the
/// <see cref="GenerateAnswerRequest"/> using <see cref="SemanticRetrieverConfig"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#SemanticRetrieverChunk">See Official API Documentation</seealso>
public class SemanticRetrieverChunk
{
    /// <summary>
    /// Output only. Name of the source matching the request's <see cref="SemanticRetriever">SemanticRetrieverConfig.source</see>.
    /// Example: <c>corpora/123</c> or <c>corpora/123/documents/abc</c>
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Output only. Name of the <see cref="Chunk">Chunk</see> containing the attributed text.
    /// Example: <c>corpora/123/documents/abc/chunks/xyz</c>
    /// </summary>
    [JsonPropertyName("chunk")]
    public string? Chunk { get; set; }
}