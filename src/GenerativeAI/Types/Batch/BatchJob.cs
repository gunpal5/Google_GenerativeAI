using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a batch processing job for generating content or embeddings at scale.
/// Batch jobs allow processing large numbers of requests asynchronously.
/// </summary>
public class BatchJob
{
    /// <summary>
    /// Output only. The resource name of the BatchJob.
    /// Format: projects/{project}/locations/{location}/batchPredictionJobs/{batch_prediction_job}
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The display name of the BatchJob.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Output only. The state of the BatchJob.
    /// </summary>
    [JsonPropertyName("state")]
    public JobState? State { get; set; }

    /// <summary>
    /// Output only. Only populated when the job's state is JOB_STATE_FAILED or JOB_STATE_CANCELLED.
    /// </summary>
    [JsonPropertyName("error")]
    public JobError? Error { get; set; }

    /// <summary>
    /// Output only. The time when the BatchJob was created.
    /// A timestamp in RFC3339 UTC "Zulu" format, with nanosecond resolution and up to nine fractional digits.
    /// Examples: "2014-10-02T15:01:23Z" and "2014-10-02T15:01:23.045123456Z".
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. Time when the BatchJob for the first time entered the JOB_STATE_RUNNING state.
    /// A timestamp in RFC3339 UTC "Zulu" format, with nanosecond resolution and up to nine fractional digits.
    /// </summary>
    [JsonPropertyName("startTime")]
    public Timestamp? StartTime { get; set; }

    /// <summary>
    /// Output only. The time when the BatchJob was completed (succeeded, failed, or cancelled).
    /// A timestamp in RFC3339 UTC "Zulu" format, with nanosecond resolution and up to nine fractional digits.
    /// </summary>
    [JsonPropertyName("endTime")]
    public Timestamp? EndTime { get; set; }

    /// <summary>
    /// Output only. The time when the BatchJob was last updated.
    /// A timestamp in RFC3339 UTC "Zulu" format, with nanosecond resolution and up to nine fractional digits.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// The name of the model that produces the predictions via the BatchJob.
    /// Format: models/{model}
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
}
