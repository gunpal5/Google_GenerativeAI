using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Parameters for setting weighted prompts for the live music API.
/// </summary>
public class LiveMusicSetWeightedPromptsParameters
{
    /// <summary>
    /// A map of text prompts to weights to use for the generation request.
    /// </summary>
    [JsonPropertyName("weightedPrompts")]
    public List<WeightedPrompt>? WeightedPrompts { get; set; }
}
