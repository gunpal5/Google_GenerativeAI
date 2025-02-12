using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from <c>documents.list</c> containing a paginated list of <see cref="Document"/>.
/// The <see cref="Document"/>s are sorted by ascending <c>document.create_time</c>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#response-body_2">See Official API Documentation</seealso>
public class ListDocumentsResponse
{
    /// <summary>
    /// The returned <see cref="Document"/>s.
    /// </summary>
    [JsonPropertyName("documents")]
    public List<Document>? Documents { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page. If this field is omitted, there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}