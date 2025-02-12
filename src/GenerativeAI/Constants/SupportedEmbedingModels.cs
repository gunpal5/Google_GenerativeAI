namespace GenerativeAI.Constants;

public static class SupportedEmbedingModels
{
    public static readonly List<string> All = new()
    {
        // From GeminiConstants
        GeminiConstants.TextEmbedding,
        GeminiConstants.Embedding,

        // From VertexAIModels.Embeddings
        VertexAIModels.Embeddings.TextEmbeddingGecko001,
        VertexAIModels.Embeddings.TextEmbeddingGecko002,
        VertexAIModels.Embeddings.TextEmbeddingGecko003,
        VertexAIModels.Embeddings.TextEmbedding004,
        VertexAIModels.Embeddings.TextEmbeddingGeckoMultilingual001,
        VertexAIModels.Embeddings.TextMultilingualEmbedding002,
        VertexAIModels.Embeddings.MultimodalEmbedding
    };
}
