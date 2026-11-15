using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// Creates ephemeral auth tokens for the Live API, so client-side apps (browsers, mobile apps)
/// can open Live sessions without holding your API key. Gemini API only; uses the <c>v1alpha</c> API.
/// </summary>
/// <example>
/// <code>
/// var tokens = new AuthTokensClient(new GoogleAIPlatformAdapter(apiKey));
/// var token = await tokens.CreateAsync(new CreateAuthTokenConfig { Uses = 1 });
/// // On the client side:
/// var live = new MultiModalLiveClient(new GoogleAIPlatformAdapter(token.Name), GoogleAIModels.Gemini38Live);
/// </code>
/// </example>
public class AuthTokensClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthTokensClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter. Must be a Gemini API (Google AI) platform.</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public AuthTokensClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates an ephemeral auth token, building the field mask from <see cref="CreateAuthTokenConfig.LockAdditionalFields"/>.
    /// </summary>
    /// <param name="config">Token options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created token; use <see cref="AuthToken.Name"/> as the API key of a Live client.</returns>
    public Task<AuthToken> CreateAsync(CreateAuthTokenConfig config, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config);
#else
        if (config == null) throw new ArgumentNullException(nameof(config));
#endif
        return CreateAsync(BuildRequest(config), cancellationToken);
    }

    /// <summary>
    /// Creates an ephemeral auth token from a raw request body.
    /// </summary>
    /// <param name="request">The request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created token.</returns>
    public async Task<AuthToken> CreateAsync(CreateAuthTokenRequest request, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        if (Platform is VertextPlatformAdapter)
            throw new NotSupportedException("Ephemeral auth tokens are only supported by the Gemini API, not Vertex AI.");

        var url = $"{Platform.GetBaseUrl(appendVersion: false)}/{ApiVersions.v1Alpha}/auth_tokens";
        return await SendAsync<CreateAuthTokenRequest, AuthToken>(url, request, HttpMethod.Post, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the request body for <paramref name="config"/>, applying the same field-mask rules as the official SDKs.
    /// </summary>
    /// <param name="config">Token options.</param>
    /// <returns>The request body that <see cref="CreateAsync(CreateAuthTokenConfig, CancellationToken)"/> sends.</returns>
    public static CreateAuthTokenRequest BuildRequest(CreateAuthTokenConfig config)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config);
#else
        if (config == null) throw new ArgumentNullException(nameof(config));
#endif
        var request = new CreateAuthTokenRequest
        {
            ExpireTime = config.ExpireTime,
            NewSessionExpireTime = config.NewSessionExpireTime,
            Uses = config.Uses,
            LiveClientSetup = config.LiveConnectConstraints
        };

        var additional = config.LockAdditionalFields?.Select(PrefixGenerationConfigField).ToList();

        if (config.LiveConnectConstraints == null)
        {
            request.FieldMask = additional is { Count: > 0 } ? string.Join(",", additional) : null;
            return request;
        }

        if (additional == null)
            return request; // No field mask: the server locks every setup field.

        var lockedFields = SetFieldsOf(config.LiveConnectConstraints);
        lockedFields.AddRange(additional);
        request.FieldMask = lockedFields.Count > 0 ? string.Join(",", lockedFields) : null;
        return request;
    }

    /// <summary>
    /// Lists the fields set in the setup as field-mask paths, one level deep (for example <c>generationConfig.temperature</c>).
    /// </summary>
    private static List<string> SetFieldsOf(LiveClientSetup setup)
    {
        var json = JsonSerializer.SerializeToNode(setup, TypesSerializerContext.Default.LiveClientSetup) as JsonObject;
        var fields = new List<string>();
        if (json == null)
            return fields;

        foreach (var property in json)
        {
            if (property.Value is JsonObject child && child.Count > 0)
                fields.AddRange(child.Select(c => $"{property.Key}.{c.Key}"));
            else
                fields.Add(property.Key);
        }

        return fields;
    }

    private static string PrefixGenerationConfigField(string field)
    {
        var isGenerationConfigField = TypesSerializerContext.Default.GenerationConfig.Properties
            .Any(p => string.Equals(p.Name, field, StringComparison.Ordinal));
        return isGenerationConfigField ? $"generationConfig.{field}" : field;
    }
}
