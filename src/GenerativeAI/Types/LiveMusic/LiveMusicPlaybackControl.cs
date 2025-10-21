using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The playback control signal to apply to the music generation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<LiveMusicPlaybackControl>))]
public enum LiveMusicPlaybackControl
{
    /// <summary>
    /// This value is unused.
    /// </summary>
    [JsonPropertyName("PLAYBACK_CONTROL_UNSPECIFIED")]
    PLAYBACK_CONTROL_UNSPECIFIED = 0,

    /// <summary>
    /// Start generating the music.
    /// </summary>
    [JsonPropertyName("PLAY")]
    PLAY = 1,

    /// <summary>
    /// Hold the music generation. Use PLAY to resume from the current position.
    /// </summary>
    [JsonPropertyName("PAUSE")]
    PAUSE = 2,

    /// <summary>
    /// Stop the music generation and reset the context (prompts retained).
    /// Use PLAY to restart the music generation.
    /// </summary>
    [JsonPropertyName("STOP")]
    STOP = 3,

    /// <summary>
    /// Reset the context of the music generation without stopping it.
    /// Retains the current prompts and config.
    /// </summary>
    [JsonPropertyName("RESET_CONTEXT")]
    RESET_CONTEXT = 4
}
