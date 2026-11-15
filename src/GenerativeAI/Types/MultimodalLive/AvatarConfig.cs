using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configures the avatar to be used in the live session.
/// </summary>
public class AvatarConfig
{
    /// <summary>
    /// Pre-built avatar id.
    /// </summary>
    [JsonPropertyName("avatarName")]
    public string? AvatarName { get; set; }

    /// <summary>
    /// Customized avatar appearance with a reference image.
    /// </summary>
    [JsonPropertyName("customizedAvatar")]
    public CustomizedAvatar? CustomizedAvatar { get; set; }

    /// <summary>
    /// The bitrate of compressed audio.
    /// </summary>
    [JsonPropertyName("audioBitrateBps")]
    public int? AudioBitrateBps { get; set; }

    /// <summary>
    /// The bitrate of compressed video output.
    /// </summary>
    [JsonPropertyName("videoBitrateBps")]
    public int? VideoBitrateBps { get; set; }
}

/// <summary>
/// Configures the customized avatar to be used in the live session.
/// </summary>
public class CustomizedAvatar
{
    /// <summary>
    /// The mime type of the reference image, e.g., "image/jpeg".
    /// </summary>
    [JsonPropertyName("imageMimeType")]
    public string? ImageMimeType { get; set; }

    /// <summary>
    /// The base64-encoded data of the reference image. The dimensions of the reference image should be
    /// 9:16 (portrait) with a minimum resolution of 704x1280.
    /// </summary>
    [JsonPropertyName("imageData")]
    public string? ImageData { get; set; }
}
