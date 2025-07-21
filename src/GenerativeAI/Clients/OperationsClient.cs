using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// Provides functionality for interacting with long-running operations. The OperationsClient
/// allows for querying, listing, canceling, and deleting operations on a given platform.
/// </summary>
public class OperationsClient : BaseClient
{
    /// <summary>
    /// Provides functionality for interacting with long-running operations.
    /// Supports querying, listing, canceling, and deleting operations via the platform interface.
    /// </summary>
    public OperationsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(
        platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Lists operations that match the specified filter in the request.
    /// </summary>
    /// <param name="name">The name of the operation's parent resource.</param>
    /// <param name="filter">The standard list filter.</param>
    /// <param name="pageSize">The standard list page size.</param>
    /// <param name="pageToken">The standard list page token.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A list of operations that match the specified filter in the request.</returns>
    public async Task<GoogleLongRunningListOperationsResponse?> ListOperationsAsync(string name, string? filter = null,
        int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(filter))
        {
            queryParams.Add($"filter={filter}");
        }

        if (pageSize.HasValue)
        {
            queryParams.Add($"pageSize={pageSize.Value}");
        }

        if (!string.IsNullOrEmpty(pageToken))
        {
            queryParams.Add($"pageToken={pageToken}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        var url = $"{Platform.GetBaseUrl(appendPublisher:false)}/{name.RecoverOperationId()}/operations{queryString}";

        return await GetAsync<GoogleLongRunningListOperationsResponse>(url, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the latest state of a long-running operation.
    /// </summary>
    /// <param name="name">The name of the operation resource.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="GoogleLongRunningOperation"/> resource.</returns>
    public async Task<GoogleLongRunningOperation?> GetOperationAsync(string name,
        CancellationToken cancellationToken = default)
    {
        var url = $"{Platform.GetBaseUrl(appendPublisher:false)}/{name.RecoverOperationId()}";
        return await GetAsync<GoogleLongRunningOperation>(url, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a long-running operation.
    /// </summary>
    /// <param name="name">The name of the operation resource to be deleted.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteOperationAsync(string name, CancellationToken cancellationToken = default)
    {
        var url = $"{Platform.GetBaseUrl(appendPublisher:false)}/{name.RecoverOperationId()}";
        await DeleteAsync(url, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts asynchronous cancellation on a long-running operation.
    /// </summary>
    /// <param name="name">The name of the operation resource to be cancelled.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CancelOperationAsync(string name, CancellationToken cancellationToken = default)
    {
        var url = $"{Platform.GetBaseUrl(appendPublisher:false)}/{name.RecoverOperationId()}:cancel";
        await GetAsync<dynamic>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Fetches the status of a long-running operation by its ID.
    /// </summary>
    /// <param name="operationId">The ID of the operation to fetch status for.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The current status of the long-running operation.</returns>
    public async Task<GoogleLongRunningOperation> FetchOperationStatusAsync(string operationId, CancellationToken cancellationToken)
    {
        var url = $"{Platform.GetBaseUrl(appendPublisher:false)}/{operationId.RecoverModelIdFromOperationId()}:fetchPredictOperation";

        GoogleLongRunningOperation post = new GoogleLongRunningOperation()
        {
            OperationName = operationId
        };
        return await SendAsync<GoogleLongRunningOperation,GoogleLongRunningOperation>(url, post, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }
}