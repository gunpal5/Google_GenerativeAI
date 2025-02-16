using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Safety setting, affecting the safety-blocking behavior.
/// Passing a safety setting for a category changes the allowed probability that content is blocked.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#safetysetting">See Official API Documentation</seealso>
public class SafetySetting
{
    /// <summary>
    /// Required. The category for this setting.
    /// </summary>
    [JsonPropertyName("category")]
    public HarmCategory Category { get; set; }

    /// <summary>
    /// Required. Controls the probability threshold at which harm is blocked.
    /// </summary>
    [JsonPropertyName("threshold")]
    public HarmBlockThreshold Threshold { get; set; }
}