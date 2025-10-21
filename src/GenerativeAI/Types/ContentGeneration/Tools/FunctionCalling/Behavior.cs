using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Defines the function behavior. Defaults to BLOCKING.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Behavior>))]
public enum Behavior
{
    /// <summary>
    /// This value is unused.
    /// </summary>
    UNSPECIFIED = 0,

    /// <summary>
    /// If set, the system will wait to receive the function response before continuing the conversation.
    /// </summary>
    BLOCKING = 1,

    /// <summary>
    /// If set, the system will not wait to receive the function response. Instead, it will attempt
    /// to handle function responses as they become available while maintaining the conversation
    /// between the user and the model.
    /// </summary>
    NON_BLOCKING = 2
}
