using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Chunk from context retrieved by the retrieval tools.
/// </summary>
public class GroundingChunkRetrievedContext
{
    /// <summary>
    /// Output only. The full document name for the referenced Vertex AI Search document.
    /// </summary>
    [JsonPropertyName("documentName")]
    public string? DocumentName { get; set; }

    /// <summary>
    /// Additional context for the RAG retrieval result. This is only populated when using the RAG retrieval tool.
    /// </summary>
    [JsonPropertyName("ragChunk")]
    public RagChunk? RagChunk { get; set; }

    /// <summary>
    /// Text of the attribution.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Title of the attribution.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// URI reference of the attribution.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}
