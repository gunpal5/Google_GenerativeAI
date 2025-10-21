using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for music generation.
/// </summary>
public class LiveMusicGenerationConfig
{
    /// <summary>
    /// Controls the variance in audio generation. Higher values produce higher variance. Range is [0.0, 3.0].
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Controls how the model selects tokens for output. Samples the topK tokens with the highest probabilities. Range is [1, 1000].
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    /// <summary>
    /// Seeds audio generation. If not set, the request uses a randomly generated seed.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Controls how closely the model follows prompts.
    /// Higher guidance follows more closely, but will make transitions more abrupt. Range is [0.0, 6.0].
    /// </summary>
    [JsonPropertyName("guidance")]
    public double? Guidance { get; set; }

    /// <summary>
    /// Beats per minute. Range is [60, 200].
    /// </summary>
    [JsonPropertyName("bpm")]
    public int? Bpm { get; set; }

    /// <summary>
    /// Density of sounds. Range is [0.0, 1.0].
    /// </summary>
    [JsonPropertyName("density")]
    public double? Density { get; set; }

    /// <summary>
    /// Brightness of the music. Range is [0.0, 1.0].
    /// </summary>
    [JsonPropertyName("brightness")]
    public double? Brightness { get; set; }

    /// <summary>
    /// Scale of the generated music.
    /// </summary>
    [JsonPropertyName("scale")]
    public Scale? Scale { get; set; }

    /// <summary>
    /// Whether the audio output should contain bass.
    /// </summary>
    [JsonPropertyName("muteBass")]
    public bool? MuteBass { get; set; }

    /// <summary>
    /// Whether the audio output should contain drums.
    /// </summary>
    [JsonPropertyName("muteDrums")]
    public bool? MuteDrums { get; set; }

    /// <summary>
    /// Whether the audio output should contain only bass and drums.
    /// </summary>
    [JsonPropertyName("onlyBassAndDrums")]
    public bool? OnlyBassAndDrums { get; set; }

    /// <summary>
    /// The mode of music generation. Default mode is QUALITY.
    /// </summary>
    [JsonPropertyName("musicGenerationMode")]
    public MusicGenerationMode? MusicGenerationMode { get; set; }
}
