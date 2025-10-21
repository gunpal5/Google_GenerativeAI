using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A RagChunk includes the content of a chunk of a RagFile, and associated metadata.
/// </summary>
public class RagChunk
{
    /// <summary>
    /// If populated, represents where the chunk starts and ends in the document.
    /// </summary>
    [JsonPropertyName("pageSpan")]
    public object? PageSpan { get; set; }

    /// <summary>
    /// The content of the chunk.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
