using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Specifies the type of task for which the embedding will be used.
/// </summary>
/// <seealso href="https://ai.google.dev/api/embeddings#tasktype">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter<TaskType>))]
public enum TaskType
{
    /// <summary>
    /// Unset value, which will default to one of the other enum values.
    /// </summary>
    TASK_TYPE_UNSPECIFIED,

    /// <summary>
    /// Embeddings optimized for general search queries. Use RETRIEVAL_QUERY for queries; RETRIEVAL_DOCUMENT for documents to be retrieved.
    /// Use case: Custom search
    /// </summary>
    RETRIEVAL_QUERY,

    /// <summary>
    /// Embeddings optimized for document search. Indexing articles, books, or web pages for search.
    /// </summary>
    RETRIEVAL_DOCUMENT,

    /// <summary>
    /// Embeddings optimized to assess text similarity.
    /// Use case: Recommendation systems, duplicate detection
    /// </summary>
    SEMANTIC_SIMILARITY,

    /// <summary>
    /// Embeddings optimized to classify texts according to preset labels.
    /// Use case: Sentiment analysis, spam detection
    /// </summary>
    CLASSIFICATION,

    /// <summary>
    /// Embeddings optimized to cluster texts based on their similarities.
    /// Use case: Document organization, market research, anomaly detection
    /// </summary>
    CLUSTERING,

    /// <summary>
    /// Embeddings optimized for retrieval of code blocks based on natural language queries. Use CODE_RETRIEVAL_QUERY for queries; RETRIEVAL_DOCUMENT for code blocks to be retrieved.
    /// Use case: Code suggestions and search
    /// </summary>
    CODE_RETRIEVAL_QUERY,

    /// <summary>
    /// Embeddings for questions in a question-answering system, optimized for finding documents that answer the question. Use QUESTION_ANSWERING for questions; RETRIEVAL_DOCUMENT for documents to be retrieved.
    /// Use case: Chatbox
    /// </summary>
    QUESTION_ANSWERING,

    /// <summary>
    /// Embeddings for statements that need to be verified, optimized for retrieving documents that contain evidence supporting or refuting the statement. Use FACT_VERIFICATION for the target text; RETRIEVAL_DOCUMENT for documents to be retrieved.
    /// Use case: Automated fact-checking systems
    /// </summary>
    FACT_VERIFICATION
}