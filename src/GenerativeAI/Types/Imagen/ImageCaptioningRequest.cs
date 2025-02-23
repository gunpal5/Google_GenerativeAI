namespace GenerativeAI.Types;

/// <summary>
/// Represents the request body for the Image Captioning API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
public class ImageCaptioningRequest
{
    /// <summary>
    /// Gets or sets the list of instances.  Only 1 image object allowed.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("instances")]
    public List<ImageInstance>? Instances { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the image captioning request.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("parameters")]
    public ImageCaptioningParameters? Parameters { get; set; }
}