using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response for the list tuning jobs method.
/// Contains a paginated list of tuning jobs.
/// </summary>
public class ListTuningJobsResponse
{
    /// <summary>
    /// A token to retrieve the next page of results.
    /// Pass to ListTuningJobsRequest.page_token to obtain that page.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    /// <summary>
    /// List of TuningJobs in the requested page.
    /// </summary>
    [JsonPropertyName("tuningJobs")]
    public List<TuningJob>? TuningJobs { get; set; }
}
