using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response for a single embedding content request.
/// </summary>
public class SingleEmbedContentResponse
{
    /// <summary>
    /// The embedding generated from the content.
    /// </summary>
    [JsonPropertyName("embedding")]
    public ContentEmbedding? Embedding { get; set; }

    /// <summary>
    /// The number of tokens in the input content.
    /// </summary>
    [JsonPropertyName("tokenCount")]
    public int? TokenCount { get; set; }
}
