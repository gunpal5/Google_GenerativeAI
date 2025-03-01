using System.Runtime.Serialization;

namespace GenerativeAI.Types.RagEngine;

public enum FileStatusState
{

    [EnumMember(Value = @"STATE_UNSPECIFIED")]
    STATE_UNSPECIFIED = 0,

    [EnumMember(Value = @"ACTIVE")]
    ACTIVE = 1,

    [EnumMember(Value = @"ERROR")]
    ERROR = 2,

}