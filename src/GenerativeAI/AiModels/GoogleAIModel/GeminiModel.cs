using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// The GeminiModel class is a specialized implementation of the GenerativeModel, designed to provide
/// capabilities for generative AI tasks. It supports multiple constructors for flexibility, allowing users
/// to configure the model according to their specific requirements, including platform, API key, configuration,
/// safety settings, and various other parameters.
/// </summary>
/// <remarks>
/// This class extends the functionality of the GenerativeModel and exposes additional configuration options,
/// such as file management through the <see cref="FileClient"/> property. The GeminiModel is intended to
/// support diverse generative AI use-cases.
/// </remarks>
public partial class GeminiModel:GenerativeModel
{
    /// Represents a specialized generative model, derived from the base functionality of the GenerativeModel class.
    /// This model is used for tasks leveraging generative AI capabilities, with additional configuration and safety settings
    /// as needed.
    /// Constructors allow instantiation with various parameters including platform adapters, API keys, files,
    /// generation configurations, safety settings, and optional HTTP clients or loggers.
    /// Features:
    /// - Supports initialization via multiple constructors with varying parameters.
    /// - Provides access to file clients for file-related tasks.
    /// - Integrates with generation configuration and safety settings for model customization.
    public GeminiModel(IPlatformAdapter platform, string model, GenerationConfig? config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, model, config, safetySettings, systemInstruction, httpClient, logger)
    {
        InitClients();
    }

    /// Represents a specialized generative model designed for handling tasks requiring generative AI capabilities.
    /// The GeminiModel class extends the GenerativeModel base class, providing various constructor overloads to
    /// facilitate flexible initialization and usage depending on the provided configurations and parameters.
    /// - Supports integration with platform adapters, API keys, and HTTP clients.
    /// - Allows customization through generation configurations, safety settings, and system instructions.
    /// - Provides methods for managing files by exposing a FileClient property for file-related operations.
    public GeminiModel(string apiKey, ModelParams modelParams, HttpClient? client = null, ILogger? logger = null) : base(apiKey, modelParams, client, logger)
    {
        InitClients();
    }

    // /// Represents a specialized implementation of the GenerativeModel class that focuses on tasks utilizing the Gemini AI platform.
    // /// The GeminiModel class provides constructors supporting a variety of initialization scenarios for different application requirements.
    // /// It integrates configuration settings, safety mechanisms, and file operations support for handling generative tasks with customization.
    // /// Features:
    // /// - Enables instantiation using platform adapters, API keys, file clients, and other configurable parameters.
    // /// - Includes functionality to work with file-related tasks using the FileClient.
    // /// - Supports advanced model configurations through GenerationConfig and optional safety settings.
    // /// - Provides optional support for features such as system instructions, HTTP clients, and logging capabilities.
    // public GeminiModel(FileClient files, int timeoutForFileStateCheck, string apiKey, string model, GenerationConfig? config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null, HttpClient? httpClient = null, ILogger? logger = null) : base(apiKey, model, config, safetySettings, systemInstruction, httpClient, logger)
    // {
    //     InitClients();
    // }

    
    private void InitClients()
    {
        Files = new FileClient(this.Platform, this.HttpClient, this.Logger);
    }

    /// <summary>
    /// Represents a property to interact with files associated with the GeminiModel.
    /// </summary>
    /// <remarks>
    /// This property is used to access and manage files through the FileClient, allowing
    /// operations such as uploading, retrieving, deleting, and listing files.
    /// </remarks>
    public FileClient Files { get; set; }

  
}