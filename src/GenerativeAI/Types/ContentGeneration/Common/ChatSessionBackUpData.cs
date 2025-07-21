using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GenerativeAI.Core;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a backup object for a chat session containing session-related data and configurations.
/// </summary>
public class ChatSessionBackUpData
{
    /// <summary>
    /// Gets or sets the history of the chat session.
    /// </summary>
    [JsonPropertyName("history")]
    public List<Content> History { get; set; }

    /// <summary>
    /// Gets or sets the content of the last request in the chat session.
    /// </summary>
    [JsonPropertyName("lastRequestContent")]
    public Content? LastRequestContent { get; set; }

    /// <summary>
    /// Gets or sets the content of the last response in the chat session.
    /// </summary>
    [JsonPropertyName("lastResponseContent")]
    public Content? LastResponseContent { get; set; }

    /// <summary>
    /// Gets or sets the configuration used for generating responses.
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// Gets or sets the safety settings for the chat session.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    /// <summary>
    /// Gets or sets the system instructions or guidelines for the chat session.
    /// </summary>
    [JsonPropertyName("systemInstructions")]
    public string? SystemInstructions { get; set; }

    /// <summary>
    /// Gets or sets the model used in the chat session.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the cached content within the chat session.
    /// </summary>
    [JsonPropertyName("cachedContent")]
    public CachedContent? CachedContent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the JSON mode is enabled.
    /// </summary>
    [JsonPropertyName("useJsonMode")]
    public bool UseJsonMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether grounding is enabled for the chat session.
    /// </summary>
    [JsonPropertyName("useGrounding")]
    public bool UseGrounding { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Google search is enabled for the chat session.
    /// </summary>
    [JsonPropertyName("useGoogleSearch")]
    public bool UseGoogleSearch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the code execution tool is enabled for the chat session.
    /// </summary>
    [JsonPropertyName("useCodeExecutionTool")]
    public bool UseCodeExecutionTool { get; set; }

    /// <summary>
    /// Gets or sets the retrieval tool used in the chat session.
    /// </summary>
    [JsonPropertyName("retrievalTool")]
    public Tool? RetrievalTool { get; set; }

    /// <summary>
    /// Gets or sets the function calling behavior for the chat session.
    /// </summary>
    [JsonPropertyName("functionCallingBehaviour")]
    public FunctionCallingBehaviour FunctionCallingBehaviour { get; set; }

    /// <summary>
    /// Gets or sets the configuration for additional tool usage in the chat session.
    /// </summary>
    [JsonPropertyName("toolConfig")]
    public ToolConfig? ToolConfig { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatSessionBackUpData"/> class with the specified required properties.
    /// </summary>
    /// <param name="history">The history of the chat session.</param>
    /// <param name="model">The model used in the chat session.</param>
    /// <param name="functionCallingBehaviour">The function calling behavior for the chat session.</param>
    public ChatSessionBackUpData(List<Content> history, string model, FunctionCallingBehaviour functionCallingBehaviour)
    {
        History = history;
        Model = model;
        FunctionCallingBehaviour = functionCallingBehaviour;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatSessionBackUpData"/> class for JSON deserialization.
    /// </summary>
    public ChatSessionBackUpData() : this(new List<Content>(), "",new FunctionCallingBehaviour()) { }
}