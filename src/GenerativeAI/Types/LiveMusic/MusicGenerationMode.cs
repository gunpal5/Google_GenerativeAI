using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The mode of music generation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MusicGenerationMode>))]
public enum MusicGenerationMode
{
    /// <summary>
    /// Rely on the server default generation mode.
    /// </summary>
    [JsonPropertyName("MUSIC_GENERATION_MODE_UNSPECIFIED")]
    MUSIC_GENERATION_MODE_UNSPECIFIED = 0,

    /// <summary>
    /// Steer text prompts to regions of latent space with higher quality music.
    /// </summary>
    [JsonPropertyName("QUALITY")]
    QUALITY = 1,

    /// <summary>
    /// Steer text prompts to regions of latent space with a larger diversity of music.
    /// </summary>
    [JsonPropertyName("DIVERSITY")]
    DIVERSITY = 2,

    /// <summary>
    /// Steer text prompts to regions of latent space more likely to generate music with vocals.
    /// </summary>
    [JsonPropertyName("VOCALIZATION")]
    VOCALIZATION = 3
}
