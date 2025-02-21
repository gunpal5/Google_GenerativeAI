using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI
{
    /// <summary>
    /// A Generative AI Model that extends BaseModel
    /// </summary>
    public partial class GenerativeModel : BaseModel,IGenerativeModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the system instruction used to guide the behavior of the generative model.
        /// This property defines the overarching context or guideline for content generation,
        /// shaping the responses produced by the model to align with specific requirements or objectives.
        /// </summary>
        public string? SystemInstruction { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the model used for generative tasks.
        /// This property determines the specific model implementation to be used
        /// for content generation and other related operations.
        /// </summary>
        public string Model { get; set; } = GoogleAIModels.DefaultGeminiModel;

        /// <summary>
        /// Gets or sets the configuration for content generation.
        /// This property defines specific parameters, such as temperature and maximum output tokens,
        /// that influence the behavior and output of the generative model. It serves as a central
        /// customization point for tailoring the content creation process.
        /// </summary>
        public GenerationConfig? Config { get; set; }

        /// <summary>
        /// Gets or sets the collection of safety settings that define constraints or limits
        /// on content generation. This property is used to enforce specified safety rules
        /// to ensure the generated content aligns with predetermined guidelines or restrictions.
        /// </summary>
        public List<SafetySetting>? SafetySettings { get; set; } = null;

        /// <summary>
        /// Gets or sets preloaded or previously generated content associated with the generative model.
        /// This property allows for the reuse of specific content, bypassing the need for regeneration,
        /// while ensuring alignment with the current model configuration and constraints.
        /// </summary>
        public CachedContent? CachedContent { get; set; }

        /// <summary>
        /// Gets or sets the client used for managing cached content operations in the generative AI model.
        /// This property provides an interface for interacting with caching-related functionalities,
        /// such as creating, updating, retrieving, or deleting cached content. It helps manage content efficiently
        /// by leveraging the CachingClient within the model's context.
        /// </summary>
        public CachingClient CachingClient { get; set; }
       
        #endregion

        #region Constructors

        /// <summary>
        /// Represents a generative AI model, providing functionality for text generation tasks.
        /// </summary>
        /// <param name="platform">The platform adapter used for the generative AI model integration.</param>
        /// <param name="model">The name or identifier of the model to be used.</param>
        /// <param name="config">Optional configuration for text generation, including settings like temperature and max tokens.</param>
        /// <param name="safetySettings">Optional collection of safety settings to ensure content generation adheres to desired guidelines.</param>
        /// <param name="systemInstruction">Optional instruction for the model, defining the initial prompt or system behavior.</param>
        /// <param name="httpClient">Optional HTTP client for API communication.</param>
        /// <param name="logger">Optional logger instance for diagnostic and logging purposes.</param>
        /// <remarks>
        /// Provides constructors for initializing the model with various configurations and integrates with platform-specific operations.
        /// </remarks>
        public GenerativeModel(
            IPlatformAdapter platform,
            string? model,
            GenerationConfig? config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null,
            HttpClient? httpClient = null,
            ILogger? logger = null)
            : base(platform, httpClient, logger)
        {
            model ??= EnvironmentVariables.GOOGLE_AI_MODEL ?? GoogleAIModels.DefaultGeminiModel;
            
            Initialize(platform, model, config, safetySettings, systemInstruction);
            InitializeClients(platform, httpClient, logger);
        }


        /// <summary>
        /// Represents a generative model designed for handling text generation tasks, offering a range of configurations
        /// and functionalities including platform-specific integration, safety settings, and optional system instructions.
        /// </summary>
        /// <param name="apiKey">The API key required for authenticating requests with the target platform.</param>
        /// <param name="modelParams">Model parameters to specify generation configuration, safety settings, and behavior customization.</param>
        /// <param name="client">Optional HTTP client for managing communication with the generative AI platform.</param>
        /// <param name="logger">Optional logger instance to facilitate diagnostic logging and debugging.</param>
        /// <remarks>
        /// This constructor allows the creation of an instance with default or user-specified configurations, streamlined for
        /// integration with external generative AI services. The underlying implementation leverages adapter-based architecture
        /// for modularity and scalability.
        /// </remarks>
        public GenerativeModel(string apiKey, ModelParams modelParams, HttpClient? client = null,
            ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey),
            modelParams.Model ?? EnvironmentVariables.GOOGLE_AI_MODEL ?? GoogleAIModels.DefaultGeminiModel,
            modelParams.GenerationConfig,
            modelParams.SafetySettings, modelParams.SystemInstruction, client, logger)
        {
        }

        /// <summary>
        /// Represents a generative AI model, extending base functionalities and supporting text generation tasks.
        /// </summary>
        /// <param name="apiKey">The API key used for authenticating requests to the generative AI platform.</param>
        /// <param name="model">Optional identifier or name of the model to be used. Defaults to system environment variables or a platform-specific default model if not provided.</param>
        /// <param name="config">Optional configuration for text generation to customize behavior, such as adjusting temperature or token limits.</param>
        /// <param name="safetySettings">Optional collection of safety settings to enforce content generation rules and guidelines.</param>
        /// <param name="systemInstruction">Optional instruction defining the system's behavior, providing an initial context or prompt.</param>
        /// <param name="httpClient">Optional HTTP client used for executing API communication with the generative platform.</param>
        /// <param name="logger">Optional logger instance for tracking or debugging the model operations and interactions.</param>
        /// <remarks>
        /// Initializes a GenerativeModel instance, leveraging the Google AI platform adapter and enabling advanced text generation capabilities with configurable parameters.
        /// </remarks>
        public GenerativeModel(string apiKey, string? model,
            GenerationConfig? config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null,
            HttpClient? httpClient = null,
            ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey),
            model ?? EnvironmentVariables.GOOGLE_AI_MODEL ?? GoogleAIModels.DefaultGeminiModel, config, safetySettings,
            systemInstruction, httpClient, logger)
        {
        }


        private void Initialize(IPlatformAdapter platformAdapter, string model, GenerationConfig? config,
            ICollection<SafetySetting>? safetySettings, string? systemInstruction)
        {
            Model = model;
            Config = config;
            if (safetySettings != null) SafetySettings = safetySettings.ToList();
            SystemInstruction = systemInstruction;
        }

        /// <summary>
        /// Initializes the necessary clients for the GenerativeModel instance.
        /// </summary>
        /// <param name="platform">The platform adapter used to communicate with the generative AI platform.</param>
        /// <param name="httpClient">Optional HTTP client used for API communication.</param>
        /// <param name="logger">Optional logger instance for logging diagnostic information.</param>
        private void InitializeClients(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger)
        {
            //Files = new FileClient(platform, httpClient, logger);
            CachingClient = new CachingClient(platform, httpClient, logger);
        }

        #endregion

        #region Protected and Private methods

        /// <summary>
        /// Validates the provided GenerateContentRequest to ensure compatibility with the current model configuration.
        /// </summary>
        /// <param name="request">The content generation request containing parameters such as generation configuration.</param>
        /// <remarks>
        /// This method ensures that specific features like grounding, Google Search, or code execution tools
        /// are not used concurrently with incompatible modes such as JSON mode or cached content mode.
        /// It also checks for consistency in the generation configuration, such as the specified response MIME type.
        /// </remarks>
        protected virtual void ValidateGenerateContentRequest(GenerateContentRequest request)
        {
            if (UseJsonMode && (UseGrounding || UseGoogleSearch || UseCodeExecutionTool))
                throw new NotSupportedException(
                    "Json mode does not support grounding or google search or code execution tool");
            if (CachedContent != null && (UseGrounding || UseGoogleSearch || UseCodeExecutionTool))
                throw new NotSupportedException(
                    "Cached content mode does not support the use of grounding, Google Search, or code execution tools. Please disable these features.");

            if (request.GenerationConfig != null)
            {
                if (request.GenerationConfig.ResponseMimeType == "application/json")
                {
                    if ((UseGrounding || UseGoogleSearch || UseCodeExecutionTool))
                        throw new NotSupportedException(
                            "Json mode does not support grounding or google search or code execution tool");
                }
            }
        }

        /// <summary>
        /// Prepares the <see cref="GenerateContentRequest"/> for content generation by populating global properties,
        /// configuring JSON mode, adding tools, cached content, and validating the request.
        /// </summary>
        /// <param name="request">The content generation request to be prepared with required settings and validations.</param>
        /// <remarks>
        /// This method ensures that the request is appropriately configured with default global parameters,
        /// tools, and any necessary system-level instructions before proceeding with content generation.
        /// </remarks>
        protected virtual void PrepareRequest(GenerateContentRequest request)
        {
            //Add Global Properties
            request.GenerationConfig ??= Config;
            request.SafetySettings ??= SafetySettings;
            request.SystemInstruction ??= RequestExtensions.FormatSystemInstruction(this.SystemInstruction);
            //Json Mode
            AdjustJsonMode(request);
            //Add Tools
            AddTools(request);
            //Add Cached Content
            AddCachedContent(request);
            //Validate Request
            ValidateGenerateContentRequest(request);
        }

        /// <summary>
        /// Adds cached content to the generate content request, ensuring the model and request contents align with the cached data.
        /// </summary>
        /// <param name="request">The generate content request to which cached content is added.</param>
        /// <remarks>
        /// Cached content includes predefined responses or data related to the current model.
        /// If cached content is present, it sets the cached contents and disables additional configurations related to tools or system instructions.
        /// An exception is thrown if the cached content's model does not match the generative model.
        /// </remarks>
        protected void AddCachedContent(GenerateContentRequest request)
        {
            if (CachedContent != null)
            {
                if (Model != CachedContent.Model)
                    throw new ArgumentException("CachedContent model must match the model of the GenerativeModel");

                request.CachedContent = CachedContent.Name;
                if (CachedContent.Contents != null)
                {
                    request.Contents.InsertRange(0, CachedContent.Contents);
                }

                // Cached Content does not support tools, tool configurations, or system instructions
                request.Tools = null;
                request.ToolConfig = null;
                request.SystemInstruction = null;
            }
        }

        /// <summary>
        /// Adjusts the response mime type to JSON mode if enabled in the configuration.
        /// </summary>
        /// <param name="request">The generate content request being processed, which includes configuration and settings for content generation.</param>
        protected void AdjustJsonMode(GenerateContentRequest request)
        {
            if (UseJsonMode)
            {
                if (request.GenerationConfig == null)
                    request.GenerationConfig = new GenerationConfig();
                if (!string.IsNullOrEmpty(request.GenerationConfig.ResponseMimeType))
                    request.GenerationConfig.ResponseMimeType = "application/json";
            }
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Starts a new chat session with the given parameters and configuration.
        /// </summary>
        /// <param name="history">Optional chat history to be included in the new session.</param>
        /// <param name="config">Optional configuration for text generation, including parameters such as temperature and maximum tokens.</param>
        /// <param name="safetySettings">Optional collection of safety settings to ensure the chat session adheres to defined guidelines.</param>
        /// <param name="systemInstruction">Optional instruction defining the system's behavior or context for the chat session.</param>
        /// <returns>A new instance of the ChatSession class representing the initiated chat session.</returns>
        public virtual ChatSession StartChat(List<Content>? history = null,
            GenerationConfig? config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null)
        {
            return new ChatSession(history,this.Platform,this.Model,config??this.Config,safetySettings??this.SafetySettings,systemInstruction??this.SystemInstruction,this.HttpClient,this.Logger);
        }

        
        
        #endregion
    }
}