using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Gemini API Corpora endpoint.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora">See Official API Documentation</seealso>
public class CorporaClient : BaseClient
{
    /// <summary>
    /// A client for interacting with the Gemini API Corpora endpoint.
    /// </summary>
    /// <param name="platform">The platform adapter that provides platform-specific operations.</param>
    /// <param name="httpClient">An optional <see cref="HttpClient"/> instance for making HTTP requests.</param>
    /// <param name="logger">An optional <see cref="ILogger"/> instance for logging.</param>
    /// <seealso cref="BaseClient"/>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora">See Official API Documentation</seealso>
    public CorporaClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a new <see cref="Corpus"/> resource.
    /// </summary>
    /// <param name="corpus">The <see cref="Corpus"/> to create.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The created <see cref="Corpus"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.create">See Official API Documentation</seealso>
    public async Task<Corpus?> CreateCorpusAsync(Corpus corpus, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/corpora";
        return await SendAsync<Corpus, Corpus>(url, corpus, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs semantic search over a <see cref="Corpus"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="Corpus"/> to query.</param>
    /// <param name="queryCorpusRequest">The <see cref="QueryCorpusRequest"/> containing the query details.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The <see cref="QueryCorpusResponse"/> containing the relevant chunks.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.query">See Official API Documentation</seealso>
    public async Task<QueryCorpusResponse?> QueryCorpusAsync(string name, QueryCorpusRequest queryCorpusRequest, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCorpusId()}:query";
        return await SendAsync<QueryCorpusRequest, QueryCorpusResponse>(url, queryCorpusRequest, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all <see cref="Corpus"/> resources owned by the user.
    /// </summary>
    /// <param name="pageSize">The maximum number of <see cref="Corpus"/> resources to return per page.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListCorporaAsync"/> call.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A list of <see cref="Corpus"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.list">See Official API Documentation</seealso>
    public async Task<ListCorporaResponse?> ListCorporaAsync(int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
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
        var url = $"{_platform.GetBaseUrl()}/corpora{queryString}";

        return await GetAsync<ListCorporaResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets information about a specific <see cref="Corpus"/> resource.
    /// </summary>
    /// <param name="name">The name of the <see cref="Corpus"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The <see cref="Corpus"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.get">See Official API Documentation</seealso>
    public async Task<Corpus?> GetCorpusAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCorpusId()}";
        return await GetAsync<Corpus>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a <see cref="Corpus"/> resource.
    /// </summary>
    /// <param name="corpusName">The name of the <see cref="Corpus"/> to update.</param>
    /// <param name="corpus">The updated <see cref="Corpus"/> resource.</param>
    /// <param name="updateMask">The list of fields to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The updated <see cref="Corpus"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.patch">See Official API Documentation</seealso>
    public async Task<Corpus?> UpdateCorpusAsync(string corpusName, Corpus corpus, string updateMask, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{corpusName.ToCorpusId()}";

        var queryParams = new List<string>
        {
            $"updateMask={updateMask}"
        };

        var queryString = "?" + string.Join("&", queryParams);

        return await SendAsync<Corpus, Corpus>(url + queryString, corpus, new HttpMethod("PATCH"), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a <see cref="Corpus"/> resource.
    /// </summary>
    /// <param name="name">The name of the <see cref="Corpus"/> to delete.</param>
    /// <param name="force">If set to true, any <see cref="Document"/> resources and objects related to this <see cref="Corpus"/> will also be deleted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/corpora#method:-corpora.delete">See Official API Documentation</seealso>
    public async Task DeleteCorpusAsync(string name, bool? force = null, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();
        var url = $"{baseUrl}/{name.ToCorpusId()}";

        var queryParams = new List<string>();

        if (force.HasValue)
        {
            queryParams.Add($"force={force.Value}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        await DeleteAsync(url + queryString, cancellationToken).ConfigureAwait(false);
    }

    ///<inheritdoc/>
    protected override Task AddAuthorizationHeader(HttpRequestMessage request, bool requiredAccessToken = false,
        CancellationToken cancellationToken = default)
    {
        return base.AddAuthorizationHeader(request, true, cancellationToken);
    }
}