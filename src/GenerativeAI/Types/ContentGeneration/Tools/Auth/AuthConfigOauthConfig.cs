using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for user oauth.
/// </summary>
public class AuthConfigOauthConfig
{
    /// <summary>
    /// Access token for extension endpoint. Only used to propagate token from runtime_auth_config at request time.
    /// </summary>
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// The service account used to generate access tokens for executing the Extension.
    /// If the service account is specified, the iam.serviceAccounts.getAccessToken permission should be granted
    /// to Vertex AI Extension Service Agent on the provided service account.
    /// </summary>
    [JsonPropertyName("serviceAccount")]
    public string? ServiceAccount { get; set; }
}
