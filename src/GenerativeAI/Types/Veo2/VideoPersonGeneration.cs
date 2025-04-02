using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Allows the generation of all types of people, including unrestricted content, in videos.
/// </summary>
[JsonConverter(typeof(VideoPersonGenerationConverter))]
public enum VideoPersonGeneration
{
    /// <summary>
    /// Prevents the generation of identifiable or realistic people in videos.
    /// </summary>
    DontAllow,

    /// <summary>
    /// Allows the generation of videos featuring adults while maintaining certain restrictions.
    /// </summary>
    AllowAdult,

    /// <summary>
    /// Allows the generation of videos with all types of people without restriction.
    /// </summary>
    AllowAll
}