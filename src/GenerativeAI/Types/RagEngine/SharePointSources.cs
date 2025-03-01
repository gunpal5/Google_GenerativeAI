using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The SharePointSources to pass to ImportRagFiles.
/// </summary>
public class SharePointSources
{
    /// <summary>
    /// The SharePoint sources.
    /// </summary>
    [JsonPropertyName("sharePointSources")]
    public System.Collections.Generic.ICollection<SharePointSource>? SharePointSourceCollection { get; set; } 
}