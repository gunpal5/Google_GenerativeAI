namespace GenerativeAI.Types;

/// <summary>
/// Represents the request for generating images.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class GenerateImageRequest
{
    /// <summary>
    /// Gets or sets the instances for the image generation request.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("instances")]
    public List<ImageGenerationInstance>? Instances { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the image generation request.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("parameters")]
    public ImageGenerationParameters? Parameters { get; set; }
}