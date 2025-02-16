using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The probability that a piece of content is harmful.
/// The classification system gives the probability of the content being unsafe.
/// This does not indicate the severity of harm for a piece of content.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#HarmProbability">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HarmProbability
{
    /// <summary>
    /// Probability is unspecified.
    /// </summary>
    HARM_PROBABILITY_UNSPECIFIED = 0,

    /// <summary>
    /// Content has a negligible chance of being unsafe.
    /// </summary>
    NEGLIGIBLE = 1,

    /// <summary>
    /// Content has a low chance of being unsafe.
    /// </summary>
    LOW = 2,

    /// <summary>
    /// Content has a medium chance of being unsafe.
    /// </summary>
    MEDIUM = 3,

    /// <summary>
    /// Content has a high chance of being unsafe.
    /// </summary>
    HIGH = 4,
}