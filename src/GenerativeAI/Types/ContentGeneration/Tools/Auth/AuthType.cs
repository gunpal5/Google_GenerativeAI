using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Type of auth scheme.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AuthType>))]
public enum AuthType
{
    /// <summary>
    /// Auth type unspecified.
    /// </summary>
    AUTH_TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// No Auth.
    /// </summary>
    NO_AUTH = 1,

    /// <summary>
    /// API Key Auth.
    /// </summary>
    API_KEY_AUTH = 2,

    /// <summary>
    /// HTTP Basic Auth.
    /// </summary>
    HTTP_BASIC_AUTH = 3,

    /// <summary>
    /// Google Service Account Auth.
    /// </summary>
    GOOGLE_SERVICE_ACCOUNT_AUTH = 4,

    /// <summary>
    /// OAuth auth.
    /// </summary>
    OAUTH = 5,

    /// <summary>
    /// OpenID Connect (OIDC) Auth.
    /// </summary>
    OIDC_AUTH = 6
}
