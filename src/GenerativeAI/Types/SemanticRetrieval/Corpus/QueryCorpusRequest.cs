using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a request to query a corpus.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#request-body_1">See Official API Documentation</seealso>
public class QueryCorpusRequest
{
    /// <summary>
    /// Gets or sets the query string to perform semantic search.
    /// </summary>
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the list of metadata filters.
    /// </summary>
    [JsonPropertyName("metadataFilters")]
    public List<MetadataFilter>? MetadataFilters { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of <see cref="RelevantChunk"/>s to return.
    /// </summary>
    [JsonPropertyName("resultsCount")]
    public int? ResultsCount { get; set; }
}