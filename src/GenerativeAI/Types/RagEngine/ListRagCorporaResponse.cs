using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Response message for VertexRagDataService.ListRagCorpora.
/// </summary>
public class ListRagCorporaResponse
{
    /// <summary>
    /// A token to retrieve the next page of results. Pass to ListRagCorporaRequest.page_token to obtain that page.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; } 

    /// <summary>
    /// List of RagCorpora in the requested page.
    /// </summary>
    [JsonPropertyName("ragCorpora")]
    public System.Collections.Generic.ICollection<RagCorpus>? RagCorpora { get; set; } 
}