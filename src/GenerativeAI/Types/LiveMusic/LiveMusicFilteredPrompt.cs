using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A prompt that was filtered with the reason.
/// </summary>
public class LiveMusicFilteredPrompt
{
    /// <summary>
    /// The text prompt that was filtered.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// The reason the prompt was filtered.
    /// </summary>
    [JsonPropertyName("filteredReason")]
    public string? FilteredReason { get; set; }
}
