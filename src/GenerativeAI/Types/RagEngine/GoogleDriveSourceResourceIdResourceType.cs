using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the type of Google Drive resource.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GoogleDriveSourceResourceIdResourceType>))]
public enum GoogleDriveSourceResourceIdResourceType
{

    /// <summary>
    /// The resource type is unspecified.
    /// </summary>
    [EnumMember(Value = @"RESOURCE_TYPE_UNSPECIFIED")]
    RESOURCE_TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// The resource is a file.
    /// </summary>
    [EnumMember(Value = @"RESOURCE_TYPE_FILE")]
    RESOURCE_TYPE_FILE = 1,

    /// <summary>
    /// The resource is a folder.
    /// </summary>
    [EnumMember(Value = @"RESOURCE_TYPE_FOLDER")]
    RESOURCE_TYPE_FOLDER = 2,

}