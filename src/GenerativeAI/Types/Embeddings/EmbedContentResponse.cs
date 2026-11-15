using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the response to an <see cref="EmbedContentRequest"/>.
/// </summary>
/// <seealso href="https://cloud.google.com/your-api-documentation#response-body">See Official API Documentation</seealso>
public class EmbedContentResponse
{
    /// <summary>
    /// Gets or sets the embedding generated from the input content.
    /// </summary>
    [JsonPropertyName("embedding")]
    public ContentEmbedding? Embedding { get; set; }

    /// <summary>
    /// Request-level metadata returned only on Gemini Enterprise Agent Platform (Vertex AI).
    /// </summary>
    [JsonPropertyName("metadata")]
    public EmbedContentMetadata? Metadata { get; set; }

    /// <summary>
    /// Whether the input content was truncated before the embedding was generated. This field is not supported in the
    /// Gemini API.
    /// </summary>
    [JsonPropertyName("truncated")]
    public bool? Truncated { get; set; }

    /// <summary>
    /// Token usage information for the embedding request.
    /// </summary>
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata? UsageMetadata { get; set; }
}