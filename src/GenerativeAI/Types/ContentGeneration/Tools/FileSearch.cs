using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tool that retrieves relevant content from File Search stores (managed retrieval-augmented generation).
/// </summary>
public class FileSearch
{
    /// <summary>
    /// Names of the File Search stores to search, in the form <c>fileSearchStores/{id}</c>.
    /// </summary>
    [JsonPropertyName("fileSearchStoreNames")]
    public List<string>? FileSearchStoreNames { get; set; }

    /// <summary>
    /// The number of chunks to retrieve.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    /// <summary>
    /// Metadata filter (AIP-160 syntax) applied to documents before retrieval.
    /// </summary>
    [JsonPropertyName("metadataFilter")]
    public string? MetadataFilter { get; set; }
}
