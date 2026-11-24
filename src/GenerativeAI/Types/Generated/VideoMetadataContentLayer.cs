using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Content layers to extract from a video.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
[JsonConverter(typeof(JsonStringEnumConverter<VideoMetadataContentLayer>))]
public enum VideoMetadataContentLayer
{
    /// <summary>
    /// The API value "CONTENT_LAYER_UNSPECIFIED".
    /// </summary>
    CONTENT_LAYER_UNSPECIFIED = 0,

    /// <summary>
    /// The API value "FRAMES".
    /// </summary>
    FRAMES = 1,

    /// <summary>
    /// The API value "AUDIO".
    /// </summary>
    AUDIO = 2,

    /// <summary>
    /// The API value "TRANSCRIPT".
    /// </summary>
    TRANSCRIPT = 3
}
