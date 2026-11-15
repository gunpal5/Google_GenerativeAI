using System.Runtime.CompilerServices;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Gemini Webhooks API (<c>/webhooks</c>): register endpoints that receive event notifications
/// for batch jobs, interactions and video generation instead of polling, ping them and rotate their signing secrets.
/// </summary>
/// <remarks>
/// The wire format follows python-genai's generated webhooks client (snake_case field and query parameter names).
/// Requests go to <c>{base}/{version}/webhooks</c> on the Gemini Developer API and to
/// <c>{base}/{version}/projects/{project}/locations/{location}/webhooks</c> on Vertex AI.
/// </remarks>
/// <seealso href="https://ai.google.dev/gemini-api/docs/webhooks">See Official API Documentation</seealso>
public class WebhooksClient : BaseClient
{
    private const string WebhookPrefix = "webhooks/";

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhooksClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public WebhooksClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
        // Resolve the Webhooks types from their own source-generated context first (Native AOT friendly).
        if (SerializerOptions != null && !SerializerOptions.IsReadOnly &&
            !SerializerOptions.TypeInfoResolverChain.Contains(WebhooksSerializerContext.Default))
        {
            SerializerOptions.TypeInfoResolverChain.Insert(0, WebhooksSerializerContext.Default);
        }
    }

    /// <summary>
    /// Gets the URL of the webhooks collection.
    /// </summary>
    /// <returns>The collection URL.</returns>
    protected string GetWebhooksUrl()
    {
        return $"{Platform.GetBaseUrl(appendPublisher: false)}/webhooks";
    }

    /// <summary>
    /// Creates a new webhook (<c>POST /webhooks</c>).
    /// </summary>
    /// <param name="request">The webhook definition; <see cref="CreateWebhookRequest.Uri"/> and
    /// <see cref="CreateWebhookRequest.SubscribedEvents"/> are required.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created webhook. <see cref="Webhook.NewSigningSecret"/> is only populated in this response.</returns>
    public async Task<Webhook> CreateAsync(CreateWebhookRequest request, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        if (string.IsNullOrEmpty(request.Uri))
            throw new ArgumentException("The webhook URI is required.", nameof(request));
        if (request.SubscribedEvents == null || request.SubscribedEvents.Count == 0)
            throw new ArgumentException("At least one subscribed event is required.", nameof(request));

        return await SendAsync<CreateWebhookRequest, Webhook>(GetWebhooksUrl(), request, HttpMethod.Post,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new webhook (<c>POST /webhooks</c>).
    /// </summary>
    /// <param name="uri">The URI to which webhook events will be sent.</param>
    /// <param name="subscribedEvents">The events to subscribe to (see <see cref="WebhookEvents"/>).</param>
    /// <param name="name">Optional user-provided name of the webhook.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created webhook. <see cref="Webhook.NewSigningSecret"/> is only populated in this response.</returns>
    public Task<Webhook> CreateAsync(string uri, IEnumerable<string> subscribedEvents, string? name = null,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(subscribedEvents);
#else
        if (subscribedEvents == null) throw new ArgumentNullException(nameof(subscribedEvents));
#endif
        return CreateAsync(new CreateWebhookRequest
        {
            Name = name,
            Uri = uri,
            SubscribedEvents = subscribedEvents.ToList()
        }, cancellationToken);
    }

    /// <summary>
    /// Gets a webhook (<c>GET /webhooks/{id}</c>).
    /// </summary>
    /// <param name="id">The webhook ID (a <c>webhooks/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The webhook.</returns>
    public async Task<Webhook> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetAsync<Webhook>(GetWebhookUrl(id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists webhooks, one page at a time (<c>GET /webhooks</c>).
    /// </summary>
    /// <param name="pageSize">The maximum number of webhooks to return (the service defaults to 50, max 1000).</param>
    /// <param name="pageToken">A <see cref="WebhookListResponse.NextPageToken"/> from a previous call.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>One page of webhooks.</returns>
    public async Task<WebhookListResponse> ListAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (pageSize.HasValue)
            query.Add($"page_size={pageSize.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        if (!string.IsNullOrEmpty(pageToken))
            query.Add($"page_token={Uri.EscapeDataString(pageToken!)}");
        var url = GetWebhooksUrl() + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
        return await GetAsync<WebhookListResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Enumerates all webhooks, fetching subsequent pages as needed.
    /// </summary>
    /// <param name="pageSize">Optional page size used for each request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An async sequence of every webhook.</returns>
    public async IAsyncEnumerable<Webhook> ListAllAsync(int? pageSize = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? pageToken = null;
        do
        {
            var page = await ListAsync(pageSize, pageToken, cancellationToken).ConfigureAwait(false);
            if (page.Webhooks != null)
            {
                foreach (var webhook in page.Webhooks)
                    yield return webhook;
            }

            pageToken = page.NextPageToken;
        } while (!string.IsNullOrEmpty(pageToken));
    }

    /// <summary>
    /// Updates a webhook (<c>PATCH /webhooks/{id}</c>).
    /// </summary>
    /// <param name="id">The webhook ID (a <c>webhooks/</c> prefix is accepted).</param>
    /// <param name="update">The fields to update; unset (<c>null</c>) fields are not sent.</param>
    /// <param name="updateMask">Optional comma-separated list of fields to update, sent as the <c>update_mask</c>
    /// query parameter (e.g. <c>"uri,subscribed_events"</c>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated webhook.</returns>
    public async Task<Webhook> UpdateAsync(string id, WebhookUpdate update, string? updateMask = null,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(update);
#else
        if (update == null) throw new ArgumentNullException(nameof(update));
#endif
        var url = GetWebhookUrl(id);
        if (!string.IsNullOrEmpty(updateMask))
            url += $"?update_mask={Uri.EscapeDataString(updateMask!)}";
        return await SendAsync<WebhookUpdate, Webhook>(url, update, new HttpMethod("PATCH"), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a webhook (<c>DELETE /webhooks/{id}</c>).
    /// </summary>
    /// <param name="id">The webhook ID (a <c>webhooks/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the webhook has been deleted.</returns>
    public new async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(GetWebhookUrl(id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a ping event to a webhook (<c>POST /webhooks/{id}:ping</c>), e.g. to test the endpoint.
    /// </summary>
    /// <param name="id">The webhook ID (a <c>webhooks/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The (currently empty) ping response.</returns>
    public async Task<WebhookPingResponse> PingAsync(string id, CancellationToken cancellationToken = default)
    {
        return await SendAsync<PingWebhookRequest, WebhookPingResponse>(GetWebhookUrl(id) + ":ping",
            new PingWebhookRequest(), HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates a new signing secret for a webhook (<c>POST /webhooks/{id}:rotateSigningSecret</c>).
    /// </summary>
    /// <param name="id">The webhook ID (a <c>webhooks/</c> prefix is accepted).</param>
    /// <param name="revocationBehavior">Optional revocation behavior for the previous signing secrets
    /// (see <see cref="WebhookRevocationBehaviors"/>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response holding the newly generated secret.</returns>
    public async Task<WebhookRotateSigningSecretResponse> RotateSigningSecretAsync(string id,
        string? revocationBehavior = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync<RotateSigningSecretRequest, WebhookRotateSigningSecretResponse>(
            GetWebhookUrl(id) + ":rotateSigningSecret",
            new RotateSigningSecretRequest { RevocationBehavior = revocationBehavior }, HttpMethod.Post,
            cancellationToken).ConfigureAwait(false);
    }

    private string GetWebhookUrl(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("The webhook ID cannot be null or empty.", nameof(id));
        var webhookId = id.StartsWith(WebhookPrefix, StringComparison.Ordinal) ? id.Substring(WebhookPrefix.Length) : id;
        return $"{GetWebhooksUrl()}/{webhookId}";
    }
}
