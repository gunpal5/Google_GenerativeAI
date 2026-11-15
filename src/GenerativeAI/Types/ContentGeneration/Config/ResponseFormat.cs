using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Output format for one response modality. Set only the member for the modality it applies to. Vertex AI only.
/// </summary>
public class ResponseFormat
{
    /// <summary>
    /// Format of text output.
    /// </summary>
    [JsonPropertyName("text")]
    public TextResponseFormat? Text { get; set; }

    /// <summary>
    /// Format of image output.
    /// </summary>
    [JsonPropertyName("image")]
    public ImageResponseFormat? Image { get; set; }

    /// <summary>
    /// Format of audio output.
    /// </summary>
    [JsonPropertyName("audio")]
    public AudioResponseFormat? Audio { get; set; }

    /// <summary>
    /// Format of video output.
    /// </summary>
    [JsonPropertyName("video")]
    public VideoResponseFormat? Video { get; set; }
}

/// <summary>
/// Format of text output.
/// </summary>
public class TextResponseFormat
{
    /// <summary>
    /// MIME type of the text, for example <c>application/json</c>.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// JSON Schema the output must follow when <see cref="MimeType"/> is <c>application/json</c>.
    /// </summary>
    [JsonPropertyName("jsonSchema")]
    public JsonNode? JsonSchema { get; set; }
}

/// <summary>
/// Format of image output.
/// </summary>
public class ImageResponseFormat
{
    /// <summary>
    /// MIME type of the image, for example <c>image/png</c>.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Aspect ratio of the image.
    /// </summary>
    [JsonPropertyName("aspectRatio")]
    public AspectRatio? AspectRatio { get; set; }

    /// <summary>
    /// Size of the image.
    /// </summary>
    [JsonPropertyName("imageSize")]
    public ImageSize? ImageSize { get; set; }

    /// <summary>
    /// Whether the image is returned inline or as a URI.
    /// </summary>
    [JsonPropertyName("delivery")]
    public Delivery? Delivery { get; set; }
}

/// <summary>
/// Format of audio output.
/// </summary>
public class AudioResponseFormat
{
    /// <summary>
    /// MIME type of the audio.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Sample rate in hertz.
    /// </summary>
    [JsonPropertyName("sampleRate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// Bit rate in bits per second.
    /// </summary>
    [JsonPropertyName("bitRate")]
    public int? BitRate { get; set; }

    /// <summary>
    /// Whether the audio is returned inline or as a URI.
    /// </summary>
    [JsonPropertyName("delivery")]
    public Delivery? Delivery { get; set; }
}

/// <summary>
/// Format of video output.
/// </summary>
public class VideoResponseFormat
{
    /// <summary>
    /// Aspect ratio of the video.
    /// </summary>
    [JsonPropertyName("aspectRatio")]
    public AspectRatio? AspectRatio { get; set; }

    /// <summary>
    /// Resolution of the video, for example <c>1080p</c>.
    /// </summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    /// <summary>
    /// Length of the video, as a duration string (for example "8s").
    /// </summary>
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    /// <summary>
    /// Cloud Storage URI to write the video to.
    /// </summary>
    [JsonPropertyName("gcsUri")]
    public string? GcsUri { get; set; }

    /// <summary>
    /// Whether the video is returned inline or as a URI.
    /// </summary>
    [JsonPropertyName("delivery")]
    public Delivery? Delivery { get; set; }
}

/// <summary>
/// How generated media is returned.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Delivery>))]
public enum Delivery
{
    /// <summary>
    /// Delivery is unspecified.
    /// </summary>
    DELIVERY_UNSPECIFIED,

    /// <summary>
    /// The media is returned inline in the response.
    /// </summary>
    INLINE,

    /// <summary>
    /// The media is written to storage and returned as a URI.
    /// </summary>
    URI
}

/// <summary>
/// Aspect ratio of generated images or video.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AspectRatio>))]
public enum AspectRatio
{
    /// <summary>Unspecified aspect ratio.</summary>
    ASPECT_RATIO_UNSPECIFIED,
    /// <summary>1:1.</summary>
    ASPECT_RATIO_ONE_BY_ONE,
    /// <summary>2:3.</summary>
    ASPECT_RATIO_TWO_BY_THREE,
    /// <summary>3:2.</summary>
    ASPECT_RATIO_THREE_BY_TWO,
    /// <summary>3:4.</summary>
    ASPECT_RATIO_THREE_BY_FOUR,
    /// <summary>4:3.</summary>
    ASPECT_RATIO_FOUR_BY_THREE,
    /// <summary>4:5.</summary>
    ASPECT_RATIO_FOUR_BY_FIVE,
    /// <summary>5:4.</summary>
    ASPECT_RATIO_FIVE_BY_FOUR,
    /// <summary>9:16.</summary>
    ASPECT_RATIO_NINE_BY_SIXTEEN,
    /// <summary>16:9.</summary>
    ASPECT_RATIO_SIXTEEN_BY_NINE,
    /// <summary>21:9.</summary>
    ASPECT_RATIO_TWENTY_ONE_BY_NINE,
    /// <summary>1:8.</summary>
    ASPECT_RATIO_ONE_BY_EIGHT,
    /// <summary>8:1.</summary>
    ASPECT_RATIO_EIGHT_BY_ONE,
    /// <summary>1:4.</summary>
    ASPECT_RATIO_ONE_BY_FOUR,
    /// <summary>4:1.</summary>
    ASPECT_RATIO_FOUR_BY_ONE
}

/// <summary>
/// Size of generated images.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ImageSize>))]
public enum ImageSize
{
    /// <summary>Unspecified size.</summary>
    IMAGE_SIZE_UNSPECIFIED,
    /// <summary>512 pixels on the longer side.</summary>
    IMAGE_SIZE_FIVE_TWELVE,
    /// <summary>1K.</summary>
    IMAGE_SIZE_ONE_K,
    /// <summary>2K.</summary>
    IMAGE_SIZE_TWO_K,
    /// <summary>4K.</summary>
    IMAGE_SIZE_FOUR_K
}
