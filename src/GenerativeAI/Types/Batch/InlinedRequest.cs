using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents an inlined request for batch processing without external storage.
/// </summary>
public class InlinedRequest
{
    /// <summary>
    /// Required. The name of the model to use for generating the completion.
    /// Format: models/{model}
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Required. The content of the current conversation with the model.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content>? Contents { get; set; }

    /// <summary>
    /// Optional. Developer set system instruction.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// Optional. Configuration options for model generation and outputs.
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// Optional. A list of unique SafetySetting instances for blocking unsafe content.
    /// </summary>
    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    /// <summary>
    /// Optional. A list of Tools the model may use to generate the next response.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }

    /// <summary>
    /// Optional. Tool configuration for any Tool specified in the request.
    /// </summary>
    [JsonPropertyName("toolConfig")]
    public ToolConfig? ToolConfig { get; set; }

    /// <summary>
    /// Optional. The metadata to be associated with the request.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Optional. The cached content to use as context for this request.
    /// </summary>
    [JsonPropertyName("cachedContent")]
    public string? CachedContent { get; set; }
}
