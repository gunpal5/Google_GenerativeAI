using System.Runtime.Serialization;

namespace GenerativeAI.Types.RagEngine;

public enum CorpusStatusState
{
    [EnumMember(Value = @"UNKNOWN")]
    UNKNOWN = 0,

    [EnumMember(Value = @"INITIALIZED")]
    INITIALIZED = 1,

    [EnumMember(Value = @"ACTIVE")]
    ACTIVE = 2,

    [EnumMember(Value = @"ERROR")]
    ERROR = 3,
}