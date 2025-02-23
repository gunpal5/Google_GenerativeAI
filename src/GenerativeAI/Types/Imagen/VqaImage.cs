namespace GenerativeAI.Types;

/// <summary>
/// Represents the image data for the visual question answering request.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
public class VqaImage
{
    /// <summary>
    /// The image as a Base64-encoded string.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded { get; set; }

    /// <summary>
    /// The Cloud Storage URI of the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("gcsUri")]
    public string? GcsUri { get; set; }

    /// <summary>
    /// The MIME type of the image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}