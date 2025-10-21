using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Messages sent by the client in the LiveMusicClientMessage call.
/// </summary>
public class LiveMusicClientMessage
{
    /// <summary>
    /// Message to be sent in the first (and only in the first) `LiveMusicClientMessage`.
    /// Clients should wait for a `LiveMusicSetupComplete` message before sending any additional messages.
    /// </summary>
    [JsonPropertyName("setup")]
    public LiveMusicClientSetup? Setup { get; set; }

    /// <summary>
    /// User input to influence music generation.
    /// </summary>
    [JsonPropertyName("clientContent")]
    public LiveMusicClientContent? ClientContent { get; set; }

    /// <summary>
    /// Configuration for music generation.
    /// </summary>
    [JsonPropertyName("musicGenerationConfig")]
    public LiveMusicGenerationConfig? MusicGenerationConfig { get; set; }

    /// <summary>
    /// Playback control signal for the music generation.
    /// </summary>
    [JsonPropertyName("playbackControl")]
    public LiveMusicPlaybackControl? PlaybackControl { get; set; }
}
