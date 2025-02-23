namespace GenerativeAI.Types;

/// <summary>
/// Represents the safety attributes of a generated image.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class SafetyAttributes
{
    /// <summary>
    /// Gets or sets the safety attribute name.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    /// <summary>
    /// Gets or sets the safety attribute score.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("scores")]
    public List<double>? Scores { get; set; }
}