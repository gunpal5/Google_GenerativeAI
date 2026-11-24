using System.Text;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Logging;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// [Experimental] Creates and revokes ephemeral auth tokens for live interactions
/// (<c>/{version}/interactions/{id}/auth_tokens</c>), so client-side apps can connect to one interaction without
/// holding your API key (python-genai <c>client.interactions.auth_tokens</c>). Gemini API only. Live Interactions
/// support is experimental and may change in future versions.
/// </summary>
/// <example>
/// <code>
/// var interactions = new InteractionsClient(new GoogleAIPlatformAdapter(apiKey));
/// var token = await interactions.AuthTokens.CreateAsync("v1_abc");
/// // On the client side, token.AuthToken ("auth_tokens/...") is used as the API key:
/// var clientSide = new InteractionsClient(new GoogleAIPlatformAdapter(token.AuthToken));
/// await using var connection = await clientSide.ConnectAsync("v1_abc");
/// </code>
/// </example>
public class InteractionAuthTokensClient : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionAuthTokensClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter. Must be a Gemini API (Google AI) platform.</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public InteractionAuthTokensClient(IPlatformAdapter platform, HttpClient? httpClient = null,
        ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
        SerializerOptions?.TypeInfoResolverChain.Insert(0, InteractionsSerializerContext.Default);
    }

    /// <summary>
    /// Creates a short-lived auth token for a live interaction.
    /// </summary>
    /// <param name="interactionId">The interaction ID (an <c>interactions/</c> prefix is accepted).</param>
    /// <param name="parameters">Token options, e.g. <see cref="InteractionAuthTokenCreateParams.ExpireTime"/>.</param>
    /// <param name="apiVersion">The API version; defaults to the platform's API version.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The token; <see cref="InteractionAuthToken.AuthToken"/> (<c>auth_tokens/...</c>) is used as an API key.</returns>
    /// <exception cref="NotSupportedException">The platform is Vertex AI.</exception>
    public async Task<InteractionAuthToken> CreateAsync(string interactionId,
        InteractionAuthTokenCreateParams? parameters = null, string? apiVersion = null,
        CancellationToken cancellationToken = default)
    {
        LiveInteractionsExperimental.Warn(Logger);
        EnsureGeminiApi();
        var url = GetAuthTokensUrl(interactionId, apiVersion);
        var json = JsonSerializer.Serialize(parameters ?? new InteractionAuthTokenCreateParams(),
            InteractionJson.GetTypeInfo<InteractionAuthTokenCreateParams>(Options));
        Logger?.LogHttpRequest("POST", url, json);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);
#if NET5_0_OR_GREATER
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        return JsonSerializer.Deserialize(body, InteractionJson.GetTypeInfo<InteractionAuthToken>(Options))
               ?? throw new InvalidOperationException("The server returned an empty auth token.");
    }

    /// <summary>
    /// Creates a short-lived auth token for a live interaction that expires at <paramref name="expireTime"/>.
    /// </summary>
    /// <param name="interactionId">The interaction ID.</param>
    /// <param name="expireTime">Hard deadline: messages are rejected after this time.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The token.</returns>
    public Task<InteractionAuthToken> CreateAsync(string interactionId, DateTimeOffset expireTime,
        CancellationToken cancellationToken = default)
    {
        return CreateAsync(interactionId, new InteractionAuthTokenCreateParams { ExpireTime = expireTime },
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes (revokes) an interaction auth token.
    /// </summary>
    /// <param name="interactionId">The interaction ID that owns the token.</param>
    /// <param name="authToken">The token, with or without its <c>auth_tokens/</c> prefix.</param>
    /// <param name="apiVersion">The API version; defaults to the platform's API version.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the token has been revoked.</returns>
    /// <exception cref="NotSupportedException">The platform is Vertex AI.</exception>
    public async Task DeleteAsync(string interactionId, string authToken, string? apiVersion = null,
        CancellationToken cancellationToken = default)
    {
        LiveInteractionsExperimental.Warn(Logger);
        EnsureGeminiApi();
        if (string.IsNullOrWhiteSpace(authToken))
            throw new ArgumentException("The auth token is required.", nameof(authToken));
        if (authToken.StartsWith(InteractionWebSocketProtocol.AuthTokenPrefix, StringComparison.Ordinal))
            authToken = authToken.Substring(InteractionWebSocketProtocol.AuthTokenPrefix.Length);

        var url = $"{GetAuthTokensUrl(interactionId, apiVersion)}/{Uri.EscapeDataString(authToken)}";
        await base.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }

    private JsonSerializerOptions Options =>
        SerializerOptions ?? throw new InvalidOperationException("SerializerOptions is not initialized");

    private void EnsureGeminiApi()
    {
        if (Platform is VertextPlatformAdapter)
            throw new NotSupportedException("Live Interactions is currently Gemini API only.");
    }

    private string GetAuthTokensUrl(string interactionId, string? apiVersion)
    {
        if (string.IsNullOrWhiteSpace(interactionId))
            throw new ArgumentException("The interaction ID is required.", nameof(interactionId));
        const string prefix = "interactions/";
        if (interactionId.StartsWith(prefix, StringComparison.Ordinal))
            interactionId = interactionId.Substring(prefix.Length);
        var version = string.IsNullOrEmpty(apiVersion) ? Platform.GetApiVersion() : apiVersion;
        return $"{Platform.GetBaseUrl(appendVersion: false)}/{version}/interactions/" +
               $"{Uri.EscapeDataString(interactionId)}/auth_tokens";
    }
}

/// <summary>
/// Logs the one-time "Live Interactions is experimental" warning, like python-genai's once-per-process warning.
/// </summary>
internal static class LiveInteractionsExperimental
{
    internal const string Message =
        "Live Interactions usage (InteractionsClient.ConnectAsync and InteractionsClient.AuthTokens) is experimental " +
        "and may change in future versions.";

    private static int _warned;

    internal static void Warn(ILogger? logger)
    {
        if (logger == null || Interlocked.Exchange(ref _warned, 1) != 0)
            return;
#pragma warning disable CA1848, CA2254 // One-time warning
        logger.LogWarning(Message);
#pragma warning restore CA1848, CA2254
    }
}
