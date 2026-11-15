using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Logging;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// Shared plumbing of the clients of the agent-related Gemini API resources (Voices, Agents, Environments, Triggers,
/// Credentials and Decisions), which — like the Interactions API — use snake_case JSON and live directly under the API
/// version (<c>{base}/{version}/{collection}</c>, or <c>.../projects/{project}/locations/{location}/{collection}</c>
/// on Vertex AI).
/// </summary>
public abstract class AgentApiClientBase : BaseClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentApiClientBase"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    protected AgentApiClientBase(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
        // Own types first, then the shared Interactions types (Native AOT friendly).
        SerializerOptions?.TypeInfoResolverChain.Insert(0, InteractionsSerializerContext.Default);
        SerializerOptions?.TypeInfoResolverChain.Insert(0, AgentApisSerializerContext.Default);
    }

    /// <summary>
    /// Gets the URL of a collection, e.g. <c>https://generativelanguage.googleapis.com/v1beta/voices</c>.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <returns>The collection URL.</returns>
    protected string GetCollectionUrl(string collection)
    {
        return $"{Platform.GetBaseUrl(appendPublisher: false)}/{collection}";
    }

    /// <summary>
    /// Gets the URL of a resource in a collection. A <c>{collection}/</c> prefix on <paramref name="id"/> is accepted.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The resource ID.</param>
    /// <returns>The resource URL.</returns>
    protected string GetResourceUrl(string collection, string id)
    {
        return $"{GetCollectionUrl(collection)}/{Uri.EscapeDataString(NormalizeId(collection, id))}";
    }

    /// <summary>
    /// Removes an optional <c>{collection}/</c> prefix from <paramref name="id"/> and validates it.
    /// </summary>
    /// <param name="collection">The collection name.</param>
    /// <param name="id">The resource ID.</param>
    /// <returns>The bare ID.</returns>
    protected static string NormalizeId(string collection, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("The resource ID is required.", nameof(id));
        var prefix = collection + "/";
        return id.StartsWith(prefix, StringComparison.Ordinal) ? id.Substring(prefix.Length) : id;
    }

    /// <summary>
    /// Sends a request without a body and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response.</returns>
    protected Task<TResponse> SendRequestAsync<TResponse>(HttpMethod method, string url,
        CancellationToken cancellationToken)
    {
        return SendCoreAsync<TResponse>(method, url, null, cancellationToken);
    }

    /// <summary>
    /// Sends a request with a JSON body and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The URL.</param>
    /// <param name="body">The request body.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response.</returns>
    protected Task<TResponse> SendRequestAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest body,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(body, InteractionJson.GetTypeInfo<TRequest>(Options));
        return SendCoreAsync<TResponse>(method, url, json, cancellationToken);
    }

    /// <summary>
    /// Sends a request and returns the raw (successful) response; the caller disposes it.
    /// </summary>
    /// <param name="request">The request. Authorization is added by this method.</param>
    /// <param name="completionOption">When the operation should complete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response.</returns>
    protected async Task<HttpResponseMessage> SendRawAsync(HttpRequestMessage request,
        HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        var url = request.RequestUri?.ToString() ?? string.Empty;
        await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);
        var response = await HttpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
        try
        {
            await CheckAndHandleErrors(response, url).ConfigureAwait(false);
        }
        catch
        {
            response.Dispose();
            throw;
        }

        return response;
    }

    /// <summary>
    /// Deserializes <paramref name="json"/> with the source-generated metadata of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="json">The JSON.</param>
    /// <returns>The value (a default instance for an empty body).</returns>
    protected T DeserializeJson<T>(string json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json))
            return new T();
        return JsonSerializer.Deserialize(json, InteractionJson.GetTypeInfo<T>(Options)) ?? new T();
    }

    /// <summary>
    /// Builds a query string (<c>?a=1&amp;b=2</c>) from name/value pairs, skipping <c>null</c> values. Values are
    /// URL-encoded; repeated names are allowed (form style, exploded arrays).
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The query string, or an empty string.</returns>
    protected static string BuildQuery(IEnumerable<KeyValuePair<string, string?>> parameters)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(parameters);
#else
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
#endif
        var parts = parameters.Where(p => p.Value != null)
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}")
            .ToList();
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    /// <summary>
    /// Creates the <c>page_size</c> / <c>page_token</c> query parameters.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <param name="pageToken">The page token.</param>
    /// <returns>The parameters.</returns>
    protected static List<KeyValuePair<string, string?>> PageParameters(int? pageSize, string? pageToken)
    {
        return new List<KeyValuePair<string, string?>>
        {
            new("page_size", pageSize?.ToString(CultureInfo.InvariantCulture)),
            new("page_token", string.IsNullOrEmpty(pageToken) ? null : pageToken)
        };
    }

    /// <summary>
    /// Formats a boolean query value (<c>true</c> / <c>false</c>).
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The formatted value, or <c>null</c>.</returns>
    protected static string? FormatBool(bool? value) => value.HasValue ? (value.Value ? "true" : "false") : null;

    /// <summary>
    /// Enumerates all items of a paged list, following <c>next_page_token</c>.
    /// </summary>
    /// <typeparam name="TPage">The page type.</typeparam>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <param name="fetchPage">Fetches a page for a page token.</param>
    /// <param name="getItems">Gets the items of a page.</param>
    /// <param name="getNextPageToken">Gets the next page token of a page.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The items.</returns>
    protected static async IAsyncEnumerable<TItem> PaginateAsync<TPage, TItem>(
        Func<string?, CancellationToken, Task<TPage>> fetchPage,
        Func<TPage, IEnumerable<TItem>?> getItems,
        Func<TPage, string?> getNextPageToken,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fetchPage);
        ArgumentNullException.ThrowIfNull(getItems);
        ArgumentNullException.ThrowIfNull(getNextPageToken);
#else
        if (fetchPage == null) throw new ArgumentNullException(nameof(fetchPage));
        if (getItems == null) throw new ArgumentNullException(nameof(getItems));
        if (getNextPageToken == null) throw new ArgumentNullException(nameof(getNextPageToken));
#endif
        string? pageToken = null;
        do
        {
            var page = await fetchPage(pageToken, cancellationToken).ConfigureAwait(false);
            foreach (var item in getItems(page) ?? Enumerable.Empty<TItem>())
                yield return item;
            pageToken = getNextPageToken(page);
        } while (!string.IsNullOrEmpty(pageToken));
    }

    /// <summary>
    /// Gets the serializer options, which must be initialized.
    /// </summary>
    protected JsonSerializerOptions Options =>
        SerializerOptions ?? throw new InvalidOperationException("SerializerOptions is not initialized");

    /// <summary>
    /// Reads an HTTP content as a string, honoring <paramref name="cancellationToken"/> where supported.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The content as a string.</returns>
    protected static Task<string> ReadContentAsStringAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(content);
        return content.ReadAsStringAsync(cancellationToken);
#else
        if (content == null) throw new ArgumentNullException(nameof(content));
        return content.ReadAsStringAsync();
#endif
    }

    private async Task<TResponse> SendCoreAsync<TResponse>(HttpMethod method, string url, string? json,
        CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(method);
#else
        if (method == null) throw new ArgumentNullException(nameof(method));
#endif
        Logger?.LogHttpRequest(method.Method, url, json);
        using var request = new HttpRequestMessage(method, url);
        if (json != null)
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await SendRawAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);
        var content = await ReadContentAsStringAsync(response.Content, cancellationToken).ConfigureAwait(false);
        Logger?.LogSuccessfulHttpResponse(url, content);
        if (string.IsNullOrWhiteSpace(content))
            content = "{}";
        var result = JsonSerializer.Deserialize(content, InteractionJson.GetTypeInfo<TResponse>(Options));
        return result ?? throw new InvalidOperationException($"The server returned an empty {typeof(TResponse).Name}.");
    }
}
