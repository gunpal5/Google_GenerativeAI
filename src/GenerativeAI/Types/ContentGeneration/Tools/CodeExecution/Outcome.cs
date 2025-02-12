using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Enumeration of possible outcomes of the code execution.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/Outcome">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Outcome
{
    /// <summary>
    /// Unspecified status. This value should not be used.
    /// </summary>
    OUTCOME_UNSPECIFIED = 0,

    /// <summary>
    /// Code execution completed successfully.
    /// </summary>
    OUTCOME_OK = 1,

    /// <summary>
    /// Code execution finished but with a failure. <c>stderr</c> should contain the reason.
    /// </summary>
    OUTCOME_FAILED = 2,

    /// <summary>
    /// Code execution ran for too long, and was cancelled. There may or may not be a partial output present.
    /// </summary>
    OUTCOME_DEADLINE_EXCEEDED = 3,
}