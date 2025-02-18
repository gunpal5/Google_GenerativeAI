using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// VertexAIModel is a specialized class for interacting with Google's Vertex AI generative models.
/// It extends the functionality of the GenerativeModel class and serves as a concrete implementation
/// for initializing and utilizing Vertex AI's capabilities for content generation tasks.
/// </summary>
/// <remarks>
/// VertexAIModel provides constructors to configure the model with various parameters such as
/// API keys, model specifications, and optional settings like safety rules or system-wide instructions.
/// It allows integration with Vertex AI for advanced generative use cases.
/// </remarks>
/// <example>
/// This class does not include direct examples or usage. Refer to the GenerativeModel class
/// for generalized behavior, or see related documentation for leveraging the Vertex AI platform.
/// </example>
/// <seealso cref="GenerativeModel" />
/// <seealso cref="BaseClient" />
/// <seealso cref="IPlatformAdapter" />
public class VertexAIModel:GenerativeModel
{
    /// Represents a model within the Vertex AI ecosystem designed to perform generative AI tasks.
    /// This class extends the base GenerativeModel and provides functionality to connect with
    /// Vertex AI services using various configurations and parameters.
    public VertexAIModel(IPlatformAdapter platform, string model, GenerationConfig? config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, model, config, safetySettings, systemInstruction, httpClient, logger)
    {
    }

    /// Represents a model within the Vertex AI ecosystem for performing advanced generative AI tasks.
    /// This class extends the GenerativeModel base class and facilitates communication with the Vertex AI platform
    /// by leveraging various configuration, safety, and platform-specific options.
    public VertexAIModel(string? model = null, string? projectId = null, string? region = null, string? accessToken = null, GenerationConfig? config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null,  bool expressMode = false, string? apiKey=null, IGoogleAuthenticator? authenticator =null, HttpClient? httpClient=null, ILogger? logger =null):this(new VertextPlatformAdapter(projectId,region,expressMode,apiKey,accessToken,authenticator:authenticator,logger:logger),model??EnvironmentVariables.GOOGLE_AI_MODEL??GoogleAIModels.DefaultGeminiModel,config,safetySettings,systemInstruction,httpClient,logger)
    {
        
    }

    

    // /// Represents a specialized model in the Vertex AI platform for performing generative AI tasks.
    // /// This class inherits from GenerativeModel and provides multiple constructors for flexible integration
    // /// with Vertex AI services, supporting configurations such as API keys, generation settings, safety settings,
    // /// and client or logger dependencies.
    // public VertexAIModel(string apiKey, ModelParams modelParams, HttpClient? client = null, ILogger? logger = null) : base(apiKey, modelParams, client, logger)
    // {
    // }
    //
    // /// Represents a specialized generative AI model that integrates with Google's Vertex AI platform.
    // /// Inherits from the GenerativeModel class and provides additional constructors tailored to
    // /// Vertex AI-specific configurations. This class supports initializing the model with platform adapters,
    // /// API keys, safety settings, generation configurations, system instructions, and optional logging or HTTP client facilities.
    // public VertexAIModel(string apiKey, string model, GenerationConfig config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null, HttpClient? httpClient = null, ILogger? logger = null) : base(apiKey, model, config, safetySettings, systemInstruction, httpClient, logger)
    // {
    // }
}