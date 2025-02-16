using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The information for a chunk relevant to a query.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#relevantchunk">See Official API Documentation</seealso>
public class RelevantChunk
{
    /// <summary>
    /// <c>Chunk</c> relevance to the query.
    /// </summary>
    [JsonPropertyName("chunkRelevanceScore")]
    public double? ChunkRelevanceScore { get; set; }

    /// <summary>
    /// <c>Chunk</c> associated with the query.
    /// </summary>
    [JsonPropertyName("chunk")]
    public Chunk? Chunk { get; set; }
}