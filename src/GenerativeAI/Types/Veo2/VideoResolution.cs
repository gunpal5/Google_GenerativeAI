using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Defines the possible resolutions for generated videos.
/// Actual available values depend on the model.
/// </summary>
[JsonConverter(typeof(VideoResolutionConverter))]
public enum VideoResolution
{
    /// <summary>
    /// Unspecified resolution. The model may choose a default.
    /// </summary>
    RESOLUTION_UNSPECIFIED,
    /// <summary>
    /// Standard definition resolution.
    /// </summary>
    HD_720P,
    /// <summary>
    /// High definition resolution.
    /// </summary>
    FullHD_1080P,
    /// <summary>
    /// High definition resolution.
    /// </summary>
    SD_480P,
}