using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Auth configuration to run the extension.
/// </summary>
public class AuthConfig
{
    /// <summary>
    /// Config for API key auth.
    /// </summary>
    [JsonPropertyName("apiKeyConfig")]
    public ApiKeyConfig? ApiKeyConfig { get; set; }

    /// <summary>
    /// Type of auth scheme.
    /// </summary>
    [JsonPropertyName("authType")]
    public AuthType? AuthType { get; set; }

    /// <summary>
    /// Config for Google Service Account auth.
    /// </summary>
    [JsonPropertyName("googleServiceAccountConfig")]
    public AuthConfigGoogleServiceAccountConfig? GoogleServiceAccountConfig { get; set; }

    /// <summary>
    /// Config for HTTP Basic auth.
    /// </summary>
    [JsonPropertyName("httpBasicAuthConfig")]
    public AuthConfigHttpBasicAuthConfig? HttpBasicAuthConfig { get; set; }

    /// <summary>
    /// Config for user oauth.
    /// </summary>
    [JsonPropertyName("oauthConfig")]
    public AuthConfigOauthConfig? OauthConfig { get; set; }

    /// <summary>
    /// Config for user OIDC auth.
    /// </summary>
    [JsonPropertyName("oidcConfig")]
    public AuthConfigOidcConfig? OidcConfig { get; set; }

    /// <summary>
    /// The API key used to authenticate requests. Only API key authentication is supported. This field is not
    /// supported in the Gemini API.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string? ApiKey { get; set; }
}
