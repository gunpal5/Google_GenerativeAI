using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to update a chunk.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#UpdateChunkRequest">See Official API Documentation</seealso>
public class UpdateChunkRequest
{
    /// <summary>
    /// Gets or sets the chunk to update.
    /// </summary>
    [JsonPropertyName("chunk")]
    public Chunk? Chunk { get; set; }

    /// <summary>
    /// Gets or sets the update mask.
    /// </summary>
    [JsonPropertyName("updateMask")]
    public string? UpdateMask { get; set; }
}