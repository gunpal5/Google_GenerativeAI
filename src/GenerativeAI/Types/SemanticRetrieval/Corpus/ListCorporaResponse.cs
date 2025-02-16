using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a list corpora response.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.list">See Official API Documentation</seealso>
public class ListCorporaResponse
{
    /// <summary>
    /// The returned corpora.
    /// </summary>
    [JsonPropertyName("corpora")]
    public List<Corpus>? Corpora { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page.
    /// If this field is omitted, there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}