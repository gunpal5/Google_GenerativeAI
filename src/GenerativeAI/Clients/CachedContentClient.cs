using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Gemini API Caching endpoint.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching">See Official API Documentation</seealso>
public class CachingClient : BaseClient
{
    /// <summary>
    /// Provides methods to interact with caching resources for generative AI models.
    /// </summary>
    /// <remarks>
    /// This client manages and performs operations related to cached contents,
    /// such as creating, retrieving, or listing cached resources.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/caching">See Official API Documentation</seealso>
    public CachingClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Asynchronously creates a new cached content resource.
    /// </summary>
    /// <param name="cachedContent">The cached content to be created.</param>
    /// <param name="cancellationToken">Optional token to monitor for cancellation requests.</param>
    /// <returns>The created cached content resource.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.create">See Official API Documentation</seealso>
    public async Task<CachedContent> CreateCachedContentAsync(CachedContent cachedContent,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/cachedContents";
        return await SendAsync<CachedContent, CachedContent>(url, cachedContent, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously retrieves a list of <see cref="CachedContent"/> resources.
    /// </summary>
    /// <param name="pageSize">Optional parameter to specify the maximum number of <see cref="CachedContent"/> resources to return.</param>
    /// <param name="pageToken">Optional parameter for a pagination token, used to retrieve the next page of results.</param>
    /// <param name="cancellationToken">Optional parameter to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="ListCachedContentsResponse"/> with the list of fetched <see cref="CachedContent"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.list">See Official API Documentation</seealso>
    public async Task<ListCachedContentsResponse> ListCachedContentsAsync(int? pageSize = null,
        string? pageToken = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (pageSize.HasValue)
        {
            queryParams.Add($"pageSize={pageSize.Value}");
        }

        if (!string.IsNullOrEmpty(pageToken))
        {
            queryParams.Add($"pageToken={pageToken}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        var url = $"{_platform.GetBaseUrl()}/cachedContents{queryString}";

        return await GetAsync<ListCachedContentsResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a specific cached content resource asynchronously.
    /// </summary>
    /// <param name="name">The resource name of the cached content to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>An instance of <see cref="CachedContent"/> representing the retrieved resource, or null if not found.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.get">See Official API Documentation</seealso>
    public async Task<CachedContent?> GetCachedContentAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCachedContentId()}";
        return await GetAsync<CachedContent>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing cached content resource by modifying supported fields, such as expiration time.
    /// </summary>
    /// <param name="cacheName">The name of the cache containing the cached content to update.</param>
    /// <param name="cachedContent">The cached content resource with updated values.</param>
    /// <param name="updateMask">Optional. Specifies which fields in the cache content resource to update.</param>
    /// <param name="cancellationToken">Optional. A token for canceling the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the updated cached content resource.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.patch">See Official API Documentation</seealso>
    public async Task<CachedContent?> UpdateCachedContentAsync(string cacheName, CachedContent cachedContent,
        string? updateMask = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{cacheName.ToCachedContentId()}";

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(updateMask))
        {
            queryParams.Add($"updateMask={updateMask}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        return await SendAsync<CachedContent,CachedContent>(url + queryString, cachedContent, new HttpMethod("PATCH"),cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a cached content resource asynchronously.
    /// </summary>
    /// <param name="name">The name of the cached content resource to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.delete">See Official API Documentation</seealso>
    public async Task DeleteCachedContentAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCachedContentId()}";
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }
}
