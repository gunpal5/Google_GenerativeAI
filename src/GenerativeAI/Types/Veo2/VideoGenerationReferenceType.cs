using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Enum for the reference type of a video generation reference image.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VideoGenerationReferenceType>))]
public enum VideoGenerationReferenceType
{
    /// <summary>
    /// A reference image that provides assets to the generated video,
    /// such as the scene, an object, a character, etc.
    /// </summary>
    ASSET = 0,

    /// <summary>
    /// A reference image that provides aesthetics including colors, lighting, texture, etc.,
    /// to be used as the style of the generated video, such as 'anime', 'photography', 'origami', etc.
    /// </summary>
    STYLE = 1
}
