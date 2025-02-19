using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Corpus Permissions endpoint.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.permissions">See Official API Documentation</seealso>
public class CorpusPermissionClient : BaseClient
{
    /// <summary>
    /// Provides operations for managing permissions in a corpus using the Corpus Permissions API.
    /// </summary>
    public CorpusPermissionClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a new <see cref="GenerativeAI.Types.Permission"/> resource.
    /// </summary>
    /// <param name="parent">The parent resource of the <see cref="GenerativeAI.Types.Permission"/>.</param>
    /// <param name="permission">The <see cref="GenerativeAI.Types.Permission"/> to create.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The created <see cref="GenerativeAI.Types.Permission"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.permissions/create">See Official API Documentation</seealso>
    public async Task<Permission?> CreatePermissionAsync(string parent, Permission permission, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{parent.ToCorpusId()}/permissions";
        return await SendAsync<Permission, Permission>(url, permission, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists available <see cref="GenerativeAI.Types.Permission"/> resources.
    /// </summary>
    /// <param name="parent">The parent resource of the permissions.</param>
    /// <param name="pageSize">The maximum number of <see cref="GenerativeAI.Types.Permission"/> resources to return.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListPermissionsAsync"/> call.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A list of <see cref="GenerativeAI.Types.Permission"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.permissions/list">See Official API Documentation</seealso>
    public async Task<ListPermissionsResponse?> ListPermissionsAsync(string parent, int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
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
        var url = $"{_platform.GetBaseUrl()}/{parent.ToCorpusId()}/permissions{queryString}";

        return await GetAsync<ListPermissionsResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a specific <see cref="GenerativeAI.Types.Permission"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="GenerativeAI.Types.Permission"/>.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="GenerativeAI.Types.Permission"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.permissions/get">See Official API Documentation</seealso>
    public async Task<Permission?> GetPermissionAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name}";
        return await GetAsync<Permission>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a <see cref="GenerativeAI.Types.Permission"/> resource.
    /// </summary>
    /// <param name="permissionName">The resource name of the permission.</param>
    /// <param name="permission">The <see cref="GenerativeAI.Types.Permission"/> resource to update.</param>
    /// <param name="updateMask">The list of fields to update.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The updated <see cref="GenerativeAI.Types.Permission"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.permissions/patch">See Official API Documentation</seealso>
    public async Task<Permission?> UpdatePermissionAsync(string permissionName, Permission permission, string? updateMask = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{permissionName}";

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(updateMask))
        {
            queryParams.Add($"updateMask={updateMask}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        return await SendAsync<Permission, Permission>(url + queryString, permission, new HttpMethod("PATCH"), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a <see cref="GenerativeAI.Types.Permission"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="GenerativeAI.Types.Permission"/> to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.permissions/delete">See Official API Documentation</seealso>
    public async Task DeletePermissionAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name}";
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }

    ///<inheritdoc/>
    protected override Task AddAuthorizationHeader(HttpRequestMessage request, bool requiredAccessToken = false,
        CancellationToken cancellationToken = default)
    {
        return base.AddAuthorizationHeader(request, true, cancellationToken);
    }
}