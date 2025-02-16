using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for retrieving grounding content from a <see cref="Corpus"/> or <see cref="Document"/>
/// created using the Semantic Retriever API.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/question-answering#SemanticRetrieverConfig">See Official API Documentation</seealso>
public class SemanticRetrieverConfig
{
    /// <summary>
    /// Required. Name of the resource for retrieval.
    /// Example: <c>corpora/123</c> or <c>corpora/123/documents/abc</c>.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = "";

    /// <summary>
    /// Required. Query to use for matching <c>Chunk</c>s in the given resource by similarity.
    /// </summary>
    [JsonPropertyName("query")]
    public Content Query { get; set; }

    /// <summary>
    /// Optional. Filters for selecting <c>Document</c>s and/or <c>Chunk</c>s from the resource.
    /// </summary>
    [JsonPropertyName("metadataFilters")]
    public List<MetadataFilter>? MetadataFilters { get; set; }

    /// <summary>
    /// Optional. Maximum number of relevant <c>Chunk</c>s to retrieve.
    /// </summary>
    [JsonPropertyName("maxChunksCount")]
    public int? MaxChunksCount { get; set; }

    /// <summary>
    /// Optional. Minimum relevance score for retrieved relevant <c>Chunk</c>s.
    /// </summary>
    [JsonPropertyName("minimumRelevanceScore")]
    public double? MinimumRelevanceScore { get; set; }
}