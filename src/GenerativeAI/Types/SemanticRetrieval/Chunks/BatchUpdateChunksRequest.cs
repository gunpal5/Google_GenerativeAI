using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the request body for batch updating chunks.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#request-body_6">See Official API Documentation</seealso>
public class BatchUpdateChunksRequest
{
    /// <summary>
    /// Gets or sets the list of update requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public List<UpdateChunkRequest>? Requests { get; set; }
}