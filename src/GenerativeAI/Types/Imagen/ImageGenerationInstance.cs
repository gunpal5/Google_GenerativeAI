namespace GenerativeAI.Types;

/// <summary>
/// Represents an instance for image generation.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class ImageGenerationInstance
{
    /// <summary>
    /// Gets or sets the text prompt for image generation. **Required.**
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// Gets or sets the image data for image generation.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("image")]
    public ImageSource? Image { get; set; }
}