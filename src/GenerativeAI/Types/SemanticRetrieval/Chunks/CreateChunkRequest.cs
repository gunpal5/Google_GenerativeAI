using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to create a <see cref="Chunk"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#CreateChunkRequest">See Official API Documentation</seealso>
public class CreateChunkRequest
{
    /// <summary>
    /// Required. The name of the <see cref="Document"/> where this <see cref="Chunk"/> will be created.
    /// Example: <c>corpora/my-corpus-123/documents/the-doc-abc</c>.
    /// </summary>
    [JsonPropertyName("parent")]
    public string Parent { get; set; }

    /// <summary>
    /// Required. The <see cref="Chunk"/> to create.
    /// </summary>
    [JsonPropertyName("chunk")]
    public Chunk Chunk { get; set; }
}