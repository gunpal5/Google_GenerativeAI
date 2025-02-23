namespace GenerativeAI.Types;

/// <summary>
/// Represents the image source, either as base64 encoded bytes or a Cloud Storage URI.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class ImageSource
{
    /// <summary>
    /// Gets or sets the base64 encoded image bytes.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded { get; set; }

    /// <summary>
    /// Gets or sets the Cloud Storage URI of the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("gcsUri")]
    public string? GcsUri { get; set; }
}