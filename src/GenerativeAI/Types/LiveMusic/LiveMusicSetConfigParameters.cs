using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Parameters for setting config for the live music API.
/// </summary>
public class LiveMusicSetConfigParameters
{
    /// <summary>
    /// Configuration for music generation.
    /// </summary>
    [JsonPropertyName("musicGenerationConfig")]
    public LiveMusicGenerationConfig? MusicGenerationConfig { get; set; }
}
