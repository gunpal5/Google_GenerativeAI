using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Representation of an audio chunk.
/// </summary>
public class AudioChunk
{
    /// <summary>
    /// Raw bytes of audio data.
    /// </summary>
    [JsonPropertyName("data")]
    public byte[]? Data { get; set; }

    /// <summary>
    /// MIME type of the audio chunk.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Prompts and config used for generating this audio chunk.
    /// </summary>
    [JsonPropertyName("sourceMetadata")]
    public LiveMusicSourceMetadata? SourceMetadata { get; set; }
}
