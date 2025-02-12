using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The mode of the predictor to be used in dynamic retrieval.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Mode">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DynamicRetrievalMode  // Renamed to DynamicRetrievalMode
{
    /// <summary>
    /// Always trigger retrieval.
    /// </summary>
    DYNAMIC_RETRIEVAL_MODE_UNSPECIFIED = 0,  // Renamed to match the class name

    /// <summary>
    /// Run retrieval only when the system decides it is necessary.
    /// </summary>
    MODE_DYNAMIC = 1,
}