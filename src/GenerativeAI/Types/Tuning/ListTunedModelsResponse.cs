using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response from <c>tunedModels.list</c> containing a paginated list of Models.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tuning#response-body_4">See Official API Documentation</seealso>
public class ListTunedModelsResponse
{
    /// <summary>
    /// The returned Models.
    /// </summary>
    [JsonPropertyName("tunedModels")]
    public List<TunedModel>? TunedModels { get; set; }

    /// <summary>
    /// A token, which can be sent as <c>pageToken</c> to retrieve the next page.
    /// If this field is omitted, there are no more pages.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}