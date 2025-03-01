using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The Google Drive location for the input content.
/// </summary>
public class GoogleDriveSource
{
    /// <summary>
    /// Required. Google Drive resource IDs.
    /// </summary>
    [JsonPropertyName("resourceIds")]
    public System.Collections.Generic.ICollection<GoogleDriveSourceResourceId>? ResourceIds { get; set; } 
}