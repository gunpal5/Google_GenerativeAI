using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The environment being operated.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Environment>))]
public enum Environment
{
    /// <summary>
    /// Defaults to browser.
    /// </summary>
    ENVIRONMENT_UNSPECIFIED = 0,

    /// <summary>
    /// Operates in a web browser.
    /// </summary>
    ENVIRONMENT_BROWSER = 1,

    /// <summary>
    /// Operates in a mobile environment.
    /// </summary>
    ENVIRONMENT_MOBILE,

    /// <summary>
    /// Operates in a desktop environment.
    /// </summary>
    ENVIRONMENT_DESKTOP
}
