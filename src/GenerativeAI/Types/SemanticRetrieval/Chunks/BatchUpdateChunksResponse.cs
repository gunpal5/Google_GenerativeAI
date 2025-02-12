using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the response body for batch updating chunks.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#response-body_6">See Official API Documentation</seealso>
public class BatchUpdateChunksResponse
{
    /// <summary>
    /// Gets or sets the list of updated chunks.
    /// </summary>
    [JsonPropertyName("chunks")]
    public List<Chunk>? Chunks { get; set; }
}