using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// A context of the query.
/// </summary>
public class RagContextsContext
{
    /// <summary>
    /// The distance between the query dense embedding vector and the context text vector.
    /// </summary>
    [JsonPropertyName("distance")]
    [System.Obsolete]
    public double? Distance { get; set; }

    /// <summary>
    /// According to the underlying Vector DB and the selected metric type, the score can be either the distance or the similarity between the query and the context and its range depends on the metric type. For example, if the metric type is COSINE_DISTANCE, it represents the distance between the query and the context. The larger the distance, the less relevant the context is to the query. The range is [0, 2], while 0 means the most relevant and 2 means the least relevant.
    /// </summary>
    [JsonPropertyName("score")]
    public double? Score { get; set; }

    /// <summary>
    /// The file display name.
    /// </summary>
    [JsonPropertyName("sourceDisplayName")]
    public string? SourceDisplayName { get; set; }

    /// <summary>
    /// If the file is imported from Cloud Storage or Google Drive, source_uri will be original file URI in Cloud Storage or Google Drive; if file is uploaded, source_uri will be file display name.
    /// </summary>
    [JsonPropertyName("sourceUri")]
    public string? SourceUri { get; set; }

    /// <summary>
    /// The distance between the query sparse embedding vector and the context text vector.
    /// </summary>
    [JsonPropertyName("sparseDistance")]
    [System.Obsolete]
    public double? SparseDistance { get; set; }

    /// <summary>
    /// The text chunk.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

   

}