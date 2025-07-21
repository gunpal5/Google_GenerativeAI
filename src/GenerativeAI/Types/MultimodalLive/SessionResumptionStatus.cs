using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the status of a session resumption attempt.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SessionResumptionStatus>))]
public enum SessionResumptionStatus
{
    /// <summary>
    /// The status is unspecified.
    /// </summary>
    UNSPECIFIED,

    /// <summary>
    /// Session resumption was successful.
    /// </summary>
    SUCCESS,

    /// <summary>
    /// Session resumption failed.
    /// </summary>
    FAILED,

    /// <summary>
    /// Session resumption is in progress.
    /// </summary>
    IN_PROGRESS
}