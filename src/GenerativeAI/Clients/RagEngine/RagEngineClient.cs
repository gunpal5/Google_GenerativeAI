using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// A client for interacting with the Vertex AI RAG Engine API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
public class RagCorpusClient : BaseClient
{
    public RagCorpusClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a new <see cref="RagCorpus"/> resource.
    /// </summary>
    /// <param name="ragCorpus">The <see cref="RagCorpus"/> to create.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The created <see cref="RagCorpus"/>.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<GoogleLongRunningOperation?> CreateRagCorpusAsync(RagCorpus ragCorpus, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl(appendPublisher:false)}/ragCorpora";
        return await SendAsync<RagCorpus, GoogleLongRunningOperation>(url, ragCorpus, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists available <see cref="RagCorpus"/> resources.
    /// </summary>
    /// <param name="pageSize">The maximum number of <see cref="RagCorpus"/> resources to return.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListRagCorporaAsync"/> call.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A list of <see cref="RagCorpus"/> resources.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<ListRagCorporaResponse?> ListRagCorporaAsync(int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
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
        var url = $"{_platform.GetBaseUrl(appendPublisher:false)}/ragCorpora{queryString}";

        return await GetAsync<ListRagCorporaResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a specific <see cref="RagCorpus"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="RagCorpus"/>.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="RagCorpus"/> resource.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<RagCorpus?> GetRagCorpusAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl(appendPublisher:false);
        var url = $"{baseUrl}/{name.ToRagCorpusId()}";
        return await GetAsync<RagCorpus>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a <see cref="RagCorpus"/> resource.
    /// </summary>
    /// <param name="corpusName">The resource name of the <see cref="RagCorpus"/> to update.</param>
    /// <param name="ragCorpus">The <see cref="RagCorpus"/> resource to update.</param>
    /// <param name="updateMask">The list of fields to update.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The updated <see cref="RagCorpus"/> resource.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<GoogleLongRunningOperation?> UpdateRagCorpusAsync(string corpusName, RagCorpus ragCorpus, string? updateMask = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl(appendPublisher:false);
        var url = $"{baseUrl}/{corpusName.ToRagCorpusId()}";

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(updateMask))
        {
            queryParams.Add($"updateMask={updateMask}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        return await SendAsync<RagCorpus, GoogleLongRunningOperation>(url + queryString, ragCorpus, new HttpMethod("PATCH"), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a <see cref="RagCorpus"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="RagCorpus"/> to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task DeleteRagCorpusAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl(appendPublisher:false);
        var url = $"{baseUrl}/{name.ToRagCorpusId()}";
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }
}