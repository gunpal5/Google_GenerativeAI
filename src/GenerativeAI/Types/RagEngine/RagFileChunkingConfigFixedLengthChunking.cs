using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the fixed length chunking config.
/// </summary>
public class RagFileChunkingConfigFixedLengthChunking
{
    /// <summary>
    /// The overlap between chunks.
    /// </summary>
    [JsonPropertyName("chunkOverlap")]
    public int? ChunkOverlap { get; set; }

    /// <summary>
    /// The size of the chunks.
    /// </summary>
    [JsonPropertyName("chunkSize")]
    public int? ChunkSize { get; set; }
}