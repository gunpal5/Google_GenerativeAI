using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// The GoogleAI class serves as the main entry point for interacting with Google's AI models.
/// It provides methods to create generative and embedding models, as well as manage and retrieve model details.
/// This class integrates Google's AI platform capabilities by utilizing appropriate models
/// and configurations to handle generative AI tasks, embeddings, and other related operations.
/// </summary>
public class GoogleAi : GenAI,IGenerativeAI
{
    /// <summary>
    /// Implements a client adapter for Google's AI platform to facilitate integration with its generative AI services.
    /// Provides configuration and methods for managing credentials, setting API versions, creating task URLs,
    /// and authorizing requests for secure access to Google's AI APIs.
    /// </summary>
    public GoogleAi(string? apiKey =null, string? accessToken = null, HttpClient? client = null, ILogger? logger = null) :
        this(new GoogleAIPlatformAdapter(apiKey, accessToken:accessToken), client, logger)
    {
    }

    /// <summary>
    /// Represents a specific implementation of the GenAI base class designed for Google's AI platform.
    /// Offers functionality to interact with Google's generative AI and machine learning models.
    /// Provides support for initialization with various parameters such as API keys, access tokens, HTTP client, and logging.
    /// </summary>
    public GoogleAi(IPlatformAdapter adapter, HttpClient? client = null, ILogger? logger = null) : base(adapter, client,
        logger)
    {
    }

    public override GenerativeModel CreateGenerativeModel(string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null, string? systemInstruction = null)
    {
        return new GeminiModel(this.Platform, modelName, config, safetyRatings, systemInstruction, this.HttpClient,
            this.Logger);
    }
    
    public GeminiModel CreateGeminiModel(string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null, string? systemInstruction = null)
    {
        return new GeminiModel(this.Platform, modelName, config, safetyRatings, systemInstruction, this.HttpClient,
            this.Logger);
    }
}