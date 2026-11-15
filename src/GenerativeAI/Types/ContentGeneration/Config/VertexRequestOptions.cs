using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Model Armor configuration for sanitizing prompts and responses. Vertex AI only.
/// </summary>
public class ModelArmorConfig
{
    /// <summary>
    /// Name of the Model Armor template used to sanitize prompts.
    /// </summary>
    [JsonPropertyName("promptTemplateName")]
    public string? PromptTemplateName { get; set; }

    /// <summary>
    /// Name of the Model Armor template used to sanitize responses.
    /// </summary>
    [JsonPropertyName("responseTemplateName")]
    public string? ResponseTemplateName { get; set; }
}

/// <summary>
/// Where to write generated output. Vertex AI only.
/// </summary>
public class GenerationOutputConfig
{
    /// <summary>
    /// Cloud Storage URI prefix that generated output is written under, for example <c>gs://bucket/path/</c>.
    /// </summary>
    [JsonPropertyName("outputUriPrefix")]
    public string? OutputUriPrefix { get; set; }
}
