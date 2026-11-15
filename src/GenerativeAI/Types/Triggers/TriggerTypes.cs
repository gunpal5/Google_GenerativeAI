using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A trigger (<c>/triggers</c>): runs an agent interaction on a cron schedule.
/// </summary>
public sealed class Trigger
{
    /// <summary>The trigger ID.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The display name.</summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>The cron schedule (e.g. <c>0 9 * * 1-5</c>).</summary>
    [JsonPropertyName("schedule")]
    public string? Schedule { get; set; }

    /// <summary>The IANA time zone of <see cref="Schedule"/> (e.g. <c>America/New_York</c>).</summary>
    [JsonPropertyName("time_zone")]
    public string? TimeZone { get; set; }

    /// <summary>The agent interaction created on each run.</summary>
    [JsonPropertyName("interaction")]
    public CreateInteractionRequest? Interaction { get; set; }

    /// <summary>The environment the runs use.</summary>
    [JsonPropertyName("environment_id")]
    public string? EnvironmentId { get; set; }

    /// <summary>The timeout of each run, in seconds.</summary>
    [JsonPropertyName("execution_timeout_seconds")]
    public int? ExecutionTimeoutSeconds { get; set; }

    /// <summary>The number of consecutive failures after which the trigger is put in the error state.</summary>
    [JsonPropertyName("max_consecutive_failures")]
    public int? MaxConsecutiveFailures { get; set; }

    /// <summary>The current number of consecutive failures.</summary>
    [JsonPropertyName("consecutive_failure_count")]
    public int? ConsecutiveFailureCount { get; set; }

    /// <summary>The trigger status (see <see cref="TriggerStatuses"/>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>The ID of the interaction created by the previous run.</summary>
    [JsonPropertyName("previous_interaction_id")]
    public string? PreviousInteractionId { get; set; }

    /// <summary>Creation time (RFC 3339).</summary>
    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    /// <summary>Last update time (RFC 3339).</summary>
    [JsonPropertyName("update_time")]
    public string? UpdateTime { get; set; }

    /// <summary>Time of the last run (RFC 3339).</summary>
    [JsonPropertyName("last_run_time")]
    public string? LastRunTime { get; set; }

    /// <summary>Time of the next scheduled run (RFC 3339).</summary>
    [JsonPropertyName("next_run_time")]
    public string? NextRunTime { get; set; }

    /// <summary>Time the trigger was last paused (RFC 3339).</summary>
    [JsonPropertyName("last_pause_time")]
    public string? LastPauseTime { get; set; }

    /// <summary>Time the trigger was last resumed (RFC 3339).</summary>
    [JsonPropertyName("last_resume_time")]
    public string? LastResumeTime { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Request body of <c>POST /triggers</c>.</summary>
public sealed class CreateTriggerRequest
{
    /// <summary>Required. The agent interaction to create on each run (set <see cref="CreateInteractionRequest.Agent"/>).</summary>
    [JsonPropertyName("interaction")]
    public CreateInteractionRequest? Interaction { get; set; }

    /// <summary>Required. The cron schedule.</summary>
    [JsonPropertyName("schedule")]
    public string? Schedule { get; set; }

    /// <summary>Required. The IANA time zone of the schedule.</summary>
    [JsonPropertyName("time_zone")]
    public string? TimeZone { get; set; }

    /// <summary>The display name.</summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>The environment the runs use.</summary>
    [JsonPropertyName("environment_id")]
    public string? EnvironmentId { get; set; }

    /// <summary>The timeout of each run, in seconds.</summary>
    [JsonPropertyName("execution_timeout_seconds")]
    public int? ExecutionTimeoutSeconds { get; set; }

    /// <summary>The number of consecutive failures after which the trigger is put in the error state.</summary>
    [JsonPropertyName("max_consecutive_failures")]
    public int? MaxConsecutiveFailures { get; set; }

    /// <summary>Additional body fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Request body of <c>PATCH /triggers/{id}</c>.</summary>
public sealed class UpdateTriggerRequest
{
    /// <summary>The new display name.</summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>The new status, e.g. <see cref="TriggerStatuses.Paused"/> to pause or <see cref="TriggerStatuses.Active"/> to resume.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>A run of a <see cref="Trigger"/>.</summary>
public sealed class TriggerExecution
{
    /// <summary>The execution ID.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The trigger ID.</summary>
    [JsonPropertyName("trigger_id")]
    public string? TriggerId { get; set; }

    /// <summary>The execution status (see <see cref="TriggerExecutionStatuses"/>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>The ID of the interaction created by this run.</summary>
    [JsonPropertyName("interaction_id")]
    public string? InteractionId { get; set; }

    /// <summary>The environment used by this run.</summary>
    [JsonPropertyName("environment_id")]
    public string? EnvironmentId { get; set; }

    /// <summary>The error message of a failed run.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>The scheduled time (RFC 3339).</summary>
    [JsonPropertyName("scheduled_time")]
    public string? ScheduledTime { get; set; }

    /// <summary>The start time (RFC 3339).</summary>
    [JsonPropertyName("start_time")]
    public string? StartTime { get; set; }

    /// <summary>The end time (RFC 3339).</summary>
    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Response of <c>GET /triggers</c>.</summary>
public sealed class ListTriggersResponse
{
    /// <summary>The triggers.</summary>
    [JsonPropertyName("triggers")]
    public List<Trigger>? Triggers { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>Response of <c>GET /triggers/{id}/executions</c>.</summary>
public sealed class ListTriggerExecutionsResponse
{
    /// <summary>The executions.</summary>
    [JsonPropertyName("trigger_executions")]
    public List<TriggerExecution>? TriggerExecutions { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>Known trigger statuses.</summary>
public static class TriggerStatuses
{
    /// <summary>The trigger runs on schedule.</summary>
    public const string Active = "active";

    /// <summary>The trigger is paused.</summary>
    public const string Paused = "paused";

    /// <summary>The trigger stopped after too many consecutive failures.</summary>
    public const string Error = "error";
}

/// <summary>Known trigger execution statuses.</summary>
public static class TriggerExecutionStatuses
{
    /// <summary>The run is in progress.</summary>
    public const string InProgress = "in_progress";

    /// <summary>The run completed.</summary>
    public const string Completed = "completed";

    /// <summary>The run failed.</summary>
    public const string Failed = "failed";

    /// <summary>The run was skipped.</summary>
    public const string Skipped = "skipped";

    /// <summary>The run timed out.</summary>
    public const string TimedOut = "timed_out";

    /// <summary>
    /// Returns whether <paramref name="status"/> is a terminal status (anything but <see cref="InProgress"/>).
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns><c>true</c> if the run has finished.</returns>
    public static bool IsTerminal(string? status) => !string.IsNullOrEmpty(status) && status != InProgress;
}
