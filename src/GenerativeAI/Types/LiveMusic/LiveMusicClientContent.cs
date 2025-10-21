using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// User input to start or steer the music.
/// </summary>
public class LiveMusicClientContent
{
    /// <summary>
    /// Weighted prompts as the model input.
    /// </summary>
    [JsonPropertyName("weightedPrompts")]
    public List<WeightedPrompt>? WeightedPrompts { get; set; }
}
