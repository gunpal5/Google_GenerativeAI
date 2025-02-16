using System.Text.Json.Serialization;
using GenerativeAI.Core;

namespace GenerativeAI.Types;

/// <summary>
/// Request to generate content.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tokens#request-body">See Official API Documentation</seealso> 
public class GenerateContentRequest:IContentsRequest
{
    /// <summary>
    /// Required. The content of the current conversation with the model.
    /// For single-turn queries, this is a single instance. For multi-turn queries like
    /// <see href="https://ai.google.dev/gemini-api/docs/text-generation#chat">chat</see>,
    /// this is a repeated field that contains the conversation history and the latest request.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content> Contents { get; set; } = new List<Content>();

    /// <summary>
    /// Optional. A list of <see cref="Tool"/>s the <c>Model</c> may use to generate the next response.
    /// A <see cref="Tool"/> is a piece of code that enables the system to interact with external systems
    /// to perform an action, or set of actions, outside of knowledge and scope of the <c>Model</c>.
    /// Supported <see cref="Tool"/>s are <c>Function</c> and <c>codeExecution</c>. Refer to the
    /// <see href="https://ai.google.dev/gemini-api/docs/function-calling">Function calling</see> and the
    /// <see href="https://ai.google.dev/gemini-api/docs/code-execution">Code execution</see> guides to learn more.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    /// <summary>
    /// Optional. Tool configuration for any <see cref="Tool"/> specified in the request.
    /// Refer to the <see href="https://ai.google.dev/gemini-api/docs/function-calling#function_calling_mode">Function calling guide</see>
    /// for a usage example.
    /// </summary>
    [JsonPropertyName("toolConfig")]
    public ToolConfig? ToolConfig { get; set; }

    /// <summary>
    /// Optional. A list of unique <see cref="SafetySetting"/> instances for blocking unsafe content.
    /// This will be enforced on the <see cref="GenerateContentRequest.Contents">GenerateContentRequest.Contents</see> and
    /// <see cref="GenerateContentResponse.Candidates">GenerateContentResponse.Candidates</see>. There should not be more than one setting for each
    /// <see cref="HarmCategory">SafetyCategory</see> type. The API will block any contents and responses that fail to meet
    /// the thresholds set by these settings. This list overrides the default settings for each
    /// <see cref="HarmCategory">SafetyCategory</see> specified in the <see cref="SafetySettings"/>. If there is no <see cref="SafetySetting"/>
    /// for a given <see cref="HarmCategory">SafetyCategory</see> provided in the list, the API will use the default safety
    /// setting for that category. Harm categories HARM_CATEGORY_HATE_SPEECH,
    /// HARM_CATEGORY_SEXUALLY_EXPLICIT, HARM_CATEGORY_DANGEROUS_CONTENT, HARM_CATEGORY_HARASSMENT,
    /// HARM_CATEGORY_CIVIC_INTEGRITY are supported. Refer to the
    /// <see href="https://ai.google.dev/gemini-api/docs/safety-settings">guide</see> for detailed
    /// information on available safety settings. Also refer to the
    /// <see href="https://ai.google.dev/gemini-api/docs/safety-guidance">Safety guidance</see> to
    /// learn how to incorporate safety considerations in your AI applications.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    /// <summary>
    /// Optional. Developer set
    /// <see href="https://ai.google.dev/gemini-api/docs/system-instructions">system instruction(s)</see>.
    /// Currently, text only.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// Optional. Configuration options for model generation and outputs.
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// Optional. The name of the content
    /// <see href="https://ai.google.dev/gemini-api/docs/caching">cached</see> to use as context
    /// to serve the prediction. Format: <c>cachedContents/{cachedContent}</c>
    /// </summary>
    [JsonPropertyName("cachedContent")]
    public string? CachedContent { get; set; }

    /// <summary>
    /// Represents a request to generate content.
    /// This class contains parameters required to define the specifics of content generation,
    /// including inputs, tools, configurations, and safety constraints.
    /// </summary>
    public GenerateContentRequest()
    {
        
    }

    /// <summary>
    /// Represents a request used to generate content.
    /// This class includes properties like content, tools, configurations,
    /// safety settings, and instructions required for the content generation process.
    /// </summary>
    public GenerateContentRequest(List<Content> contents, List<Tool>? tools = null, ToolConfig? toolConfig = null, List<SafetySetting>? safetySettings = null, Content? systemInstruction = null, GenerationConfig? generationConfig = null, string? cachedContent = null)
    {
        Contents = contents;
        Tools = tools;
        ToolConfig = toolConfig;
        SafetySettings = safetySettings;
        SystemInstruction = systemInstruction;
        GenerationConfig = generationConfig;
        CachedContent = cachedContent;
    }

    /// <summary>
    /// Represents a request to generate content.
    /// This class enables the definition of content generation specifics through properties
    /// such as the content details, tools, tool configurations, safety settings, and system instructions.
    /// Optional configurations allow customization and adaptation of the generation process.
    /// </summary>
    public GenerateContentRequest(Content content)
    {
        Contents = new List<Content>() { content };
    }
    
}