using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The type and ID of the Google Drive resource.
/// </summary>
public class GoogleDriveSourceResourceId
{
    /// <summary>
    /// Required. The ID of the Google Drive resource.
    /// </summary>
    [JsonPropertyName("resourceId")]
    public string? ResourceId { get; set; } 

    /// <summary>
    /// Required. The type of the Google Drive resource.
    /// </summary>
    [JsonPropertyName("resourceType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GoogleDriveSourceResourceIdResourceType? ResourceType { get; set; } 
}