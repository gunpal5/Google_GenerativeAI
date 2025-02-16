using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from <see cref="ListPermissions"/> containing a paginated list of permissions.
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/permissions#response-body_1">See Official API Documentation</seealso>
/// </summary>
public class ListPermissionsResponse
{
    /// <summary>
    /// Returned permissions.
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<Permission>? Permissions { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page.
    /// If this field is omitted, there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}