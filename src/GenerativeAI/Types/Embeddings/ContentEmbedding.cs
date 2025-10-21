﻿using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a list of floats that make up an embedding.
/// </summary>
/// <seealso href="https://ai.google.dev/api/embeddings#contentembedding">See Official API Documentation</seealso>
public class ContentEmbedding
{
    /// <summary>
    /// The embedding values.
    /// </summary>
    [JsonPropertyName("values")]
    public List<float>? Values { get; set; }

    /// <summary>
    /// Statistics of the input text associated with the result of content embedding.
    /// </summary>
    [JsonPropertyName("statistics")]
    public ContentEmbeddingStatistics? Statistics { get; set; }
}