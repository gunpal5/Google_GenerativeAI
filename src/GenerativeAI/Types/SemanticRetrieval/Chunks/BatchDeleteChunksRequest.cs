namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to batch delete chunks.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#request-body_7">See Official API Documentation</seealso>
public class BatchDeleteChunksRequest:List<DeleteChunkRequest>
{
    public BatchDeleteChunksRequest(IEnumerable<DeleteChunkRequest> requests)
    {
        this.AddRange(requests);
    }
    // /// <summary>
    // /// Gets or sets the request messages specifying the <see cref="DeleteChunkRequest"/>s to delete.
    // /// </summary>
    // [JsonPropertyName("requests")]
    // public List<DeleteChunkRequest>? Requests { get; set; }
}