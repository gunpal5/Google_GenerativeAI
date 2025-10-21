using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The API spec that the external API implements.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ApiSpec>))]
public enum ApiSpec
{
    /// <summary>
    /// Unspecified API spec. This value should not be used.
    /// </summary>
    API_SPEC_UNSPECIFIED = 0,

    /// <summary>
    /// Simple search API spec.
    /// </summary>
    SIMPLE_SEARCH = 1,

    /// <summary>
    /// Elastic search API spec.
    /// </summary>
    ELASTIC_SEARCH = 2
}
