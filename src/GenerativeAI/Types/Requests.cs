using GenerativeAI.Tools;

namespace GenerativeAI.Types
{
    /// <summary>
    /// Base parameters for a number of methods.
    /// </summary>
    public class BaseParams
    {
        public SafetySetting[]? SafetySettings { get; set; }
        public GenerationConfig? GenerationConfig { get; set; }
    }

    /// <summary>
    /// Params passed to {@link GoogleGenerativeAI.getGenerativeModel}.
    /// </summary>
    public class ModelParams : BaseParams
    {
        public string? Model { get; set; }
    }

    /// <summary>
    /// Request sent to `generateContent` endpoint.
    /// </summary>
    public class GenerateContentRequest : BaseParams
    {
        public Content[]? Contents { get; set; }
        public List<GenerativeAITool> Tools { get; set; }
    }

    /// <summary>
    /// Params for {@link GenerativeModel.startChat}.
    /// </summary>
    public class StartChatParams : BaseParams
    {
        public InputContent[]? History { get; set; }
    }

    /// <summary>
    /// Params for calling {@link GenerativeModel.countTokens}
    /// </summary>
    public class CountTokensRequest
    {
        public Content[]? Contents
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Params for calling {@link GenerativeModel.embedContent}
    /// </summary>
    public class EmbedContentRequest
    {
        public Content? Content { get; set; }
        public TaskType TaskType { get; set; }
        public string? Title { get; set; }
    }

    public class BatchEmbedContentsRequest
    {
        public EmbedContentRequest[]? Requests { get; set; }
    }

    /// <summary>
    /// Config options for content-related requests
    /// </summary>
    public class GenerationConfig
    {
        public int? CandidateCount { get; set; }
        public string[]? StopSequences { get; set; }
        public int? MaxOutputTokens { get; set; }
        public double? Temperature { get; set; }
        public double? TopP { get; set; }
        public double? TopK { get; set; }
    }

    /// <summary>
    /// Safety setting that can be sent as part of request parameters.
    /// </summary>
    public class SafetySetting
    {
        public HarmCategory Category { get; set; }
        public HarmBlockThreshold Threshold { get; set; }
    }
}
