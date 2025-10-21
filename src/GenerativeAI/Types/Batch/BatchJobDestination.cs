using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for the output data destination of a batch job.
/// </summary>
public class BatchJobDestination
{
    /// <summary>
    /// Storage format of the output files. Must be one of: 'jsonl', 'bigquery'.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// The Google Cloud Storage URI to the output file.
    /// The output will be written as JSONL with one response per line.
    /// </summary>
    [JsonPropertyName("gcsUri")]
    public string? GcsUri { get; set; }

    /// <summary>
    /// The BigQuery URI to the output table.
    /// Format: bq://projectId.datasetId.tableId
    /// </summary>
    [JsonPropertyName("bigqueryUri")]
    public string? BigqueryUri { get; set; }

    /// <summary>
    /// The Gemini API's file resource name of the output data.
    /// Format: files/{file-id}
    /// The file will be a JSONL file with a single response per line.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    /// <summary>
    /// The responses to the requests in the batch. Returned when the batch was
    /// built using inlined requests. The responses will be in the same order as
    /// the input requests.
    /// </summary>
    [JsonPropertyName("inlinedResponses")]
    public List<InlinedResponse>? InlinedResponses { get; set; }

    /// <summary>
    /// The embedding responses to the requests in the batch. Returned when the batch was
    /// built using inlined embedding requests. The responses will be in the same order as
    /// the input requests.
    /// </summary>
    [JsonPropertyName("inlinedEmbedContentResponses")]
    public List<InlinedEmbedContentResponse>? InlinedEmbedContentResponses { get; set; }
}
