using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Wrapper for inlined request in batch processing (Google AI format).
/// </summary>
public class InlinedRequestWrapper
{
    /// <summary>
    /// The actual request containing model, contents, and configuration.
    /// </summary>
    [JsonPropertyName("request")]
    public InlinedRequestBody? Request { get; set; }

    /// <summary>
    /// Optional metadata to be associated with the request.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// The actual request body for batch inlined requests.
/// </summary>
public class InlinedRequestBody
{
    /// <summary>
    /// The name of the model to use.
    /// Format: models/{model}
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// The content of the current conversation with the model.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content>? Contents { get; set; }

    /// <summary>
    /// Optional configuration options for model generation.
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// Optional safety settings for blocking unsafe content.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    /// <summary>
    /// Optional tools the model may use.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    /// <summary>
    /// Optional tool configuration.
    /// </summary>
    [JsonPropertyName("toolConfig")]
    public ToolConfig? ToolConfig { get; set; }

    /// <summary>
    /// Optional system instruction.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// Optional cached content reference.
    /// </summary>
    [JsonPropertyName("cachedContent")]
    public string? CachedContent { get; set; }
}
