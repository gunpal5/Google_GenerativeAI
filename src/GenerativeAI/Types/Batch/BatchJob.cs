using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a batch processing job for generating content or embeddings at scale.
/// Batch jobs allow processing large numbers of requests asynchronously.
/// </summary>
public class BatchJob
{
    /// <summary>
    /// The resource name of the BatchJob. Output only.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The display name of the BatchJob.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// The state of the BatchJob.
    /// </summary>
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JobStateConverter))]
    public JobState? State { get; set; }

    /// <summary>
    /// Output only. Only populated when the job's state is JOB_STATE_FAILED or JOB_STATE_CANCELLED.
    /// </summary>
    [JsonPropertyName("error")]
    public JobError? Error { get; set; }

    /// <summary>
    /// The time when the BatchJob was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// Output only. Time when the Job for the first time entered the JOB_STATE_RUNNING state.
    /// </summary>
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    /// <summary>
    /// The time when the BatchJob was completed.
    /// </summary>
    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    /// <summary>
    /// The time when the BatchJob was last updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    /// <summary>
    /// The name of the model that produces the predictions via the BatchJob.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Configuration for the input data source.
    /// </summary>
    [JsonPropertyName("src")]
    public BatchJobSource? Source { get; set; }

    /// <summary>
    /// Configuration for the output data destination.
    /// </summary>
    [JsonPropertyName("dest")]
    public BatchJobDestination? Destination { get; set; }

    /// <summary>
    /// The type identifier for the metadata object (Google AI only).
    /// </summary>
    [JsonPropertyName("@type")]
    public string? Type { get; set; }

    /// <summary>
    /// Statistics about the batch job (Google AI only).
    /// </summary>
    [JsonPropertyName("batchStats")]
    public BatchStats? BatchStats { get; set; }

    /// <summary>
    /// Output only. Information about where the output is written (Vertex AI only).
    /// Only populated when the job's state is JOB_STATE_SUCCEEDED.
    /// </summary>
    [JsonPropertyName("outputInfo")]
    public BatchOutputInfo? OutputInfo { get; set; }

    /// <summary>
    /// Input configuration for the batch job (Vertex AI only).
    /// </summary>
    [JsonPropertyName("inputConfig")]
    public VertexInputConfig? InputConfig { get; set; }

    /// <summary>
    /// Output configuration for the batch job (Vertex AI only).
    /// </summary>
    [JsonPropertyName("outputConfig")]
    public VertexOutputConfig? OutputConfig { get; set; }
}

/// <summary>
/// Information about the output of a batch prediction job (Vertex AI only).
/// </summary>
public class BatchOutputInfo
{
    /// <summary>
    /// Output only. The full path of the Cloud Storage directory created, into which the prediction output is written.
    /// </summary>
    [JsonPropertyName("gcsOutputDirectory")]
    public string? GcsOutputDirectory { get; set; }

    /// <summary>
    /// Output only. The path of the BigQuery dataset created, in bq://projectId.bqDatasetId format,
    /// into which the prediction output is written.
    /// </summary>
    [JsonPropertyName("bigqueryOutputDataset")]
    public string? BigqueryOutputDataset { get; set; }

    /// <summary>
    /// Output only. The name of the BigQuery table created, in predictions_&lt;timestamp&gt; format,
    /// into which the prediction output is written.
    /// </summary>
    [JsonPropertyName("bigqueryOutputTable")]
    public string? BigqueryOutputTable { get; set; }
}
