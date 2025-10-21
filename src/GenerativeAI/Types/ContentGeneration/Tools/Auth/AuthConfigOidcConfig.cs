using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for user OIDC auth.
/// </summary>
public class AuthConfigOidcConfig
{
    /// <summary>
    /// OpenID Connect formatted ID token for extension endpoint.
    /// Only used to propagate token from runtime_auth_config at request time.
    /// </summary>
    [JsonPropertyName("idToken")]
    public string? IdToken { get; set; }

    /// <summary>
    /// The service account used to generate an OpenID Connect (OIDC)-compatible JWT token signed by the Google OIDC Provider
    /// for extension endpoint. The audience for the token will be set to the URL in the server url defined in the OpenApi spec.
    /// If the service account is provided, the service account should grant iam.serviceAccounts.getOpenIdToken permission
    /// to Vertex AI Extension Service Agent.
    /// </summary>
    [JsonPropertyName("serviceAccount")]
    public string? ServiceAccount { get; set; }
}
