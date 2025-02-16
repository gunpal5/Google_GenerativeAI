using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to delete a specific chunk.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#DeleteChunkRequest">See Official API Documentation</seealso>
public class DeleteChunkRequest
{
    /// <summary>
    /// Gets or sets the name of the chunk to delete.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}