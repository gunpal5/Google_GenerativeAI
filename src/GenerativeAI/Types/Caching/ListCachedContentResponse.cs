using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response to listing cached contents.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#response-body_1">See Official API Documentation</seealso>
public class ListCachedContentsResponse
{
    /// <summary>
    /// The list of cached contents.
    /// </summary>
    [JsonPropertyName("cachedContents")]
    public List<CachedContent>? CachedContents { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page.
    /// If this field is omitted, there are no subsequent pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}