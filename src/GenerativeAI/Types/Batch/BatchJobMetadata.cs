using System.Text.Json;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response wrapper for batch job operations (Google AI format).
/// </summary>
[JsonConverter(typeof(BatchJobResponseConverter))]
public class BatchJobResponse
{
    /// <summary>
    /// The resource name of the batch job.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The batch job metadata containing detailed information.
    /// </summary>
    [JsonPropertyName("metadata")]
    public BatchJob? Metadata { get; set; }

    /// <summary>
    /// Indicates if the operation is complete.
    /// </summary>
    [JsonPropertyName("done")]
    public bool? Done { get; set; }

    /// <summary>
    /// Output for embedding batch jobs. Populated when metadata.@type contains "EmbedContent".
    /// </summary>
    public BatchEmbeddingOutput? EmbeddingOutput { get; set; }

    /// <summary>
    /// Output for content generation batch jobs. Populated when metadata.@type contains "GenerateContent".
    /// </summary>
    public BatchContentOutput? ContentOutput { get; set; }
}

/// <summary>
/// Output wrapper for embedding batch jobs.
/// </summary>
public class BatchEmbeddingOutput
{
    /// <summary>
    /// The type identifier for the output object.
    /// </summary>
    [JsonPropertyName("@type")]
    public string? Type { get; set; }

    /// <summary>
    /// Wrapper containing the array of embedding responses.
    /// </summary>
    [JsonPropertyName("inlinedResponses")]
    public BatchEmbeddingResponsesWrapper? InlinedResponses { get; set; }
}

/// <summary>
/// Wrapper for embedding batch responses (nested structure).
/// </summary>
public class BatchEmbeddingResponsesWrapper
{
    /// <summary>
    /// Array of embedding responses.
    /// </summary>
    [JsonPropertyName("inlinedResponses")]
    public List<InlinedEmbedContentResponse>? InlinedResponses { get; set; }
}

/// <summary>
/// Output wrapper for content generation batch jobs.
/// </summary>
public class BatchContentOutput
{
    /// <summary>
    /// The type identifier for the output object.
    /// </summary>
    [JsonPropertyName("@type")]
    public string? Type { get; set; }

    /// <summary>
    /// Wrapper containing the array of content generation responses.
    /// </summary>
    [JsonPropertyName("inlinedResponses")]
    public BatchContentResponsesWrapper? InlinedResponses { get; set; }
}

/// <summary>
/// Wrapper for content generation batch responses (nested structure).
/// </summary>
public class BatchContentResponsesWrapper
{
    /// <summary>
    /// Array of content generation responses.
    /// </summary>
    [JsonPropertyName("inlinedResponses")]
    public List<InlinedResponse>? InlinedResponses { get; set; }
}

/// <summary>
/// Statistics about a batch job's progress.
/// </summary>
public class BatchStats
{
    /// <summary>
    /// Total number of requests in the batch.
    /// </summary>
    [JsonPropertyName("requestCount")]
    public string? RequestCount { get; set; }

    /// <summary>
    /// Number of requests that are pending.
    /// </summary>
    [JsonPropertyName("pendingRequestCount")]
    public string? PendingRequestCount { get; set; }

    /// <summary>
    /// Number of requests that have succeeded.
    /// </summary>
    [JsonPropertyName("succeededRequestCount")]
    public string? SucceededRequestCount { get; set; }

    /// <summary>
    /// Number of requests that have failed.
    /// </summary>
    [JsonPropertyName("failedRequestCount")]
    public string? FailedRequestCount { get; set; }
}
