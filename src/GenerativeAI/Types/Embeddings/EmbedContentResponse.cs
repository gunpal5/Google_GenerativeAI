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
}