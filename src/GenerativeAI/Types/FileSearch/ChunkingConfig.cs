using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tells the service how to split a file into chunks when adding it to a <see cref="FileSearchStore"/>.
/// </summary>
public class ChunkingConfig
{
    /// <summary>
    /// White space chunking configuration.
    /// </summary>
    [JsonPropertyName("whiteSpaceConfig")]
    public WhiteSpaceConfig? WhiteSpaceConfig { get; set; }
}

/// <summary>
/// Configuration for the white space chunking algorithm.
/// </summary>
public class WhiteSpaceConfig
{
    /// <summary>
    /// Maximum number of tokens per chunk.
    /// </summary>
    [JsonPropertyName("maxTokensPerChunk")]
    public int? MaxTokensPerChunk { get; set; }

    /// <summary>
    /// Maximum number of overlapping tokens between two adjacent chunks.
    /// </summary>
    [JsonPropertyName("maxOverlapTokens")]
    public int? MaxOverlapTokens { get; set; }
}
