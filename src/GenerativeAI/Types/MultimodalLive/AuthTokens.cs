using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// An ephemeral auth token for the Live API. Pass <see cref="Name"/> as the API key of a
/// <c>GoogleAIPlatformAdapter</c> used by a Live client; the client then connects to the constrained endpoint.
/// </summary>
public class AuthToken
{
    /// <summary>
    /// The token, in the form <c>auth_tokens/{token}</c>. Use it in place of an API key.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    [JsonPropertyName("expireTime")]
    public Timestamp? ExpireTime { get; set; }

    /// <summary>
    /// The latest time the token can be used to start a new Live session.
    /// </summary>
    [JsonPropertyName("newSessionExpireTime")]
    public Timestamp? NewSessionExpireTime { get; set; }

    /// <summary>
    /// How many sessions the token can start.
    /// </summary>
    [JsonPropertyName("uses")]
    public int? Uses { get; set; }
}

/// <summary>
/// Request body for creating an ephemeral auth token (<c>POST v1alpha/auth_tokens</c>).
/// Most callers should use <see cref="CreateAuthTokenConfig"/>, which builds the field mask for you.
/// </summary>
public class CreateAuthTokenRequest
{
    /// <summary>
    /// When the token expires. Defaults to 30 minutes from now on the server.
    /// </summary>
    [JsonPropertyName("expireTime")]
    public Timestamp? ExpireTime { get; set; }

    /// <summary>
    /// The latest time the token can start a new session. Defaults to 1 minute from now on the server.
    /// </summary>
    [JsonPropertyName("newSessionExpireTime")]
    public Timestamp? NewSessionExpireTime { get; set; }

    /// <summary>
    /// How many sessions the token can start. Defaults to 1 on the server.
    /// </summary>
    [JsonPropertyName("uses")]
    public int? Uses { get; set; }

    /// <summary>
    /// Live session setup that sessions using this token are constrained to.
    /// </summary>
    [JsonPropertyName("bidiGenerateContentSetup")]
    public LiveClientSetup? LiveClientSetup { get; set; }

    /// <summary>
    /// Comma-separated setup fields that are locked to the values in <see cref="LiveClientSetup"/>.
    /// When the setup is set and this is omitted, all setup fields are locked.
    /// </summary>
    [JsonPropertyName("fieldMask")]
    public string? FieldMask { get; set; }
}

/// <summary>
/// Options for creating an ephemeral Live API auth token.
/// </summary>
public class CreateAuthTokenConfig
{
    /// <summary>
    /// When the token expires. Defaults to 30 minutes from now on the server.
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// The latest time the token can start a new session. Defaults to 1 minute from now on the server.
    /// </summary>
    public DateTime? NewSessionExpireTime { get; set; }

    /// <summary>
    /// How many sessions the token can start. Defaults to 1 on the server.
    /// </summary>
    public int? Uses { get; set; }

    /// <summary>
    /// Setup (model, generation config, system instruction, tools, ...) that sessions using the token are constrained to.
    /// </summary>
    public LiveClientSetup? LiveConnectConstraints { get; set; }

    /// <summary>
    /// Controls which setup fields are locked, following the official SDKs:
    /// <list type="bullet">
    /// <item><description>null (default): with <see cref="LiveConnectConstraints"/>, every setup field is locked.</description></item>
    /// <item><description>Empty: only the fields set in <see cref="LiveConnectConstraints"/> are locked.</description></item>
    /// <item><description>Non-empty: the fields set in <see cref="LiveConnectConstraints"/> plus these fields are locked.
    /// Use wire names; generation config fields such as <c>temperature</c> are prefixed with <c>generationConfig.</c> automatically.</description></item>
    /// </list>
    /// </summary>
    public IList<string>? LockAdditionalFields { get; set; }
}
