using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Statistics of the input text associated with the result of content embedding.
/// </summary>
public class ContentEmbeddingStatistics
{
    /// <summary>
    /// Vertex API only. If the input text was truncated due to having a length longer than the allowed maximum input.
    /// </summary>
    [JsonPropertyName("truncated")]
    public bool? Truncated { get; set; }

    /// <summary>
    /// Vertex API only. Number of tokens of the input text.
    /// </summary>
    [JsonPropertyName("tokenCount")]
    public double? TokenCount { get; set; }
}
