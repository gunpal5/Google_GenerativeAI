using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for the input data source of a batch job.
/// </summary>
public class BatchJobSource
{
    /// <summary>
    /// Storage format of the input files. Must be one of: 'jsonl', 'bigquery'.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// The Google Cloud Storage URIs to input files.
    /// May contain multiple files (e.g., "gs://my-bucket/input*.jsonl").
    /// </summary>
    [JsonPropertyName("gcsUri")]
    public List<string>? GcsUri { get; set; }

    /// <summary>
    /// The BigQuery URI to input table.
    /// Format: bq://projectId.datasetId.tableId
    /// </summary>
    [JsonPropertyName("bigqueryUri")]
    public string? BigqueryUri { get; set; }

    /// <summary>
    /// The Gemini API's file resource name of the input data.
    /// Format: files/{file-id}
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    /// <summary>
    /// The Gemini API's inlined input data to run batch job.
    /// Used for batch jobs that don't require external storage.
    /// </summary>
    [JsonPropertyName("inlinedRequests")]
    public List<InlinedRequest>? InlinedRequests { get; set; }
}
