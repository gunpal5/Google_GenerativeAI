using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Job state enumeration representing the lifecycle states of a job.
/// Supports both BATCH_STATE_* (Google AI) and JOB_STATE_* (Vertex AI) formats.
/// </summary>
[JsonConverter(typeof(JobStateConverter))]
public enum JobState
{
    /// <summary>
    /// The job state is unspecified.
    /// </summary>
    JOB_STATE_UNSPECIFIED = 0,

    /// <summary>
    /// The job has been just created or resumed and processing has not yet begun.
    /// </summary>
    JOB_STATE_QUEUED = 1,

    /// <summary>
    /// The service is preparing to run the job.
    /// </summary>
    JOB_STATE_PENDING = 2,

    /// <summary>
    /// The job is in progress.
    /// </summary>
    JOB_STATE_RUNNING = 3,

    /// <summary>
    /// The job completed successfully.
    /// </summary>
    JOB_STATE_SUCCEEDED = 4,

    /// <summary>
    /// The job failed.
    /// </summary>
    JOB_STATE_FAILED = 5,

    /// <summary>
    /// The job is being cancelled. From this state the job may only go to either
    /// JOB_STATE_SUCCEEDED, JOB_STATE_FAILED or JOB_STATE_CANCELLED.
    /// </summary>
    JOB_STATE_CANCELLING = 6,

    /// <summary>
    /// The job has been cancelled.
    /// </summary>
    JOB_STATE_CANCELLED = 7,

    /// <summary>
    /// The job has been stopped, and can be resumed.
    /// </summary>
    JOB_STATE_PAUSED = 8,

    /// <summary>
    /// The job has expired.
    /// </summary>
    JOB_STATE_EXPIRED = 9,

    /// <summary>
    /// The job is being updated. Only jobs in the JOB_STATE_RUNNING state can be updated.
    /// After updating, the job goes back to the JOB_STATE_RUNNING state.
    /// </summary>
    JOB_STATE_UPDATING = 10,

    /// <summary>
    /// The job is partially succeeded, some results may be missing due to errors.
    /// </summary>
    JOB_STATE_PARTIALLY_SUCCEEDED = 11
}
