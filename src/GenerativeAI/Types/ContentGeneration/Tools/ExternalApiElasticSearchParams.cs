using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The search parameters to use for the ELASTIC_SEARCH spec.
/// </summary>
public class ExternalApiElasticSearchParams
{
    /// <summary>
    /// The ElasticSearch index to use.
    /// </summary>
    [JsonPropertyName("index")]
    public string? Index { get; set; }

    /// <summary>
    /// Optional. Number of hits (chunks) to request. When specified, it is passed to Elasticsearch as the `num_hits` param.
    /// </summary>
    [JsonPropertyName("numHits")]
    public int? NumHits { get; set; }

    /// <summary>
    /// The ElasticSearch search template to use.
    /// </summary>
    [JsonPropertyName("searchTemplate")]
    public string? SearchTemplate { get; set; }
}
