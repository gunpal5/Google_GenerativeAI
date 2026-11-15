using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The different activity states of the live session.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<InteractionStatus>))]
public enum InteractionStatus
{
    /// <summary>
    /// Unspecified interaction status.
    /// </summary>
    INTERACTION_STATUS_UNSPECIFIED = 0,

    /// <summary>
    /// The server is still actively processing user input or running background reasoning. More model output may follow.
    /// </summary>
    IN_PROGRESS = 1,

    /// <summary>
    /// Deprecated: Use <see cref="IDLE"/> instead.
    /// </summary>
    REQUIRES_ACTION = 2,

    /// <summary>
    /// The server has completed all processing and background reasoning.
    /// </summary>
    IDLE = 3
}
