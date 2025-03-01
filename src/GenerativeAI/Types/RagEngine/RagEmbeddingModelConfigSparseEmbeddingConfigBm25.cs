using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Message for BM25 parameters.
/// </summary>
public class RagEmbeddingModelConfigSparseEmbeddingConfigBm25
{
    /// <summary>
    /// Optional. The parameter to control document length normalization. It determines how much the document length affects the final score. b is in the range of [0, 1]. The default value is 0.75.
    /// </summary>
    [JsonPropertyName("b")]
    public float? B { get; set; }

    /// <summary>
    /// Optional. The parameter to control term frequency saturation. It determines the scaling between the matching term frequency and final score. k1 is in the range of [1.2, 3]. The default value is 1.2.
    /// </summary>
    [JsonPropertyName("k1")]
    public float? K1 { get; set; }

    /// <summary>
    /// Optional. Use multilingual tokenizer if set to true.
    /// </summary>
    [JsonPropertyName("multilingual")]
    public bool? Multilingual { get; set; }
}