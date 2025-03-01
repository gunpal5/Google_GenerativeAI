using System.Runtime.Serialization;

namespace GenerativeAI.Types.RagEngine;

public enum GoogleDriveSourceResourceIdResourceType
{

    [EnumMember(Value = @"RESOURCE_TYPE_UNSPECIFIED")]
    RESOURCE_TYPE_UNSPECIFIED = 0,

    [EnumMember(Value = @"RESOURCE_TYPE_FILE")]
    RESOURCE_TYPE_FILE = 1,

    [EnumMember(Value = @"RESOURCE_TYPE_FOLDER")]
    RESOURCE_TYPE_FOLDER = 2,

}