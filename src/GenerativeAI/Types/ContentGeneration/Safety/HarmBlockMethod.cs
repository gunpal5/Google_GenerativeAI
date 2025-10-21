using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Specifies if the threshold is used for probability or severity score.
/// If not specified, the threshold is used for probability score.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<HarmBlockMethod>))]
public enum HarmBlockMethod
{
    /// <summary>
    /// The harm block method is unspecified.
    /// </summary>
    HARM_BLOCK_METHOD_UNSPECIFIED = 0,

    /// <summary>
    /// The harm block method uses both probability and severity scores.
    /// </summary>
    SEVERITY = 1,

    /// <summary>
    /// The harm block method uses the probability score.
    /// </summary>
    PROBABILITY = 2
}
