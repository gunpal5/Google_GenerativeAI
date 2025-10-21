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
}
