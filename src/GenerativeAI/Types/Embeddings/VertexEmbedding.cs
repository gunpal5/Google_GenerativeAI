using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Embedding options sent in the Vertex AI <c>embedContent</c> request format.
/// Built from <see cref="EmbedContentRequest"/> by the SDK; you normally do not create this directly.
/// </summary>
public class VertexEmbedContentConfig
{
    /// <summary>
    /// The task the embedding will be used for.
    /// </summary>
    [JsonPropertyName("taskType")]
    public TaskType? TaskType { get; set; }

    /// <summary>
    /// Title of the document, for <see cref="Types.TaskType.RETRIEVAL_DOCUMENT"/>.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Reduced dimension of the output embedding.
    /// </summary>
    [JsonPropertyName("outputDimensionality")]
    public int? OutputDimensionality { get; set; }

    /// <summary>
    /// Whether to truncate input that exceeds the model's maximum length instead of failing.
    /// </summary>
    [JsonPropertyName("autoTruncate")]
    public bool? AutoTruncate { get; set; }

    /// <summary>
    /// Whether to run OCR on documents (for example PDFs) before embedding.
    /// </summary>
    [JsonPropertyName("documentOcr")]
    public bool? DocumentOcr { get; set; }

    /// <summary>
    /// Whether to extract and embed the audio track of video input.
    /// </summary>
    [JsonPropertyName("audioTrackExtraction")]
    public bool? AudioTrackExtraction { get; set; }
}

/// <summary>
/// Request body for the Vertex AI <c>embedContent</c> method, used by Gemini embedding models on Vertex AI.
/// </summary>
public class VertexEmbedContentRequest
{
    /// <summary>
    /// The content to embed.
    /// </summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    /// <summary>
    /// Embedding options.
    /// </summary>
    [JsonPropertyName("embedContentConfig")]
    public VertexEmbedContentConfig? EmbedContentConfig { get; set; }
}

/// <summary>
/// Request body for the Vertex AI <c>batchEmbedContents</c> method.
/// </summary>
public class VertexBatchEmbedContentsRequest
{
    /// <summary>
    /// The individual embedding requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public List<VertexEmbedContentRequest>? Requests { get; set; }
}

/// <summary>
/// Response of the Vertex AI <c>embedContent</c> method.
/// </summary>
public class VertexEmbedContentResponse
{
    /// <summary>
    /// The embedding.
    /// </summary>
    [JsonPropertyName("embedding")]
    public ContentEmbedding? Embedding { get; set; }

    /// <summary>
    /// Token usage of the request.
    /// </summary>
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata? UsageMetadata { get; set; }

    /// <summary>
    /// Whether the input was truncated.
    /// </summary>
    [JsonPropertyName("truncated")]
    public bool? Truncated { get; set; }
}

/// <summary>
/// Request body for the Vertex AI <c>predict</c> method, used by text embedding models such as
/// <c>text-embedding-005</c> and <c>textembedding-gecko</c>.
/// </summary>
public class VertexEmbeddingPredictRequest
{
    /// <summary>
    /// The texts to embed.
    /// </summary>
    [JsonPropertyName("instances")]
    public List<VertexEmbeddingInstance>? Instances { get; set; }

    /// <summary>
    /// Options that apply to all instances.
    /// </summary>
    [JsonPropertyName("parameters")]
    public VertexEmbeddingParameters? Parameters { get; set; }
}

/// <summary>
/// One text to embed with the Vertex AI <c>predict</c> method.
/// </summary>
public class VertexEmbeddingInstance
{
    /// <summary>
    /// The text to embed.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// The task the embedding will be used for. The predict API uses snake_case for this field.
    /// </summary>
    [JsonPropertyName("task_type")]
    public TaskType? TaskType { get; set; }

    /// <summary>
    /// Title of the document, for <see cref="Types.TaskType.RETRIEVAL_DOCUMENT"/>.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// MIME type of the content.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}

/// <summary>
/// Options for the Vertex AI embedding <c>predict</c> method.
/// </summary>
public class VertexEmbeddingParameters
{
    /// <summary>
    /// Reduced dimension of the output embedding.
    /// </summary>
    [JsonPropertyName("outputDimensionality")]
    public int? OutputDimensionality { get; set; }

    /// <summary>
    /// Whether to truncate input that exceeds the model's maximum length instead of failing.
    /// </summary>
    [JsonPropertyName("autoTruncate")]
    public bool? AutoTruncate { get; set; }
}

/// <summary>
/// Response of the Vertex AI embedding <c>predict</c> method.
/// </summary>
public class VertexEmbeddingPredictResponse
{
    /// <summary>
    /// One prediction per instance, in request order.
    /// </summary>
    [JsonPropertyName("predictions")]
    public List<VertexEmbeddingPrediction>? Predictions { get; set; }
}

/// <summary>
/// One prediction returned by the Vertex AI embedding <c>predict</c> method.
/// </summary>
public class VertexEmbeddingPrediction
{
    /// <summary>
    /// The embedding.
    /// </summary>
    [JsonPropertyName("embeddings")]
    public VertexPredictEmbedding? Embeddings { get; set; }
}

/// <summary>
/// An embedding returned by the Vertex AI embedding <c>predict</c> method.
/// </summary>
public class VertexPredictEmbedding
{
    /// <summary>
    /// The embedding values.
    /// </summary>
    [JsonPropertyName("values")]
    public List<float>? Values { get; set; }

    /// <summary>
    /// Statistics about the input.
    /// </summary>
    [JsonPropertyName("statistics")]
    public VertexPredictEmbeddingStatistics? Statistics { get; set; }
}

/// <summary>
/// Input statistics returned by the Vertex AI embedding <c>predict</c> method (snake_case on the wire).
/// </summary>
public class VertexPredictEmbeddingStatistics
{
    /// <summary>
    /// Whether the input was truncated.
    /// </summary>
    [JsonPropertyName("truncated")]
    public bool? Truncated { get; set; }

    /// <summary>
    /// Number of tokens in the input.
    /// </summary>
    [JsonPropertyName("token_count")]
    public double? TokenCount { get; set; }
}
