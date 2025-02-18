using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Represents an interface defining operations for generative AI systems, including
/// initializing models, retrieving model details, listing available models, and accessing
/// platform-specific adapters.
/// </summary>
public interface IGenerativeAI
{
    /// <summary>
    /// Creates and initializes a generative model capable of producing AI-generated content.
    /// The model is configured with the specified parameters to customize its behavior and ensure safety compliance when generating content.
    /// </summary>
    /// <param name="modelName">The name of the generative model to initialize, used to identify the specific AI model for content generation.</param>
    /// <param name="config">Optional parameter specifying the generation configuration, such as temperature, max tokens, or other customization options.</param>
    /// <param name="safetyRatings">Optional collection of safety settings used to enforce content safety thresholds during generation.</param>
    /// <param name="systemInstruction">Optional system-level instruction guiding the behavior of the generative model.</param>
    /// <returns>An instance of <see cref="GenerativeModel"/> configured with the specified parameters for generative tasks.</returns>
    GenerativeModel CreateGenerativeModel(
        string modelName,
        GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null,
        string? systemInstruction = null);

     /// <summary>
     /// Creates and initializes an embedding model for use in embedding-related AI tasks.
     /// The created model is configured with the provided model name and integrates with the underlying platform adapter.
     /// </summary>
     /// <param name="modelName">The name of the embedding model to initialize, used to identify the specific AI model for embeddings.</param>
     /// <returns>An instance of <see cref="EmbeddingModel"/> initialized with the specified model name and platform configurations.</returns>

    EmbeddingModel CreateEmbeddingModel(string modelName);

    /// <summary>
    /// Asynchronously retrieves a list of AI models available in Google's AI platform.
    /// This method supports pagination to fetch models in batches using the specified page size
    /// and page token, enabling efficient handling of large model datasets.
    /// </summary>
    /// <param name="pageSize">Optional parameter specifying the maximum number of models to retrieve per page. Defaults to null for the API's standard page size.</param>
    /// <param name="pageToken">Optional parameter indicating the token for the next page of results, as provided in a previous response.</param>
    /// <param name="cancellationToken">Optional parameter for propagating cancellation signals, allowing the operation to be canceled if needed.</param>
    /// <returns>A task representing the asynchronous operation, which contains a <see cref="ListModelsResponse"/> with the retrieved models and pagination details.</returns>
    Task<ListModelsResponse> ListModelsAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific AI model by its name asynchronously from the Google's AI platform.
    /// </summary>
    /// <param name="modelName">The unique name of the model to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the requested model details as a <see cref="Model"/> object.</returns>
    Task<Model> GetModelAsync(string modelName,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Retrieves the platform adapter responsible for handling platform-specific operations such as authentication, API versioning, and URL construction.
    /// The platform adapter ensures seamless integration with the underlying platform by managing necessary credentials and configurations.
    /// </summary>
    /// <returns>An instance of <see cref="IPlatformAdapter"/> that provides methods for platform-specific functionality.</returns>
    IPlatformAdapter GetPlatformAdapter();
}