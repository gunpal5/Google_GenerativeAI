using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents token counting info for a single modality.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#Modality">See Official API Documentation</seealso> 
public class ModalityTokenCount
{
    /// <summary>
    /// The modality associated with this token count.
    /// </summary>
    [JsonPropertyName("modality")]
    public Modality Modality { get; set; }

    /// <summary>
    /// Number of tokens.
    /// </summary>
    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; set; }
}