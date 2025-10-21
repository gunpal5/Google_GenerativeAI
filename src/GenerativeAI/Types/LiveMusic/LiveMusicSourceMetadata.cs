using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Prompts and config used for generating this audio chunk.
/// </summary>
public class LiveMusicSourceMetadata
{
    /// <summary>
    /// Weighted prompts for generating this audio chunk.
    /// </summary>
    [JsonPropertyName("clientContent")]
    public LiveMusicClientContent? ClientContent { get; set; }

    /// <summary>
    /// Music generation config for generating this audio chunk.
    /// </summary>
    [JsonPropertyName("musicGenerationConfig")]
    public LiveMusicGenerationConfig? MusicGenerationConfig { get; set; }
}
