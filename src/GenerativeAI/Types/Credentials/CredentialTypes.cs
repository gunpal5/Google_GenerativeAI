using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// A stored credential (<c>/credentials</c>) that agent environments can use without exposing the secret (e.g. as
/// an environment variable or a bearer token injected into allow-listed egress requests). Secrets are write-only and
/// never returned.
/// </summary>
public sealed class Credential
{
    /// <summary>The credential ID.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The credential type (see <see cref="CredentialTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The credential status (see <see cref="CredentialStatuses"/>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Creation time (RFC 3339).</summary>
    [JsonPropertyName("create_time")]
    public string? CreateTime { get; set; }

    /// <summary>Last update time (RFC 3339).</summary>
    [JsonPropertyName("update_time")]
    public string? UpdateTime { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Response of <c>GET /credentials</c>.</summary>
public sealed class ListCredentialsResponse
{
    /// <summary>The credentials.</summary>
    [JsonPropertyName("credentials")]
    public List<Credential>? Credentials { get; set; }

    /// <summary>The token of the next page, if any.</summary>
    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// The body of <c>POST /credentials</c> (create, <see cref="Id"/> required) and <c>PATCH /credentials/{id}</c>
/// (update, <see cref="Id"/> not sent), discriminated by <c>type</c> (see <see cref="CredentialTypes"/>).
/// For updates, only the fields that are set are sent; combine with an <c>update_mask</c> to clear fields.
/// </summary>
[JsonConverter(typeof(CredentialConfigConverter))]
public abstract class CredentialConfig
{
    /// <summary>
    /// Initializes a new configuration with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected CredentialConfig(string type)
    {
        Type = type;
    }

    /// <summary>The credential type.</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>The credential ID. Required on creation; ignored on update (the ID is part of the URL).</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Additional body fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>A secret exposed to the environment as a variable (<c>environment_variable</c>).</summary>
public sealed class EnvironmentVariableCredentialConfig : CredentialConfig
{
    /// <summary>Initializes a new instance of the <see cref="EnvironmentVariableCredentialConfig"/> class.</summary>
    public EnvironmentVariableCredentialConfig() : base(CredentialTypes.EnvironmentVariable)
    {
    }

    /// <summary>The secret value. Required on creation.</summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// Where the secret may be injected into outgoing requests (see <see cref="CredentialInjectionLocations"/>).
    /// Required on creation. The API accepts a single value or a list; a list is always sent.
    /// </summary>
    [JsonPropertyName("injection_location")]
    [JsonConverter(typeof(StringOrStringListConverter))]
    public List<string>? InjectionLocation { get; set; }

    /// <summary>The domains the secret may be sent to.</summary>
    [JsonPropertyName("trusted_domains")]
    public List<string>? TrustedDomains { get; set; }
}

/// <summary>An HTTP bearer token (<c>bearer_token</c>).</summary>
public sealed class BearerTokenCredentialConfig : CredentialConfig
{
    /// <summary>Initializes a new instance of the <see cref="BearerTokenCredentialConfig"/> class.</summary>
    public BearerTokenCredentialConfig() : base(CredentialTypes.BearerToken)
    {
    }

    /// <summary>The token. Required on creation.</summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    /// <summary>The header the token is sent in (server default: <c>Authorization</c>).</summary>
    [JsonPropertyName("header_name")]
    public string? HeaderName { get; set; }

    /// <summary>The prefix of the header value (server default: <c>Bearer</c>).</summary>
    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }
}

/// <summary>OAuth 2.0 refresh-token credentials (<c>oauth2</c>).</summary>
public sealed class OAuth2CredentialConfig : CredentialConfig
{
    /// <summary>Initializes a new instance of the <see cref="OAuth2CredentialConfig"/> class.</summary>
    public OAuth2CredentialConfig() : base(CredentialTypes.OAuth2)
    {
    }

    /// <summary>The OAuth client ID. Required on creation.</summary>
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    /// <summary>The OAuth client secret. Required on creation.</summary>
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }

    /// <summary>The refresh token. Required on creation.</summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>The token endpoint URL. Required on creation.</summary>
    [JsonPropertyName("token_url")]
    public string? TokenUrl { get; set; }

    /// <summary>The OAuth scopes.</summary>
    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }
}

/// <summary>A credential configuration whose <c>type</c> this SDK does not know; the raw JSON is kept and sent as is.</summary>
public sealed class UnknownCredentialConfig : CredentialConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownCredentialConfig"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownCredentialConfig(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the configuration.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>Known credential types.</summary>
public static class CredentialTypes
{
    /// <summary>An HTTP bearer token.</summary>
    public const string BearerToken = "bearer_token";

    /// <summary>OAuth 2.0 refresh-token credentials.</summary>
    public const string OAuth2 = "oauth2";

    /// <summary>A secret exposed as an environment variable.</summary>
    public const string EnvironmentVariable = "environment_variable";
}

/// <summary>Known credential statuses.</summary>
public static class CredentialStatuses
{
    /// <summary>The credential is active.</summary>
    public const string Active = "active";

    /// <summary>The credential was revoked.</summary>
    public const string Revoked = "revoked";
}

/// <summary>Known values of <see cref="EnvironmentVariableCredentialConfig.InjectionLocation"/>.</summary>
public static class CredentialInjectionLocations
{
    /// <summary>Request headers.</summary>
    public const string Header = "header";

    /// <summary>The query string.</summary>
    public const string Query = "query";

    /// <summary>The request body.</summary>
    public const string Body = "body";
}
