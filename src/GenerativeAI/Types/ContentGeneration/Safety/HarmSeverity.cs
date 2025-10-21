using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Output only. Harm severity levels in the content.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<HarmSeverity>))]
public enum HarmSeverity
{
    /// <summary>
    /// Harm severity unspecified.
    /// </summary>
    HARM_SEVERITY_UNSPECIFIED = 0,

    /// <summary>
    /// Negligible level of harm severity.
    /// </summary>
    HARM_SEVERITY_NEGLIGIBLE = 1,

    /// <summary>
    /// Low level of harm severity.
    /// </summary>
    HARM_SEVERITY_LOW = 2,

    /// <summary>
    /// Medium level of harm severity.
    /// </summary>
    HARM_SEVERITY_MEDIUM = 3,

    /// <summary>
    /// High level of harm severity.
    /// </summary>
    HARM_SEVERITY_HIGH = 4
}
