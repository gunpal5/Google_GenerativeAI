using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Defines the possible states of a corpus within the system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CorpusStatusState>))]
public enum CorpusStatusState
{
    /// <summary>
    /// Represents an undefined or unknown state for a corpus.
    /// This value indicates that the state of the corpus could not be determined
    /// or has not been explicitly set.
    /// </summary>
    [EnumMember(Value = @"UNKNOWN")]
    UNKNOWN = 0,

    /// <summary>
    /// Indicates that the corpus has been successfully initialized.
    /// This state signifies that the corpus has been prepared and is ready for further operations
    /// but is not currently active.
    /// </summary>
    [EnumMember(Value = @"INITIALIZED")]
    INITIALIZED = 1,

    /// <summary>
    /// Indicates that the corpus is in an active state and operational.
    /// This status denotes that the corpus is fully initialized and ready for use
    /// within the system.
    /// </summary>
    [EnumMember(Value = @"ACTIVE")]
    ACTIVE = 2,

    /// <summary>
    /// Indicates that the corpus has encountered an error state.
    /// This value signals that an issue occurred during the handling or processing
    /// of the corpus, preventing it from functioning as expected.
    /// </summary>
    [EnumMember(Value = @"ERROR")]
    ERROR = 3,
}