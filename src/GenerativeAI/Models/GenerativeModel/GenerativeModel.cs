using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Constants;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Models
{
    /// <summary>
    /// A Generative AI Model that extends BaseModel
    /// </summary>
    public partial class GenerativeModel : BaseModel
    {
        #region Properties

        public string Model { get; set; } = "gemini-1.5-flash";
        public GenerationConfig Config { get; set; } = new GenerationConfig { Temperature = 0.8, MaxOutputTokens = 2048 };
        public List<SafetySetting>? SafetySettings { get; set; } = null;
        

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
        }
        
        public GenerativeModel(string apiKey, ModelParams modelParams, HttpClient? client = null, ILogger? logger = null):this(new GoogleAIPlatformAdapter(apiKey),modelParams.Model??GeminiConstants.DefaultGeminiModel,modelParams.GenerationConfig,modelParams.SafetySettings,modelParams.SystemInstruction,client,logger)
        {
            
        }
        
        public GenerativeModel(string apiKey,string model,
            GenerationConfig config = null,
            ICollection<SafetySetting>? safetySettings = null,
            string? systemInstruction = null,
            HttpClient? httpClient = null,
            ILogger? logger = null):this(new GoogleAIPlatformAdapter(apiKey),model,config,safetySettings,systemInstruction,httpClient,logger)
        {
           
        }


        private void Initialize(IPlatformAdapter platformAdapter, string model, GenerationConfig config, ICollection<SafetySetting>? safetySettings, string? systemInstruction)
        {
            Model = model;
            Config = config;
            if (safetySettings != null) SafetySettings = safetySettings.ToList();
            SystemInstruction = systemInstruction;
        }

        #endregion

       

        
    }
}