using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response to list files.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/files/list">See Official API Documentation</seealso>
public class ListFilesResponse
{
    /// <summary>
    /// The list of <c>File</c>s.
    /// </summary>
    [JsonPropertyName("files")]
    public List<RemoteFile>? Files { get; set; }

    /// <summary>
    /// A token that can be sent as a <c>pageToken</c> into a subsequent <c>files.list</c> call.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}