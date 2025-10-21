using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Wrapper for embedding batch request (Google AI format).
/// </summary>
public class EmbeddingBatchWrapper
{
    /// <summary>
    /// The batch configuration for embeddings.
    /// </summary>
    [JsonPropertyName("batch")]
    public EmbeddingBatchConfig? Batch { get; set; }
}

/// <summary>
/// Batch configuration for embeddings.
/// </summary>
public class EmbeddingBatchConfig
{
    /// <summary>
    /// Input configuration for the embedding batch job.
    /// </summary>
    [JsonPropertyName("inputConfig")]
    public EmbeddingBatchInputConfig? InputConfig { get; set; }

    /// <summary>
    /// Display name for the batch job.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
