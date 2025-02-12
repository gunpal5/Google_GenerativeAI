using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Grounding chunk.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#GroundingChunk">See Official API Documentation</seealso> 
public class GroundingChunk
{
    /// <summary>
    /// Grounding chunk from the web.
    /// </summary>
    [JsonPropertyName("web")]
    public Web? Web { get; set; }
}