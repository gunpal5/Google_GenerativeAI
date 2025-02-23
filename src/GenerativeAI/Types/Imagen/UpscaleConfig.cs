namespace GenerativeAI.Types;

/// <summary>
/// Represents the Upscale configuration.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class UpscaleConfig
{
    /// <summary>
    /// Gets or sets the upscale factor. The supported values are <c>"x2"</c> and <c>"x4"</c>. **Required.**
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("upscaleFactor")]
    public string? UpscaleFactor { get; set; }
}