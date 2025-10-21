using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Grounding chunk.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#GroundingChunk">See Official API Documentation</seealso>
public class GroundingChunk
{
    /// <summary>
    /// Grounding chunk from Google Maps.
    /// </summary>
    [JsonPropertyName("maps")]
    public GroundingChunkMaps? Maps { get; set; }

    /// <summary>
    /// Grounding chunk from context retrieved by the retrieval tools.
    /// </summary>
    [JsonPropertyName("retrievedContext")]
    public GroundingChunkRetrievedContext? RetrievedContext { get; set; }

    /// <summary>
    /// Grounding chunk from the web.
    /// </summary>
    [JsonPropertyName("web")]
    public Web? Web { get; set; }
}