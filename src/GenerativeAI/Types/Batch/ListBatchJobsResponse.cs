using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response for the list batch jobs method.
/// Contains a paginated list of batch jobs.
/// </summary>
public class ListBatchJobsResponse
{
    /// <summary>
    /// A token to retrieve the next page of results.
    /// Pass to the next list request to obtain that page.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    /// <summary>
    /// List of BatchJobs in the requested page.
    /// </summary>
    [JsonPropertyName("batchJobs")]
    public List<BatchJob>? BatchJobs { get; set; }
}
