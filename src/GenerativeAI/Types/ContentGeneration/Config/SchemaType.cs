using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Type contains the list of OpenAPI data types as defined by
/// <see href="https://spec.openapis.org/oas/v3.0.3#data-types">https://spec.openapis.org/oas/v3.0.3#data-types</see>.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#Type">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SchemaType
{
    /// <summary>
    /// Not specified, should not be used.
    /// </summary>
    TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// String type.
    /// </summary>
    STRING = 1,

    /// <summary>
    /// Number type.
    /// </summary>
    NUMBER = 2,

    /// <summary>
    /// Integer type.
    /// </summary>
    INTEGER = 3,

    /// <summary>
    /// Boolean type.
    /// </summary>
    BOOLEAN = 4,

    /// <summary>
    /// Array type.
    /// </summary>
    ARRAY = 5,

    /// <summary>
    /// Object type.
    /// </summary>
    OBJECT = 6,
}