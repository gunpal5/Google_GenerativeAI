using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

[JsonConverter(typeof(JsonStringEnumConverter<CorpusStatusState>))]
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