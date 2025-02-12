using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from <c>chunks.list</c> containing a paginated list of <see cref="Chunk"/>.
/// The <see cref="Chunk"/>s are sorted by ascending <c>chunk.create_time</c>.
/// </summary>
/// <seealso href="https://developers.google.com/ai/generativelanguage/reference/rest/v1beta/corpora.documents.chunks/list">See Official API Documentation</seealso>
public class ListChunksResponse
{
    /// <summary>
    /// The returned <see cref="Chunk"/>s.
    /// </summary>
    [JsonPropertyName("chunks")]
    public List<Chunk>? Chunks { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page. If this field is omitted, there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}