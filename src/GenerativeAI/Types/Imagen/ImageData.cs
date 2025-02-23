namespace GenerativeAI.Types;

/// <summary>
/// Represents the image data, which can be either base64 encoded or a Cloud Storage URI.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
public class ImageData
{
    /// <summary>
    /// Gets or sets the base64 encoded image string.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded { get; set; }

    /// <summary>
    /// Gets or sets the Cloud Storage URI of the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("gcsUri")]
    public string? GcsUri { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}