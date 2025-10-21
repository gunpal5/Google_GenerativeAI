using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Enum for the mask mode of a video generation mask.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VideoGenerationMaskMode>))]
public enum VideoGenerationMaskMode
{
    /// <summary>
    /// The image mask contains a masked rectangular region which is applied on the first frame of the input video.
    /// The object described in the prompt is inserted into this region and will appear in subsequent frames.
    /// </summary>
    INSERT = 0,

    /// <summary>
    /// The image mask is used to determine an object in the first video frame to track.
    /// This object is removed from the video.
    /// </summary>
    REMOVE = 1,

    /// <summary>
    /// The image mask is used to determine a region in the video.
    /// Objects in this region will be removed.
    /// </summary>
    REMOVE_STATIC = 2,

    /// <summary>
    /// The image mask contains a masked rectangular region where the input video will go.
    /// The remaining area will be generated. Video masks are not supported.
    /// </summary>
    OUTPAINT = 3
}
