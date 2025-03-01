using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the advanced parsing for RagFiles.
/// </summary>
public class RagFileParsingConfigAdvancedParser
{
    /// <summary>
    /// Whether to use advanced PDF parsing.
    /// </summary>
    [JsonPropertyName("useAdvancedPdfParsing")]
    public bool? UseAdvancedPdfParsing { get; set; }
}