namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to batch delete chunks.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#request-body_7">See Official API Documentation</seealso>
public class BatchDeleteChunksRequest:List<DeleteChunkRequest>
{
    /// <summary>
    /// Represents a request to batch delete multiple chunks.
    /// The class is a specialized list of <see cref="DeleteChunkRequest"/> objects and is used to encapsulate multiple delete requests within a single API call.
    /// </summary>
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