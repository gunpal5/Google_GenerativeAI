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

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateChunkRequest"/> class with the specified parent and chunk.
    /// </summary>
    /// <param name="parent">The name of the document where this chunk will be created.</param>
    /// <param name="chunk">The chunk to create.</param>
    public CreateChunkRequest(string parent, Chunk chunk)
    {
        Parent = parent;
        Chunk = chunk;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateChunkRequest"/> class for JSON deserialization.
    /// </summary>
    public CreateChunkRequest() : this("", new Chunk()) { }
}