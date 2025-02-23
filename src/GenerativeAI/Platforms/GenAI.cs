using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// Represents the base class for generative AI services. This class provides a framework for generative AI capabilities, including
/// the management of AI models, creation of generative and embedding models, and interacting with generative AI infrastructure.
/// </summary>
public abstract class GenAI
{
    /// <summary>
    /// Gets the underlying platform adapter instance utilized by the <see cref="GoogleAi"/> class.
    /// This property provides essential platform-specific functionality, such as URL generation,
    /// authorization handling, and API versioning, enabling seamless communication with Google's generative AI services.
    /// </summary>
    protected IPlatformAdapter Platform { get; }

    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> instance used to perform HTTP requests
    /// to Google's AI platform. This property allows for custom configuration of network communication,
    /// including request headers, timeouts, and other HTTP-specific settings, enabling seamless
    /// interaction with Google's generative and embedding models.
    /// </summary>
    protected HttpClient? HttpClient { get; }

    /// <summary>
    /// Gets the logger instance used by the <see cref="GoogleAi"/> class for logging diagnostic messages,
    /// errors, and other runtime information. This property provides a mechanism to track and debug
    /// the internal operations of the class and its related components.
    /// </summary>
    protected ILogger? Logger { get; }

    /// <summary>
    /// Gets the instance of the <see cref="ModelClient"/> class associated with the <see cref="GoogleAi"/> instance.
    /// This property allows access to functionality for interacting with the Gemini API Models endpoint, enabling
    /// operations such as retrieving and listing AI models.
    /// </summary>
    public ModelClient ModelClient { get; }


    protected GenAI(IPlatformAdapter platformAdapter, HttpClient? client = null, ILogger? logger = null)
    {
        this.Platform = platformAdapter;
        this.HttpClient = client;
        this.Logger = logger;
        this.ModelClient = new ModelClient(this.Platform, this.HttpClient, this.Logger);
    }

    /// <summary>
    /// Creates and initializes a generative model for use with Google's generative AI platform.
    /// This method allows the specification of model configuration, safety settings,
    /// and system-level instructions for controlling model behavior.
    /// </summary>
    /// <param name="modelName">The name of the generative model to initialize.</param>
    /// <param name="config">Optional configuration settings for generation,
    /// such as temperature, max tokens, or other generation parameters.</param>
    /// <param name="safetyRatings">Optional safety settings to define behavior
    /// constraints or safety parameters for the generative model.</param>
    /// <param name="systemInstruction">Optional system-wide instruction to apply
    /// when the model processes requests or generates responses.</param>
    /// <returns>An instance of <see cref="GenerativeModel"/> configured with the specified parameters.</returns>
    public virtual GenerativeModel CreateGenerativeModel(string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null, string? systemInstruction = null)
    {
        return new GenerativeModel(this.Platform, modelName, config, safetyRatings, systemInstruction, this.HttpClient,
            this.Logger);
    }

    /// <summary>
    /// Creates and initializes an embedding model for use in embedding-related AI tasks.
    /// The created model is configured with the provided model name and integrates with the underlying platform adapter.
    /// </summary>
    /// <param name="modelName">The name of the embedding model to initialize, used to identify the specific AI model for embeddings.</param>
    /// <returns>An instance of <see cref="EmbeddingModel"/> initialized with the specified model name and platform configurations.</returns>
    public EmbeddingModel CreateEmbeddingModel(string modelName)
    {
        return new EmbeddingModel(this.Platform, modelName, this.HttpClient, this.Logger);
    }

    /// <summary>
    /// Asynchronously retrieves a list of AI models available in Google's AI platform.
    /// This method supports pagination to fetch models in batches using the specified page size
    /// and page token, enabling efficient handling of large model datasets.
    /// </summary>
    /// <param name="pageSize">Optional parameter specifying the maximum number of models to retrieve per page. Defaults to null for the API's standard page size.</param>
    /// <param name="pageToken">Optional parameter indicating the token for the next page of results, as provided in a previous response.</param>
    /// <param name="cancellationToken">Optional parameter for propagating cancellation signals, allowing the operation to be canceled if needed.</param>
    /// <returns>A task representing the asynchronous operation, which contains a <see cref="ListModelsResponse"/> with the retrieved models and pagination details.</returns>
    public async Task<ListModelsResponse> ListModelsAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        return await this.ModelClient.ListModelsAsync(pageSize, pageToken, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a specific AI model by its name asynchronously from the Google's AI platform.
    /// </summary>
    /// <param name="modelName">The unique name of the model to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the requested model details as a <see cref="Model"/> object.</returns>
    public async Task<Model> GetModelAsync(string modelName,
        CancellationToken cancellationToken = default)
    {
        return await this.ModelClient.GetModelAsync(modelName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the platform adapter associated with this instance.
    /// The platform adapter provides core functionalities, such as authorization,
    /// URL creation, and API version management, for interacting with the underlying platform.
    /// </summary>
    /// <returns>The platform adapter instance implementing <see cref="IPlatformAdapter"/>.</returns>
    public IPlatformAdapter GetPlatformAdapter()
    {
        return this.Platform;
    }

    /// <summary>
    /// Creates and initializes an image generation model for use with the Imagen image generation API.
    /// This method configures the model to generate images based on provided prompts or inputs.
    /// </summary>
    /// <param name="modelName">The name of the image generation model to initialize.</param>
    /// <returns>An instance of <see cref="ImagenModel"/> configured for generating images using the specified model name.</returns>
    public ImagenModel CreateImageModel(string modelName)
    {
        return new ImagenModel(this.Platform, modelName, this.HttpClient, this.Logger);
    }
}