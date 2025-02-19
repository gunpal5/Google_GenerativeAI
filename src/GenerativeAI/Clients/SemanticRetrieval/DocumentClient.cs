using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Documents API.
/// </summary>
/// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents">See Official API Documentation</seealso>
public class DocumentsClient : BaseClient
{
    /// <summary>
    /// A client for interacting with the Documents API.
    /// Provides methods to create, query, list, retrieve, update, and delete document resources.
    /// </summary>
    /// <remarks>
    /// This client simplifies interaction with the backend endpoint handling document-related operations.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents">See Official API Documentation</seealso>
    public DocumentsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a new <see cref="Document"/> resource.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Corpus"/> where this <see cref="Document"/> will be created. Required.</param>
    /// <param name="document">The <see cref="Document"/> to create.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The created <see cref="Document"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#method:-corpora.documents.create">See Official API Documentation</seealso>
    public async Task<Document?> CreateDocumentAsync(string parent, Document document, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{parent.ToCorpusId()}/documents";
        return await SendAsync<Document, Document>(url, document, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs semantic search over a <see cref="Document"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="Document"/> to query. Required.</param>
    /// <param name="queryDocumentRequest">The query parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A list of <see cref="RelevantChunk"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#method:-corpora.documents.query">See Official API Documentation</seealso>
    public async Task<QueryDocumentResponse?> QueryDocumentAsync(string name, QueryDocumentRequest queryDocumentRequest, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{name}:query";
        return await SendAsync<QueryDocumentRequest, QueryDocumentResponse>(url, queryDocumentRequest, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all <see cref="Document"/> resources in a <see cref="Corpus"/>.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Corpus"/> containing <see cref="Document"/> resources. Required.</param>
    /// <param name="pageSize">The maximum number of <see cref="Document"/> resources to return (per page). Optional.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListDocumentsAsync"/> call. Optional.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A list of <see cref="Document"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#method:-corpora.documents.list">See Official API Documentation</seealso>
    public async Task<ListDocumentsResponse?> ListDocumentsAsync(string parent, int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
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
        var url = $"{_platform.GetBaseUrl()}/{parent}/documents{queryString}";

        return await GetAsync<ListDocumentsResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets information about a specific <see cref="Document"/> resource.
    /// </summary>
    /// <param name="name">The name of the <see cref="Document"/> to retrieve. Required.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The <see cref="Document"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#method:-corpora.documents.get">See Official API Documentation</seealso>
    public async Task<Document?> GetDocumentAsync(string name, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{name}";
        return await GetAsync<Document>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a <see cref="Document"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="Document"/>.</param>
    /// <param name="document">The <see cref="Document"/> resource to update.</param>
    /// <param name="updateMask">The list of fields to update. Required.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The updated <see cref="Document"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#method:-corpora.documents.patch">See Official API Documentation</seealso>
    public async Task<Document?> UpdateDocumentAsync(string name, Document document, string updateMask, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{name}";

        var queryParams = new List<string>
        {
            $"updateMask={updateMask}"
        };

        var queryString = "?" + string.Join("&", queryParams);

        return await SendAsync<Document, Document>(url + queryString, document, new HttpMethod("PATCH"), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a <see cref="Document"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="Document"/> to delete. Required.</param>
    /// <param name="force">If set to true, any <see cref="Chunk"/> resources and objects related to this <see cref="Document"/> will also be deleted. Optional.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://ai.google.dev/api/semantic-retrieval/documents#method:-corpora.documents.delete">See Official API Documentation</seealso>
    public async Task DeleteDocumentAsync(string name, bool? force = null, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{name}";

        var queryParams = new List<string>();

        if (force.HasValue)
        {
            queryParams.Add($"force={force.Value}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

        await DeleteAsync(url + queryString, cancellationToken).ConfigureAwait(false);
    }
    
    protected override Task AddAuthorizationHeader(HttpRequestMessage request, bool requiredAccessToken = false,
        CancellationToken cancellationToken = default)
    {
        return base.AddAuthorizationHeader(request, true, cancellationToken);
    }
}