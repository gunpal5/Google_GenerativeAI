using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response to list models.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/models/list">See Official API Documentation</seealso>
public class ListModelsResponse
{
    /// <summary>
    /// The returned Models.
    /// </summary>
    [JsonPropertyName("models")]
    public List<Model>? Models { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page.
    /// If this field is omitted, there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}