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
    public CachingClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a new <see cref="CachedContent"/> resource.
    /// </summary>
    /// <param name="cachedContent">The <see cref="CachedContent"/> to create.</param>
    /// <returns>The created <see cref="CachedContent"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.create">See Official API Documentation</seealso>
    public async Task<CachedContent?> CreateCachedContentAsync(CachedContent cachedContent)
    {
        var url = $"{_platform.GetBaseUrl()}/cachedContents";
        return await SendAsync<CachedContent,CachedContent>(url, cachedContent,HttpMethod.Post);
    }

    /// <summary>
    /// Lists available <see cref="CachedContent"/> resources.
    /// </summary>
    /// <param name="pageSize">The maximum number of <see cref="CachedContent"/> resources to return.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListCachedContentsAsync"/> call.</param>
    /// <returns>A list of <see cref="CachedContent"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.list">See Official API Documentation</seealso>
    public async Task<ListCachedContentsResponse?> ListCachedContentsAsync(int? pageSize = null, string? pageToken = null)
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

        return await GetAsync<ListCachedContentsResponse>(url);
    }

    /// <summary>
    /// Gets a specific <see cref="CachedContent"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="CachedContent"/>.</param>
    /// <returns>The <see cref="CachedContent"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.get">See Official API Documentation</seealso>
    public async Task<CachedContent?> GetCachedContentAsync(string name)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCachedContentId()}";
        return await GetAsync<CachedContent>(url);
    }

    /// <summary>
    /// Updates a <see cref="CachedContent"/> resource.  Only expiration can be updated.
    /// </summary>
    /// <param name="cachedContent">The <see cref="CachedContent"/> resource to update.</param>
    /// <param name="updateMask">The list of fields to update.</param>
    /// <returns>The updated <see cref="CachedContent"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.patch">See Official API Documentation</seealso>
    public async Task<CachedContent?> UpdateCachedContentAsync(string cacheName,CachedContent cachedContent, string? updateMask = null)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{cacheName.ToCachedContentId()}";

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(updateMask))
        {
            queryParams.Add($"updateMask={updateMask}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        return await SendAsync<CachedContent,CachedContent>(url + queryString, cachedContent, new HttpMethod("PATCH"));
    }

    /// <summary>
    /// Deletes a <see cref="CachedContent"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="CachedContent"/> to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://ai.google.dev/api/caching#method:-cachedcontents.delete">See Official API Documentation</seealso>
    public async Task DeleteCachedContentAsync(string name)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCachedContentId()}";
        await DeleteAsync(url);
    }
}
