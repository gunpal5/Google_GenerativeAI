using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

/// <summary>
/// Retrieve from data source powered by external API for grounding.
/// The external API is not owned by Google, but needs to follow the pre-defined API spec.
/// </summary>
public class ExternalApi
{
    /// <summary>
    /// The authentication config to access the API.
    /// Deprecated. Please use <see cref="AuthConfig"/> instead.
    /// </summary>
    [JsonPropertyName("apiAuth")]
    public ApiAuth? ApiAuth { get; set; }

    /// <summary>
    /// The API spec that the external API implements.
    /// </summary>
    [JsonPropertyName("apiSpec")]
    public ApiSpec? ApiSpec { get; set; }

    /// <summary>
    /// The authentication config to access the API.
    /// </summary>
    [JsonPropertyName("authConfig")]
    public AuthConfig? AuthConfig { get; set; }

    /// <summary>
    /// Parameters for the elastic search API.
    /// </summary>
    [JsonPropertyName("elasticSearchParams")]
    public ExternalApiElasticSearchParams? ElasticSearchParams { get; set; }

    /// <summary>
    /// The endpoint of the external API. The system will call the API at this endpoint to retrieve the data for grounding.
    /// Example: https://acme.com:443/search
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Parameters for the simple search API.
    /// </summary>
    [JsonPropertyName("simpleSearchParams")]
    public ExternalApiSimpleSearchParams? SimpleSearchParams { get; set; }
}
