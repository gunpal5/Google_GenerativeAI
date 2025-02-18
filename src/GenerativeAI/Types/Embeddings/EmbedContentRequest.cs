using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a request containing the content for the model to embed.
/// </summary>
/// <seealso href="https://ai.google.dev/api/embeddings#EmbedContentRequest">See Official API Documentation</seealso>
public class EmbedContentRequest
{
    /// <summary>
    /// Gets or sets the model's resource name. This serves as an ID for the model to use.
    /// Format: models/{model}.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the content to embed. Only the <see cref="Part.Text">Parts.Text</see> fields will be counted.
    /// </summary>
    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    /// <summary>
    /// Gets or sets the optional task type for which the embeddings will be used.
    /// Can only be set for models/embedding-001.
    /// </summary>
    [JsonPropertyName("taskType")]
    public TaskType? TaskType { get; set; }

    /// <summary>
    /// Gets or sets an optional title for the text when the task type is <see cref="TaskType.RETRIEVAL_DOCUMENT"/>.
    /// Specifying a title provides better quality embeddings for retrieval.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional reduced dimension for the output embedding.
    /// If set, excessive values in the output embedding are truncated from the end.
    /// Supported by newer models since 2024 only. Cannot be set when using models/embedding-001.
    /// </summary>
    [JsonPropertyName("outputDimensionality")]
    public int? OutputDimensionality { get; set; }
}