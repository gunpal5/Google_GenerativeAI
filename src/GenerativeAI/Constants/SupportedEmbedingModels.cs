namespace GenerativeAI;

/// <summary>
/// Provides a centralized collection of supported embedding model identifiers for use in the GenerativeAI namespace.
/// </summary>
/// <remarks>
/// SupportedEmbedingModels contains a static list of embedding model identifiers which are utilized in the
/// embedding-related functionalities of the GenerativeAI library. These identifiers are sourced from different
/// model providers, including Google AI Models and Vertex AI Models.
/// </remarks>
/// <example>
/// Attempting to use a model for embedding that is not included in SupportedEmbedingModels will result in a
/// NotSupportedException. This ensures only verified and compatible models are used for embeddings.
/// </example>
public static class SupportedEmbedingModels
{
    /// <summary>
    /// Represents a collection of supported embedding model identifiers.
    /// </summary>
    /// <remarks>
    /// This list includes model identifiers from multiple sources, such as GoogleAIModels
    /// and VertexAIModels.Embeddings, which are used for text and multimodal embeddings.
    /// Some of the models included may have been deprecated or superseded by newer versions.
    /// Refer to the individual model source documentation for additional details.
    /// </remarks>
    public static readonly List<string> All = new()
    {
        // From GeminiConstants
        GoogleAIModels.GeminiEmbedding2,
        GoogleAIModels.GeminiEmbedding,
#pragma warning disable CS0618 // Kept so existing callers of the shut-down model still pass validation
        GoogleAIModels.TextEmbedding,
#pragma warning restore CS0618

        // From VertexAIModels.Embeddings
        VertexAIModels.Embeddings.GeminiEmbedding001,
        VertexAIModels.Gemini.TextEmbedding005,
        VertexAIModels.Embeddings.TextEmbeddingGecko001,
        VertexAIModels.Embeddings.TextEmbeddingGecko002,
        VertexAIModels.Embeddings.TextEmbeddingGecko003,
        VertexAIModels.Embeddings.TextEmbedding004,
        VertexAIModels.Embeddings.TextEmbeddingGeckoMultilingual001,
        VertexAIModels.Embeddings.TextMultilingualEmbedding002,
        VertexAIModels.Embeddings.MultimodalEmbedding
    };
}
