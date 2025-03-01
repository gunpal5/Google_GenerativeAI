using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// A client for interacting with the RAG Engine API for file management.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
public class FileManagementClient : BaseClient
{
    public FileManagementClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Uploads a RAG file to the specified RAG corpus.
    /// </summary>
    /// <param name="corpusName">The name of the target RAG corpus to which the file will be uploaded.</param>
    /// <param name="filePath">The file path of the RAG file to be uploaded.</param>
    /// <param name="displayName">An optional display name for the RAG file.</param>
    /// <param name="description">An optional description for the RAG file.</param>
    /// <param name="uploadRagFileConfig">The configuration data for uploading the RAG file.</param>
    /// <param name="progressCallback">An optional callback to report upload progress as a percentage.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the upload operation.</param>
    /// <returns>The response containing details of the uploaded <see cref="RagFile"/>.</returns>
    public async Task<RagFile?> UploadRagFileAsync(string corpusName, string filePath,
        string? displayName = null, string? description = null, UploadRagFileConfig uploadRagFileConfig = null,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        var url =
            $"{_platform.GetBaseUrl(appendPublisher: false)}/{corpusName.ToRagCorpusId()}/ragFiles:upload?alt=json&uploadType=multipart";

        var version = _platform.GetApiVersion();
        url = url.Replace($"/{version}", $"/upload/{version}");
        //Validate File
        // ValidateFile(filePath);

        var request = new UploadRagFileRequest()
        {
           RagFile = new RagFile()
           {
               DisplayName = displayName,
               Description = description
           },
           UploadRagFileConfig = uploadRagFileConfig
        };
        if (progressCallback == null)
            progressCallback = d => { };

        var json = JsonSerializer.Serialize(request, SerializerOptions);
        //Upload File
        using var file = File.OpenRead(filePath);
        var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);
        httpMessage.Headers.Add("X-Goog-Upload-Protocol", "multipart");
        var multipart = new MultipartContent("related");
        multipart.Add(new StringContent(json, Encoding.UTF8, "application/json"));
        var content = new ProgressStreamContent(file, progressCallback);
        content.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(filePath));
        content.Headers.ContentLength = file.Length;
        multipart.Add(content);
        httpMessage.Content = multipart;
        await _platform.AddAuthorizationAsync(httpMessage, true, cancellationToken).ConfigureAwait(false);
        var response = await HttpClient.SendAsync(httpMessage,cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);

        var fileResponse = await Deserialize<UploadRagFileResponse>(response).ConfigureAwait(false);
        if (fileResponse.Error != null)
            throw new VertexAIException(fileResponse.Error.Message, fileResponse.Error);
        return fileResponse.RagFile;
    }

    /// <summary>
    /// Imports RAG files from the specified source into the RAG corpus.
    /// </summary>
    /// <param name="parent">The name of the target <see cref="RagCorpus"/> resource where the RAG files will be imported.</param>
    /// <param name="request">The request object containing the details and configuration for importing the RAG files.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the import operation if needed.</param>
    /// <returns>A task representing the asynchronous operation, resulting in a <see cref="GoogleLongRunningOperation"/>.</returns>
    public async Task<GoogleLongRunningOperation> ImportRagFilesAsync(string parent, ImportRagFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl(appendPublisher:false)}/{parent.ToRagCorpusId()}/ragFiles:import";

        return await SendAsync<ImportRagFilesRequest, GoogleLongRunningOperation>(url, request, HttpMethod.Post,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists available RAG files in the specified RAG corpus.
    /// </summary>
    /// <param name="parent">The name of the <see cref="RagCorpus"/> resource.</param>
    /// <param name="pageSize">The maximum number of RAG files to return.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListRagFilesAsync"/> call.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A list of RAG files.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<ListRagFilesResponse?> ListRagFilesAsync(string parent, int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
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
        var url = $"{_platform.GetBaseUrl(appendPublisher:false)}/{parent.ToRagCorpusId()}/ragFiles{queryString}";
    
        return await GetAsync<ListRagFilesResponse>(url, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Gets a specific RAG file.
    /// </summary>
    /// <param name="name">The name of the <see cref="RagFile"/> resource.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="RagFile"/> resource.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<RagFile?> GetRagFileAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl(appendPublisher:false);
        var url = $"{baseUrl}/{name.ToRagFileId()}";
        return await GetAsync<RagFile>(url, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Deletes a RAG file.
    /// </summary>
    /// <param name="name">The name of the <see cref="RagFile"/> resource to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task DeleteRagFileAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl(appendPublisher:false);
        var url = $"{baseUrl}/{name.ToRagFileId()}";
        await DeleteAsync(url, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}