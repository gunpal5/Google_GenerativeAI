using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the response to a <see cref="BatchEmbedContentRequest"/>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/embeddings#response-body_1">See Official API Documentation</seealso>
public class BatchEmbedContentsResponse
{
    /// <summary>
    /// Gets or sets the embeddings for each request, in the same order as provided in the batch request.
    /// </summary>
    [JsonPropertyName("embeddings")]
    public List<ContentEmbedding>? Embeddings { get; set; }

    /// <summary>
    /// Token usage information for the whole batch, such as the number of prompt tokens embedded.
    /// Returned by Gemini embedding models on Gemini Enterprise Agent Platform (Vertex AI).
    /// </summary>
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata? UsageMetadata { get; set; }
}