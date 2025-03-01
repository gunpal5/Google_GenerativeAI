using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The BigQuery location for the output content.
/// </summary>
public class BigQueryDestination
{
    /// <summary>
    /// Required. BigQuery URI to a project or table, up to 2000 characters long. When only the project is specified, the Dataset and Table is created. When the full table reference is specified, the Dataset must exist and table must not exist. Accepted forms: * BigQuery path. For example: `bq://projectId` or `bq://projectId.bqDatasetId` or `bq://projectId.bqDatasetId.bqTableId`.
    /// </summary>
    [JsonPropertyName("outputUri")]
    public string? OutputUri { get; set; } 
}