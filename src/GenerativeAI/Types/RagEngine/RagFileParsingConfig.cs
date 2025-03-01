using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the parsing config for RagFiles.
/// </summary>
public class RagFileParsingConfig
{
    /// <summary>
    /// The Advanced Parser to use for RagFiles.
    /// </summary>
    [JsonPropertyName("advancedParser")]
    public RagFileParsingConfigAdvancedParser? AdvancedParser { get; set; }

    /// <summary>
    /// The Layout Parser to use for RagFiles.
    /// </summary>
    [JsonPropertyName("layoutParser")]
    public RagFileParsingConfigLayoutParser? LayoutParser { get; set; }

    /// <summary>
    /// The LLM Parser to use for RagFiles.
    /// </summary>
    [JsonPropertyName("llmParser")]
    public RagFileParsingConfigLlmParser? LlmParser { get; set; }

    /// <summary>
    /// Whether to use advanced PDF parsing.
    /// </summary>
    [JsonPropertyName("useAdvancedPdfParsing")]
    [System.Obsolete]
    public bool? UseAdvancedPdfParsing { get; set; }
}