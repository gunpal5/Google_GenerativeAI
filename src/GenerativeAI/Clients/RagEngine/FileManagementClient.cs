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
    /// <summary>
    /// Initializes a new instance of the <see cref="FileManagementClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter for API communication.</param>
    /// <param name="httpClient">Optional HTTP client for API requests.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
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
        string? displayName = null, string? description = null, UploadRagFileConfig? uploadRagFileConfig = null,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        var url =
            $"{Platform.GetBaseUrl(appendPublisher: false)}/{corpusName.ToRagCorpusId()}/ragFiles:upload?alt=json&uploadType=multipart";

        var version = Platform.GetApiVersion();
#if NET6_0_OR_GREATER
        url = url.Replace($"/{version}", $"/upload/{version}", StringComparison.Ordinal);
#else
        url = url.Replace($"/{version}", $"/upload/{version}");
#endif
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

        if (SerializerOptions == null)
            throw new InvalidOperationException("SerializerOptions is not initialized");
        var typeInfo = SerializerOptions.GetTypeInfo(request.GetType());
        if (typeInfo == null)
            throw new InvalidOperationException($"Could not get type info for {request.GetType()}");
        var json = JsonSerializer.Serialize(request, typeInfo);
        //Upload File
        using var file = File.OpenRead(filePath);
#pragma warning disable CA2000 // Objects are disposed properly via HttpRequestMessage ownership transfer
        var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);
        httpMessage.Headers.Add("X-Goog-Upload-Protocol", "multipart");
        MultipartContent? multipart = null;
        StringContent? stringContent = null;
        ProgressStreamContent? progressContent = null;
        
        try
        {
            multipart = new MultipartContent("related");
            stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            multipart.Add(stringContent);
            
            progressContent = new ProgressStreamContent(file, progressCallback);
            progressContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(filePath));
            progressContent.Headers.ContentLength = file.Length;
            multipart.Add(progressContent);
            
            httpMessage.Content = multipart;
            // After setting content, ownership is transferred to httpMessage
            multipart = null;
            stringContent = null;
            progressContent = null;
            
            await Platform.AddAuthorizationAsync(httpMessage, true, cancellationToken).ConfigureAwait(false);
            var response = await HttpClient.SendAsync(httpMessage, cancellationToken).ConfigureAwait(false);
            await CheckAndHandleErrors(response, url).ConfigureAwait(false);

            var fileResponse = await Deserialize<UploadRagFileResponse>(response).ConfigureAwait(false);
            if (fileResponse != null && fileResponse.Error != null)
                throw new VertexAIException(fileResponse.Error.Message ?? "Unknown error", fileResponse.Error);
            
            return fileResponse?.RagFile;
        }
        catch
        {
            // HttpRequestMessage disposal will handle content disposal
            httpMessage.Dispose();
            throw;
        }
#pragma warning restore CA2000
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
        var url = $"{Platform.GetBaseUrl(appendPublisher: false)}/{parent.ToRagCorpusId()}/ragFiles:import";

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
        var url = $"{Platform.GetBaseUrl(appendPublisher: false)}/{parent.ToRagCorpusId()}/ragFiles{queryString}";
    
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
        var baseUrl = Platform.GetBaseUrl(appendPublisher: false);
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
        var baseUrl = Platform.GetBaseUrl(appendPublisher: false);
        var url = $"{baseUrl}/{name.ToRagFileId()}";
        await DeleteAsync(url, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}