using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the response from a batch create chunks operation.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#request-body_5">See Official API Documentation</seealso>
public class BatchCreateChunksResponse
{
    /// <summary>
    /// Gets or sets the list of <see cref="Chunk"/>s that were created.
    /// </summary>
    [JsonPropertyName("chunks")]
    public List<Chunk>? Chunks { get; set; }
}