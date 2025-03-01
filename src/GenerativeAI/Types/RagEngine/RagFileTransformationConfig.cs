using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the transformation config for RagFiles.
/// </summary>
public class RagFileTransformationConfig
{
    /// <summary>
    /// Specifies the chunking config for RagFiles.
    /// </summary>
    [JsonPropertyName("ragFileChunkingConfig")]
    public RagFileChunkingConfig? RagFileChunkingConfig { get; set; }
}