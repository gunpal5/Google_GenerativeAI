namespace GenerativeAI.Types;

/// <summary>
/// Represents the response body from the Image Captioning API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
public class ImageCaptioningResponse
{
    /// <summary>
    /// Gets or sets the list of predicted text strings (captions).
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("predictions")]
    public List<string>? Predictions { get; set; }
}