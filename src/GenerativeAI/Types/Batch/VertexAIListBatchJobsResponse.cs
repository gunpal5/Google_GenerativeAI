using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response for the list batch jobs method (Vertex AI wire format).
/// </summary>
public class VertexAIListBatchJobsResponse
{
    /// <summary>
    /// A token to retrieve the next page of results.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    /// <summary>
    /// List of batch prediction jobs.
    /// </summary>
    [JsonPropertyName("batchPredictionJobs")]
    public List<BatchJob>? BatchPredictionJobs { get; set; }
}
