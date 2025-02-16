using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Block at and beyond a specified harm probability.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#HarmBlockThreshold">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HarmBlockThreshold
{
    /// <summary>
    /// Threshold is unspecified.
    /// </summary>
    HARM_BLOCK_THRESHOLD_UNSPECIFIED = 0,

    /// <summary>
    /// Content with NEGLIGIBLE will be allowed.
    /// </summary>
    BLOCK_LOW_AND_ABOVE = 1,

    /// <summary>
    /// Content with NEGLIGIBLE and LOW will be allowed.
    /// </summary>
    BLOCK_MEDIUM_AND_ABOVE = 2,

    /// <summary>
    /// Content with NEGLIGIBLE, LOW, and MEDIUM will be allowed.
    /// </summary>
    BLOCK_ONLY_HIGH = 3,

    /// <summary>
    /// All content will be allowed.
    /// </summary>
    BLOCK_NONE = 4,

    /// <summary>
    /// Turn off the safety filter.
    /// </summary>
    OFF = 5,
}