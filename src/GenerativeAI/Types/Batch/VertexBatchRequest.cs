using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request for batch job creation (Vertex AI format).
/// </summary>
public class VertexBatchRequest
{
    /// <summary>
    /// The model resource name to use for the batch job.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Display name for the batch job.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Input configuration for the batch job.
    /// </summary>
    [JsonPropertyName("inputConfig")]
    public VertexInputConfig? InputConfig { get; set; }

    /// <summary>
    /// Output configuration for the batch job.
    /// </summary>
    [JsonPropertyName("outputConfig")]
    public VertexOutputConfig? OutputConfig { get; set; }
}

/// <summary>
/// Input configuration for Vertex AI batch jobs.
/// </summary>
public class VertexInputConfig
{
    /// <summary>
    /// Instances format (e.g., "jsonl", "bigquery").
    /// </summary>
    [JsonPropertyName("instancesFormat")]
    public string? InstancesFormat { get; set; }

    /// <summary>
    /// GCS source configuration.
    /// </summary>
    [JsonPropertyName("gcsSource")]
    public BatchGcsSource? GcsSource { get; set; }

    /// <summary>
    /// BigQuery source configuration.
    /// </summary>
    [JsonPropertyName("bigquerySource")]
    public BatchBigQuerySource? BigQuerySource { get; set; }
}

/// <summary>
/// Output configuration for Vertex AI batch jobs.
/// </summary>
public class VertexOutputConfig
{
    /// <summary>
    /// Predictions format (e.g., "jsonl", "bigquery").
    /// </summary>
    [JsonPropertyName("predictionsFormat")]
    public string? PredictionsFormat { get; set; }

    /// <summary>
    /// GCS destination configuration.
    /// </summary>
    [JsonPropertyName("gcsDestination")]
    public BatchGcsDestination? GcsDestination { get; set; }

    /// <summary>
    /// BigQuery destination configuration.
    /// </summary>
    [JsonPropertyName("bigqueryDestination")]
    public BatchBigQueryDestination? BigQueryDestination { get; set; }
}

/// <summary>
/// GCS source for batch input.
/// </summary>
public class BatchGcsSource
{
    /// <summary>
    /// GCS URIs for input files.
    /// </summary>
    [JsonPropertyName("uris")]
    public List<string>? Uris { get; set; }
}

/// <summary>
/// BigQuery source for batch input.
/// </summary>
public class BatchBigQuerySource
{
    /// <summary>
    /// BigQuery input URI.
    /// </summary>
    [JsonPropertyName("inputUri")]
    public string? InputUri { get; set; }
}

/// <summary>
/// GCS destination for batch output.
/// </summary>
public class BatchGcsDestination
{
    /// <summary>
    /// GCS output URI prefix.
    /// </summary>
    [JsonPropertyName("outputUriPrefix")]
    public string? OutputUriPrefix { get; set; }
}

/// <summary>
/// BigQuery destination for batch output.
/// </summary>
public class BatchBigQueryDestination
{
    /// <summary>
    /// BigQuery output URI.
    /// </summary>
    [JsonPropertyName("outputUri")]
    public string? OutputUri { get; set; }
}
