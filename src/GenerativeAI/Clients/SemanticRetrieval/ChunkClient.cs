using GenerativeAI.Core;
using GenerativeAI.Types.SemanticRetrieval.Chunks;
using GenerativeAI.Types.SemanticRetrieval.Document;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Chunks API.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks">See Official API Documentation</seealso>
public class ChunkClient : BaseClient
{
    public ChunkClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a new <see cref="Chunk"/> resource.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Document"/> where this <see cref="Chunk"/> will be created. Example: <c>corpora/my-corpus-123/documents/the-doc-abc</c></param>
    /// <param name="chunk">The <see cref="Chunk"/> to create.</param>
    /// <returns>The created <see cref="Chunk"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/create">See Official API Documentation</seealso>
    public async Task<Chunk?> CreateChunkAsync(string parent, Chunk chunk)
    {
        var url = $"{_platform.GetBaseUrl()}/{parent}/chunks";
        return await SendAsync<Chunk, Chunk>(url, chunk, HttpMethod.Post);
    }

    /// <summary>
    /// Lists available <see cref="Chunk"/> resources.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Document"/> containing <see cref="Chunk"/>s. Example: <c>corpora/my-corpus-123/documents/the-doc-abc</c></param>
    /// <param name="pageSize">The maximum number of <see cref="Chunk"/> resources to return.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListChunksAsync"/> call.</param>
    /// <returns>A list of <see cref="Chunk"/> resources.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/list">See Official API Documentation</seealso>
    public async Task<ListChunksResponse?> ListChunksAsync(string parent, int? pageSize = null, string? pageToken = null)
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
        var url = $"{_platform.GetBaseUrl()}/{parent}/chunks{queryString}";

        return await GetAsync<ListChunksResponse>(url);
    }

    /// <summary>
    /// Gets a specific <see cref="Chunk"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="Chunk"/> to retrieve. Example: <c>corpora/my-corpus-123/documents/the-doc-abc/chunks/some-chunk</c></param>
    /// <returns>The <see cref="Chunk"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/get">See Official API Documentation</seealso>
    public async Task<Chunk?> GetChunkAsync(string name)
    {
        var url = $"{_platform.GetBaseUrl()}/{name}";
        return await GetAsync<Chunk>(url);
    }

    /// <summary>
    /// Updates a <see cref="Chunk"/> resource.
    /// </summary>
    /// <param name="chunk">The <see cref="Chunk"/> resource to update.</param>
    /// <param name="updateMask">The list of fields to update. Currently, this only supports updating <c>customMetadata</c> and <c>data</c>.</param>
    /// <returns>The updated <see cref="Chunk"/> resource.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/patch">See Official API Documentation</seealso>
    public async Task<Chunk?> UpdateChunkAsync(Chunk chunk, string updateMask)
    {
        var url = $"{_platform.GetBaseUrl()}/{chunk.Name}";

        var queryParams = new List<string>
        {
            $"updateMask={updateMask}"
        };

        var queryString = string.Join("&", queryParams);

        return await SendAsync<Chunk, Chunk>($"{url}?{queryString}", chunk, new HttpMethod("PATCH"));
    }

    /// <summary>
    /// Deletes a <see cref="Chunk"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="Chunk"/> to delete. Example: <c>corpora/my-corpus-123/documents/the-doc-abc/chunks/some-chunk</c></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/delete">See Official API Documentation</seealso>
    public async Task DeleteChunkAsync(string name)
    {
        var url = $"{_platform.GetBaseUrl()}/{name}";
        await DeleteAsync(url);
    }

    /// <summary>
    /// Batches create <see cref="Chunk"/> resources.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Document"/> where this batch of <see cref="Chunk"/>s will be created. Example: <c>corpora/my-corpus-123/documents/the-doc-abc</c></param>
    /// <param name="requests">The request messages specifying the <see cref="Chunk"/>s to create. A maximum of 100 <see cref="Chunk"/>s can be created in a batch.</param>
    /// <returns>A list of created <see cref="Chunk"/>s.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/batchCreate">See Official API Documentation</seealso>
    public async Task<BatchCreateChunksResponse?> BatchCreateChunksAsync(string parent, List<CreateChunkRequest> requests)
    {
        var url = $"{_platform.GetBaseUrl()}/{parent}/chunks:batchCreate";
        var requestBody = new { requests };
        return await SendAsync<object, BatchCreateChunksResponse>(url, requestBody, HttpMethod.Post);
    }

    /// <summary>
    /// Batches update <see cref="Chunk"/> resources.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Document"/> containing the <see cref="Chunk"/>s to update. Example: <c>corpora/my-corpus-123/documents/the-doc-abc</c></param>
    /// <param name="requests">The request messages specifying the <see cref="Chunk"/>s to update. A maximum of 100 <see cref="Chunk"/>s can be updated in a batch.</param>
    /// <returns>A list of updated <see cref="Chunk"/>s.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/batchUpdate">See Official API Documentation</seealso>
    public async Task<BatchUpdateChunksResponse?> BatchUpdateChunksAsync(string parent, List<UpdateChunkRequest> requests)
    {
        var url = $"{_platform.GetBaseUrl()}/{parent}/chunks:batchUpdate";
        var requestBody = new { requests };
        return await SendAsync<object, BatchUpdateChunksResponse>(url, requestBody, HttpMethod.Post);
    }

    /// <summary>
    /// Batches delete <see cref="Chunk"/> resources.
    /// </summary>
    /// <param name="parent">The name of the <see cref="Document"/> containing the <see cref="Chunk"/>s to delete. Example: <c>corpora/my-corpus-123/documents/the-doc-abc</c></param>
    /// <param name="requests">The request messages specifying the <see cref="Chunk"/>s to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://ai.google.dev/api/rest/v1beta/corpora.documents.chunks/batchDelete">See Official API Documentation</seealso>
    public async Task BatchDeleteChunksAsync(string parent, List<DeleteChunkRequest> requests)
    {
        var url = $"{_platform.GetBaseUrl()}/{parent}/chunks:batchDelete";
        var requestBody = new BatchDeleteChunksRequest(requests);
        await SendAsync<BatchDeleteChunksRequest,object?>(url, requestBody, HttpMethod.Post);
    }
}
