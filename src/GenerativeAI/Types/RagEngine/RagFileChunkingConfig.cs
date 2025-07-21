using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the size and overlap of chunks for RagFiles.
/// </summary>
public class RagFileChunkingConfig
{
    /// <summary>
    /// The overlap between chunks.
    /// </summary>
    [JsonPropertyName("chunkOverlap")]
    [System.Obsolete("Use FixedLengthChunking property instead. ChunkOverlap property will be removed in a future version.")]
    public int? ChunkOverlap { get; set; }

    /// <summary>
    /// The size of the chunks.
    /// </summary>
    [JsonPropertyName("chunkSize")]
    [System.Obsolete("Use FixedLengthChunking property instead. ChunkSize property will be removed in a future version.")]
    public int? ChunkSize { get; set; }

    /// <summary>
    /// Specifies the fixed length chunking config.
    /// </summary>
    [JsonPropertyName("fixedLengthChunking")]
    public RagFileChunkingConfigFixedLengthChunking? FixedLengthChunking { get; set; }
}