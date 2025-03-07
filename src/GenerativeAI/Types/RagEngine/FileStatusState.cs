using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

[JsonConverter(typeof(JsonStringEnumConverter<FileStatusState>))]
public enum FileStatusState
{

    [EnumMember(Value = @"STATE_UNSPECIFIED")]
    STATE_UNSPECIFIED = 0,

    [EnumMember(Value = @"ACTIVE")]
    ACTIVE = 1,

    [EnumMember(Value = @"ERROR")]
    ERROR = 2,

}