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
    [System.Obsolete]
    public int? ChunkOverlap { get; set; }

    /// <summary>
    /// The size of the chunks.
    /// </summary>
    [JsonPropertyName("chunkSize")]
    [System.Obsolete]
    public int? ChunkSize { get; set; }

    /// <summary>
    /// Specifies the fixed length chunking config.
    /// </summary>
    [JsonPropertyName("fixedLengthChunking")]
    public RagFileChunkingConfigFixedLengthChunking? FixedLengthChunking { get; set; }
}