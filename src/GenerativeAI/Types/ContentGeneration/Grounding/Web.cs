using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Chunk from the web.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#Web">See Official API Documentation</seealso> 
public class Web
{
    /// <summary>
    /// URI reference of the chunk.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    /// <summary>
    /// Title of the chunk.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}