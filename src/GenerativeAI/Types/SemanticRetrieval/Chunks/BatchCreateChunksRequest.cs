using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to batch create chunks.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#method:-corpora.documents.chunks.batchcreate">See Official API Documentation</seealso>
public class BatchCreateChunksRequest
{
    /// <summary>
    /// Gets or sets the requests messages specifying the <see cref="CreateChunkRequest"/>s to create.
    /// A maximum of 100 <see cref="CreateChunkRequest"/>s can be created in a batch.
    /// </summary>
    [JsonPropertyName("requests")]
    public List<CreateChunkRequest>? Requests { get; set; }
}