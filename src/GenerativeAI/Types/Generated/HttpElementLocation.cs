using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The location in the HTTP request where the API key is sent. This enum is not supported in the Gemini API.
/// </summary>
/// <remarks>Generated from python-genai by scripts/drift/update_dtos.py.</remarks>
[JsonConverter(typeof(JsonStringEnumConverter<HttpElementLocation>))]
public enum HttpElementLocation
{
    /// <summary>
    /// The API value "HTTP_IN_UNSPECIFIED".
    /// </summary>
    HTTP_IN_UNSPECIFIED = 0,

    /// <summary>
    /// The API value "HTTP_IN_QUERY".
    /// </summary>
    HTTP_IN_QUERY = 1,

    /// <summary>
    /// The API value "HTTP_IN_HEADER".
    /// </summary>
    HTTP_IN_HEADER = 2,

    /// <summary>
    /// The API value "HTTP_IN_PATH".
    /// </summary>
    HTTP_IN_PATH = 3,

    /// <summary>
    /// The API value "HTTP_IN_BODY".
    /// </summary>
    HTTP_IN_BODY = 4,

    /// <summary>
    /// The API value "HTTP_IN_COOKIE".
    /// </summary>
    HTTP_IN_COOKIE = 5
}
