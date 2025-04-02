using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents an image, which can be provided via Google Cloud Storage URI or raw byte data.
/// </summary>
/// <remarks>
/// Used as input for certain video generation tasks (e.g., image-to-video).
/// </remarks>
public class ImageSample
{
    /// <summary>
    /// The Cloud Storage URI of the image. Cannot be set if <see cref="ImageBytes"/> is set.
    /// Example: "gs://bucket-name/image.png".
    /// </summary>
    [JsonPropertyName("gcsUri")]
    public string? GcsUri { get; set; }

    /// <summary>
    /// The raw image data as a byte array, Base64 encoded when serialized. Cannot be set if <see cref="GcsUri"/> is set.
    /// </summary>
    [JsonPropertyName("imageBytes")]
    public byte[]? ImageBytes { get; set; }

    /// <summary>
    /// The MIME type of the image (e.g., "image/png", "image/jpeg"). Required if providing image data.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}