using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Defines the valid operators that can be applied to a key-value pair.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#Operator">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Operator
{
    /// <summary>
    /// The default value. This value is unused.
    /// </summary>
    OPERATOR_UNSPECIFIED = 0,

    /// <summary>
    /// Supported by numeric.
    /// </summary>
    LESS = 1,

    /// <summary>
    /// Supported by numeric.
    /// </summary>
    LESS_EQUAL = 2,

    /// <summary>
    /// Supported by numeric &amp; string.
    /// </summary>
    EQUAL = 3,

    /// <summary>
    /// Supported by numeric.
    /// </summary>
    GREATER_EQUAL = 4,

    /// <summary>
    /// Supported by numeric.
    /// </summary>
    GREATER = 5,

    /// <summary>
    /// Supported by numeric &amp; string.
    /// </summary>
    NOT_EQUAL = 6,

    /// <summary>
    /// Supported by string only when <see cref="CustomMetadata"/> value type for the given key
    /// has a <c>stringListValue</c>.
    /// </summary>
    INCLUDES = 7,

    /// <summary>
    /// Supported by string only when <see cref="CustomMetadata"/> value type for the given key
    /// has a <c>stringListValue</c>.
    /// </summary>
    EXCLUDES = 8,
}