using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Input configuration for embedding batch jobs (Google AI format).
/// </summary>
public class EmbeddingBatchInputConfig
{
    /// <summary>
    /// The nested requests wrapper containing the array of embedding requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public EmbedRequestsWrapper? Requests { get; set; }

    /// <summary>
    /// The file name for batch processing.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
}

/// <summary>
/// Wrapper for embedding requests array (Google AI nested structure).
/// </summary>
public class EmbedRequestsWrapper
{
    /// <summary>
    /// The array of embedding requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public List<EmbedRequestWrapper>? Requests { get; set; }
}

/// <summary>
/// Wrapper for a single embed request in batch processing.
/// </summary>
public class EmbedRequestWrapper
{
    /// <summary>
    /// The actual embedding request.
    /// </summary>
    [JsonPropertyName("request")]
    public EmbedRequestBody? Request { get; set; }
}

/// <summary>
/// The actual embedding request body for batch processing.
/// </summary>
public class EmbedRequestBody
{
    /// <summary>
    /// The model to use for embedding.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// The content to embed (singular).
    /// </summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    /// <summary>
    /// Optional task type for the embeddings.
    /// </summary>
    [JsonPropertyName("taskType")]
    public TaskType? TaskType { get; set; }

    /// <summary>
    /// Optional title for retrieval document task type.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Optional reduced dimension for output embedding.
    /// </summary>
    [JsonPropertyName("outputDimensionality")]
    public int? OutputDimensionality { get; set; }
}
