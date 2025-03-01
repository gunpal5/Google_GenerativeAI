using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Response message for VertexRagDataService.ListRagFiles.
/// </summary>
public class ListRagFilesResponse
{
    /// <summary>
    /// A token to retrieve the next page of results. Pass to ListRagFilesRequest.page_token to obtain that page.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; } 

    /// <summary>
    /// List of RagFiles in the requested page.
    /// </summary>
    [JsonPropertyName("ragFiles")]
    public System.Collections.Generic.ICollection<RagFile>? RagFiles { get; set; } 
}