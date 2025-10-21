using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Maps a prompt to a relative weight to steer music generation.
/// </summary>
public class WeightedPrompt
{
    /// <summary>
    /// Text prompt.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Weight of the prompt. The weight is used to control the relative importance of the prompt.
    /// Higher weights are more important than lower weights.
    /// Weight must not be 0. Weights of all weighted_prompts in this LiveMusicClientContent message will be normalized.
    /// </summary>
    [JsonPropertyName("weight")]
    public double? Weight { get; set; }
}
