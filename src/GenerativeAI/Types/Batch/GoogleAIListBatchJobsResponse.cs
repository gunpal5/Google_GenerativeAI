using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response for the list batch jobs method (Google AI wire format).
/// Contains operations array that needs to be transformed to batch jobs.
/// </summary>
public class GoogleAIListBatchJobsResponse
{
    /// <summary>
    /// A token to retrieve the next page of results.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    /// <summary>
    /// List of operation wrappers (each contains name + metadata).
    /// </summary>
    [JsonPropertyName("operations")]
    public List<BatchJobResponse>? Operations { get; set; }
}
