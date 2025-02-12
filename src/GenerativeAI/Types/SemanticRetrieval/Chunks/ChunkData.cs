using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Extracted data that represents the <see cref="Chunk"/> content.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/chunks#ChunkData">See Official API Documentation</seealso>
public class ChunkData
{
    /// <summary>
    /// The <see cref="Chunk"/> content as a string. The maximum number of tokens per chunk is 2043.
    /// </summary>
    [JsonPropertyName("stringValue")]
    public string? StringValue { get; set; }
}