using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Credentials API (<c>/credentials</c>): store secrets (environment variables, bearer tokens,
/// OAuth 2.0 refresh tokens) that agent environments can use without exposing them. Secrets are write-only.
/// Follows python-genai's <c>client.credentials</c> (experimental).
/// </summary>
public class CredentialsClient : AgentApiClientBase
{
    private const string Collection = "credentials";

    /// <summary>
    /// Initializes a new instance of the <see cref="CredentialsClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public CredentialsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a credential (<c>POST /credentials</c>).
    /// </summary>
    /// <param name="config">
    /// The credential: <see cref="EnvironmentVariableCredentialConfig"/>, <see cref="BearerTokenCredentialConfig"/> or
    /// <see cref="OAuth2CredentialConfig"/>, with <see cref="CredentialConfig.Id"/> set.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created credential (without the secret).</returns>
    public Task<Credential> CreateAsync(CredentialConfig config, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config);
#else
        if (config == null) throw new ArgumentNullException(nameof(config));
#endif
        if (string.IsNullOrEmpty(config.Id) && config is not UnknownCredentialConfig)
            throw new ArgumentException("'Id' is required.", nameof(config));

        return SendRequestAsync<CredentialConfig, Credential>(HttpMethod.Post, GetCollectionUrl(Collection), config,
            cancellationToken);
    }

    /// <summary>
    /// Updates a credential (<c>PATCH /credentials/{id}</c>). The <c>type</c> must match the stored credential;
    /// <see cref="CredentialConfig.Id"/> is not sent.
    /// </summary>
    /// <param name="id">The credential ID (a <c>credentials/</c> prefix is accepted).</param>
    /// <param name="update">The fields to update.</param>
    /// <param name="updateMask">Optional comma-separated list of fields to update (sent as <c>update_mask</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated credential.</returns>
    public Task<Credential> UpdateAsync(string id, CredentialConfig update, string? updateMask = null,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(update);
#else
        if (update == null) throw new ArgumentNullException(nameof(update));
#endif
        var body = JsonSerializer.SerializeToNode(update, InteractionJson.GetTypeInfo<CredentialConfig>(Options))
                   ?? new JsonObject();
        if (body is JsonObject obj)
            obj.Remove("id");

        var url = GetResourceUrl(Collection, id) +
                  BuildQuery(new[] { new KeyValuePair<string, string?>("update_mask", updateMask) });
        return SendRequestAsync<JsonNode, Credential>(new HttpMethod("PATCH"), url, body, cancellationToken);
    }

    /// <summary>
    /// Gets a credential (<c>GET /credentials/{id}</c>).
    /// </summary>
    /// <param name="id">The credential ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The credential (without the secret).</returns>
    public Task<Credential> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<Credential>(HttpMethod.Get, GetResourceUrl(Collection, id), cancellationToken);
    }

    /// <summary>
    /// Deletes a credential (<c>DELETE /credentials/{id}</c>).
    /// </summary>
    /// <param name="id">The credential ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the credential has been deleted.</returns>
    public new async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(GetResourceUrl(Collection, id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists one page of credentials (<c>GET /credentials</c>).
    /// </summary>
    /// <param name="pageSize">The maximum number of credentials per page.</param>
    /// <param name="pageToken">The page token of a previous call.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListCredentialsResponse> ListAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<ListCredentialsResponse>(HttpMethod.Get,
            GetCollectionUrl(Collection) + BuildQuery(PageParameters(pageSize, pageToken)), cancellationToken);
    }

    /// <summary>
    /// Lists all credentials, following page tokens.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The credentials.</returns>
    public IAsyncEnumerable<Credential> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListCredentialsResponse, Credential>((token, ct) => ListAsync(pageSize, token, ct),
            page => page.Credentials, page => page.NextPageToken, cancellationToken);
    }
}
