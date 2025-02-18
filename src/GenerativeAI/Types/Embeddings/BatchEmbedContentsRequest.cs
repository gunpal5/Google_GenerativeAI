using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a batched request for embedding content.
/// </summary>
/// <seealso href="https://ai.google.dev/api/embeddings#method:-models.batchembedcontents">See Official API Documentation</seealso>
public class BatchEmbedContentRequest
{
    /// <summary>
    /// Required. Embed requests for the batch. The model in each of these requests must match the model specified in <see cref="BatchEmbedContentRequest"/>.
    /// </summary>
    [JsonPropertyName("requests")]
    public List<EmbedContentRequest>? Requests { get; set; }
}