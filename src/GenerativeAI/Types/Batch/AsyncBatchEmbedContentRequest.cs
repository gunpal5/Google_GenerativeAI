using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request for batch embedding generation.
/// </summary>
/// <seealso href="https://ai.google.dev/api/batch#method:-models.asyncbatchembedcontent">See Official API Documentation</seealso>
public class AsyncBatchEmbedContentRequest
{
    /// <summary>
    /// Required. The source configuration specifying input data for embeddings.
    /// </summary>
    [JsonPropertyName("src")]
    public BatchJobSource Src { get; set; } = null!;

    /// <summary>
    /// Optional. The destination configuration for embedding output.
    /// </summary>
    [JsonPropertyName("dest")]
    public BatchJobDestination? Dest { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncBatchEmbedContentRequest"/> class.
    /// </summary>
    public AsyncBatchEmbedContentRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncBatchEmbedContentRequest"/> class with specified source and destination.
    /// </summary>
    /// <param name="src">The source configuration.</param>
    /// <param name="dest">The optional destination configuration.</param>
    public AsyncBatchEmbedContentRequest(BatchJobSource src, BatchJobDestination? dest = null)
    {
        Src = src;
        Dest = dest;
    }
}
