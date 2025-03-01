using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The BigQuery location for the input content.
/// </summary>
public class BigQuerySource
{
    /// <summary>
    /// Required. BigQuery URI to a table, up to 2000 characters long. Accepted forms: * BigQuery path. For example: `bq://projectId.bqDatasetId.bqTableId`.
    /// </summary>
    [JsonPropertyName("inputUri")]
    public string? InputUri { get; set; } 
}