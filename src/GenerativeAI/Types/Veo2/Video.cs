using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents a video resource, which can be referenced via a URI or contained as raw byte data.
/// </summary>
/// <remarks>
/// This class is typically used for outputting generated video content.
/// </remarks>
public class Video
{
    /// <summary>
    /// The URI (e.g., Google Cloud Storage path) where the video is stored.
    /// Path to another storage.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    /// <summary>
    /// The raw video data as a byte array, Base64 encoded when serialized. May not always be populated, especially for large videos stored at a URI.
    /// </summary>
    [JsonPropertyName("videoBytes")]
    public byte[]? VideoBytes { get; set; }
    
    private string? _bytesBase64Encoded;
    /// <summary>
    /// The raw video data as a byte array, Base64 encoded when serialized. May not always be populated, especially for large videos stored at a URI.
    /// </summary>
    [JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded 
    {
        get
        {
            return _bytesBase64Encoded;
        }
        set
        {
            this._bytesBase64Encoded = value;
            if(value != null)
                this.VideoBytes = Convert.FromBase64String(value);
        }
    }

    /// <summary>
    /// The MIME type of the video (e.g., "video/mp4").
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
}