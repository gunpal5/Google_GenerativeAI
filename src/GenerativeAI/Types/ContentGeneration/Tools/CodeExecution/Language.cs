using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Supported programming languages for the generated code.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Language">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Language
{
    /// <summary>
    /// Unspecified language. This value should not be used.
    /// </summary>
    LANGUAGE_UNSPECIFIED = 0,

    /// <summary>
    /// Python &gt;= 3.10, with numpy and simpy available.
    /// </summary>
    PYTHON = 1,
}