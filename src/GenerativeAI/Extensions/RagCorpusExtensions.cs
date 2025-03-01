using GenerativeAI.Types.RagEngine;

namespace GenerativeAI;

/// <summary>
/// Extension methods for configuring the <see cref="RagCorpus"/> with different vector database configurations.
/// </summary>
public static class RagCorpusExtensions
{
    /// <summary>
    /// Configures the <see cref="RagCorpus"/> to use Pinecone as the vector database.
    /// </summary>
    /// <param name="corpus">The <see cref="RagCorpus"/> instance to configure.</param>
    /// <param name="config">The <see cref="PineconeConfig"/> containing Pinecone-specific settings.</param>
    /// <param name="apiKeySecretResourceName">The resource name of the secret containing the Pinecone API key.<br/>Format: projects/{PROJECT_NUMBER}/secrets/{SECRET_ID}/versions/{VERSION_ID}</param>
    public static void AddPinecone(this RagCorpus corpus, RagVectorDbConfigPinecone config,
        string apiKeySecretResourceName)
    {
        corpus.VectorDbConfig = new RagVectorDbConfig()
        {
            Pinecone = config,
            ApiAuth = new ApiAuth()
            {
                ApiKeyConfig = new ApiAuthApiKeyConfig()
                {
                    ApiKeySecretVersion = apiKeySecretResourceName
                }
            }
        };
    }

    /// <summary>
    /// Configures the <see cref="RagCorpus"/> to use Weaviate as the vector database.
    /// </summary>
    /// <param name="corpus">The <see cref="RagCorpus"/> instance to configure.</param>
    /// <param name="config">The <see cref="RagVectorDbConfigWeaviate"/> containing Weaviate-specific settings.</param>
    /// <param name="apiKeySecretResourceName">The resource name of the secret containing the Weaviate API key.<br/>Format: projects/{PROJECT_NUMBER}/secrets/{SECRET_ID}/versions/{VERSION_ID}</param>
    public static void AddWeaviate(this RagCorpus corpus, RagVectorDbConfigWeaviate config,
        string apiKeySecretResourceName)
    {
        corpus.VectorDbConfig = new RagVectorDbConfig()
        {
            Weaviate = config,
            ApiAuth = new ApiAuth()
            {
                ApiKeyConfig = new ApiAuthApiKeyConfig()
                {
                    ApiKeySecretVersion = apiKeySecretResourceName
                }
            }
        };
    }

    /// <summary>
    /// Configures the <see cref="RagCorpus"/> to use Vertex AI Feature Store as the vector database.
    /// </summary>
    /// <param name="corpus">The <see cref="RagCorpus"/> instance to configure.</param>
    /// <param name="config">The <see cref="RagVectorDbConfigVertexFeatureStore"/> containing Vertex AI Feature Store-specific settings.</param>
    public static void AddVertexFeatureStore(this RagCorpus corpus, RagVectorDbConfigVertexFeatureStore config)
    {
        corpus.VectorDbConfig = new RagVectorDbConfig()
        {
            VertexFeatureStore = config
        };
    }

    /// <summary>
    /// Configures the <see cref="RagCorpus"/> to use Vertex Vector Search as the vector database.
    /// </summary>
    /// <param name="corpus">The <see cref="RagCorpus"/> instance to configure.</param>
    /// <param name="config">The <see cref="RagVectorDbConfigVertexVectorSearch"/> containing Vertex Vector Search-specific settings.</param>
    public static void AddVertexSearch(this RagCorpus corpus, RagVectorDbConfigVertexVectorSearch config)
    {
        corpus.VectorDbConfig = new RagVectorDbConfig()
        {
            VertexVectorSearch = config
        };
    }

    public static void AddEmbeddingModel(this RagCorpus corpus, string embeddingModelName)
    {
        if (embeddingModelName == null)
            throw new ArgumentNullException(nameof(embeddingModelName));
        
        corpus.VectorDbConfig ??= new RagVectorDbConfig();

        corpus.VectorDbConfig.RagEmbeddingModelConfig = new RagEmbeddingModelConfig()
        {
            VertexPredictionEndpoint =
            {
                Endpoint = embeddingModelName
            }
        };
    }
}