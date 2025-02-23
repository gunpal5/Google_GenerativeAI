namespace GenerativeAI.Types;

/// <summary>
/// Represents the response from the image generation API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class GenerateImageResponse
{
    /// <summary>
    /// Gets or sets the array of predictions, each containing information about a generated image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("predictions")]
    public List<VisionGenerativeModelResult>? Predictions { get; set; }
}