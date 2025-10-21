using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Specifies how the response should be scheduled in the conversation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FunctionResponseScheduling>))]
public enum FunctionResponseScheduling
{
    /// <summary>
    /// This value is unused.
    /// </summary>
    SCHEDULING_UNSPECIFIED = 0,

    /// <summary>
    /// Only add the result to the conversation context, do not interrupt or trigger generation.
    /// </summary>
    SILENT = 1,

    /// <summary>
    /// Add the result to the conversation context, and prompt to generate output without interrupting ongoing generation.
    /// </summary>
    WHEN_IDLE = 2,

    /// <summary>
    /// Add the result to the conversation context, interrupt ongoing generation and prompt to generate output.
    /// </summary>
    INTERRUPT = 3
}
