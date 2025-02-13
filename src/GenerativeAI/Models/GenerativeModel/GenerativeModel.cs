using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI
{
    /// <summary>
    /// A Generative AI Model that extends BaseModel
    /// </summary>
    public partial class GenerativeModel : BaseModel
    {
        #region Properties

        public string? SystemInstruction { get; set; }

        public string Model { get; set; } = GoogleAIModels.DefaultGeminiModel;

        public GenerationConfig Config { get; set; } =
            new GenerationConfig { Temperature = 0.8, MaxOutputTokens = 2048 };

        public List<SafetySetting>? SafetySettings { get; set; } = null;

        public CachedContent? CachedContent { get; set; }

        // Used to show or hide function-call capabilities
        public bool AutoCallFunction { get; set; } = true;
        public bool AutoReplyFunction { get; set; } = true;
        public List<FunctionDeclaration>? Functions { get; set; }
        public bool FunctionEnabled { get; set; } = true;
        public bool AutoHandleBadFunctionCalls { get; set; } = false;

        /// <summary>
        /// Stores the mapping from function name to a delegate that handles the call
        /// </summary>
        public IDictionary<string, Func<string, CancellationToken, Task<string>>> Calls { get; set; }
            = new Dictionary<string, Func<string, CancellationToken, Task<string>>>();

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
            string model,
            GenerationConfig config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null,
            HttpClient? httpClient = null,
            ILogger? logger = null)
            : base(platform, httpClient, logger)
        {
            Initialize(platform, model, config, safetySettings, systemInstruction);
            InitializeClients(platform, httpClient, logger);
        }


        /// <summary>
        /// Represents a generative AI model designed to handle text generation tasks with configurable settings,
        /// platform adapters, safety measures, and advanced functionalities such as function calling and content caching.
        /// </summary>
        /// <param name="platform">The platform adapter facilitating the interaction with generative AI services.</param>
        /// <param name="model">The specific model identifier or name to use for generation tasks.</param>
        /// <param name="config">Optional configuration for text generation, including temperature and token limits.</param>
        /// <param name="safetySettings">Optional safety settings to enforce content generation constraints.</param>
        /// <param name="systemInstruction">Optional instruction guiding the system behavior or initial prompt.</param>
        /// <param name="httpClient">Optional HTTP client for API communication.</param>
        /// <param name="logger">Optional logger for tracking and debugging operations.</param>
        /// <remarks>
        /// This class provides multiple constructors to support customization, enabling seamless integration with
        /// platform-specific APIs, safety configurations, and logging utilities.
        /// </remarks>
        public GenerativeModel(string apiKey, ModelParams modelParams, HttpClient? client = null,
            ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey),
            modelParams.Model ?? GoogleAIModels.DefaultGeminiModel, modelParams.GenerationConfig,
            modelParams.SafetySettings, modelParams.SystemInstruction, client, logger)
        {
        }

        /// <summary>
        /// Represents a generative AI model designed for text generation and related tasks.
        /// </summary>
        /// <remarks>
        /// The class provides constructors with various initialization parameters for customization, including platform integration, model configuration,
        /// and safety settings. It supports additional features like function calling, automatic function management, and caching of generated content.
        /// </remarks>
        /// <param name="platform">The platform adapter integrating with the underlying generative AI service.</param>
        /// <param name="model">The name or identifier of the AI model to be utilized.</param>
        /// <param name="config">Optional configuration defining text generation parameters such as temperature and token limits.</param>
        /// <param name="safetySettings">Optional collection of safety settings to ensure generated content adheres to specific guidelines.</param>
        /// <param name="systemInstruction">Optional instruction specifying the initial guidance or context for the model's behavior.</param>
        /// <param name="httpClient">Optional HTTP client for facilitating API communication with the AI platform.</param>
        /// <param name="logger">Optional logger for capturing diagnostic messages and runtime events.</param>
        /// <param name="apiKey">API key for authenticating with the AI platform when a specific adapter is not provided explicitly.</param>
        /// <param name="modelParams">Object encompassing model settings including name, generation configuration, and safety settings.</param>
        /// <remarks>
        /// The class also includes methods to validate and prepare content generation requests, along with the ability to initialize a conversational chat session.
        /// </remarks>
        public GenerativeModel(string apiKey, string model,
            GenerationConfig config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null,
            HttpClient? httpClient = null,
            ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey), model, config, safetySettings,
            systemInstruction, httpClient, logger)
        {
        }


        private void Initialize(IPlatformAdapter platformAdapter, string model, GenerationConfig config,
            ICollection<SafetySetting>? safetySettings, string? systemInstruction)
        {
            Model = model;
            Config = config;
            if (safetySettings != null) SafetySettings = safetySettings.ToList();
            SystemInstruction = systemInstruction;
        }

        private void InitializeClients(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger)
        {
            Files = new FileClient(platform, httpClient, logger);
        }

        #endregion

        #region Protected and Private methods

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

        private void AddCachedContent(GenerateContentRequest request)
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

        private void AdjustJsonMode(GenerateContentRequest request)
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
        /// Start a Chat Session
        /// </summary>
        /// <param name="startSessionParams">Session Params with Chat History</param>
        /// <returns>ChatSession Object</returns>
        public virtual ChatSession StartChat(List<Content>? history = null,
            GenerationConfig config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null)
        {
            return new ChatSession(history,this.Platform,this.Model,config??this.Config,safetySettings??this.SafetySettings,systemInstruction??this.SystemInstruction,this.HttpClient,this.Logger);
        }
        #endregion
    }
}