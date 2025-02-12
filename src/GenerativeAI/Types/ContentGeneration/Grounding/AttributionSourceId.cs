using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Identifier for the source contributing to this attribution.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#AttributionSourceId">See Official API Documentation</seealso> 
public class AttributionSourceId
{
    /// <summary>
    /// Identifier for an inline passage.
    /// </summary>
    [JsonPropertyName("groundingPassage")]
    public GroundingPassageId? GroundingPassage { get; set; }

    /// <summary>
    /// Identifier for a <see cref="Chunk">Chunk</see> fetched via Semantic Retriever.
    /// </summary>
    [JsonPropertyName("semanticRetrieverChunk")]
    public SemanticRetrieverChunk? SemanticRetrieverChunk { get; set; }
}