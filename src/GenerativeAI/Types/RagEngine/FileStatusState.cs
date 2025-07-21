using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Represents the state of a file in the RAG engine.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FileStatusState>))]
public enum FileStatusState
{

    /// <summary>
    /// The state is unspecified.
    /// </summary>
    [EnumMember(Value = @"STATE_UNSPECIFIED")]
    STATE_UNSPECIFIED = 0,

    /// <summary>
    /// The file is active and ready for use.
    /// </summary>
    [EnumMember(Value = @"ACTIVE")]
    ACTIVE = 1,

    /// <summary>
    /// The file has an error and cannot be used.
    /// </summary>
    [EnumMember(Value = @"ERROR")]
    ERROR = 2,

}