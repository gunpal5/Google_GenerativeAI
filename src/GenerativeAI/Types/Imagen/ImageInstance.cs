namespace GenerativeAI.Types;

/// <summary>
/// Represents an image instance in the <see cref="ImageCaptioningRequest"/>.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
public class ImageInstance
{
    /// <summary>
    /// Gets or sets the image data.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("image")]
    public ImageData? Image { get; set; }
}