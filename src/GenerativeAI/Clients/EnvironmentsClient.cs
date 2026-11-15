using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Logging;
using GenerativeAI.Types;
using GenerativeAI.Types.Converters;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Environments API (<c>/environments</c>): sandbox environments used by agent interactions, with
/// mounted sources (e.g. your own GCS bucket, inline files, repositories) and workspace files that can be listed,
/// uploaded (resumable upload, optionally extracting a tar archive) and downloaded. Follows python-genai's
/// <c>client.environments</c> (experimental).
/// </summary>
public class EnvironmentsClient : AgentApiClientBase
{
    private const string Collection = "environments";

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentsClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter (Google AI or Vertex AI).</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public EnvironmentsClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
    }

    /// <summary>
    /// Creates an environment (<c>POST /environments</c>).
    /// </summary>
    /// <param name="request">The request (all fields optional).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created environment.</returns>
    public Task<AgentEnvironment> CreateAsync(CreateEnvironmentRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<CreateEnvironmentRequest, AgentEnvironment>(HttpMethod.Post,
            GetCollectionUrl(Collection), request ?? new CreateEnvironmentRequest(), cancellationToken);
    }

    /// <summary>
    /// Gets an environment (<c>GET /environments/{id}</c>).
    /// </summary>
    /// <param name="id">The environment ID (an <c>environments/</c> prefix is accepted).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The environment.</returns>
    public Task<AgentEnvironment> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<AgentEnvironment>(HttpMethod.Get, GetResourceUrl(Collection, id), cancellationToken);
    }

    /// <summary>
    /// Deletes an environment (<c>DELETE /environments/{id}</c>).
    /// </summary>
    /// <param name="id">The environment ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the environment has been deleted.</returns>
    public new async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(GetResourceUrl(Collection, id), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists one page of environments (<c>GET /environments</c>).
    /// </summary>
    /// <param name="pageSize">The maximum number of environments per page.</param>
    /// <param name="pageToken">The page token of a previous call.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListEnvironmentsResponse> ListAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        return SendRequestAsync<ListEnvironmentsResponse>(HttpMethod.Get,
            GetCollectionUrl(Collection) + BuildQuery(PageParameters(pageSize, pageToken)), cancellationToken);
    }

    /// <summary>
    /// Lists all environments, following page tokens.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The environments.</returns>
    public IAsyncEnumerable<AgentEnvironment> ListAllAsync(int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListEnvironmentsResponse, AgentEnvironment>((token, ct) => ListAsync(pageSize, token, ct),
            page => page.Environments, page => page.NextPageToken, cancellationToken);
    }

    /// <summary>
    /// Lists one page of the contents of a directory (or a single file) in an environment workspace
    /// (<c>GET /environments/{environment}/files/{path}</c>).
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <param name="path">The path relative to the workspace root (empty for the root).</param>
    /// <param name="recursive">Whether to list subdirectories recursively.</param>
    /// <param name="pageSize">The maximum number of entries per page.</param>
    /// <param name="pageToken">The page token of a previous call.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The page.</returns>
    public Task<ListEnvironmentFilesResponse> ListFilesAsync(string environmentId, string? path = null,
        bool? recursive = null, int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var query = PageParameters(pageSize, pageToken);
        query.Add(new("recursive", FormatBool(recursive)));
        return SendRequestAsync<ListEnvironmentFilesResponse>(HttpMethod.Get,
            GetFileUrl(environmentId, path) + BuildQuery(query), cancellationToken);
    }

    /// <summary>
    /// Lists all entries under a path in an environment workspace, following page tokens.
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <param name="path">The path relative to the workspace root (empty for the root).</param>
    /// <param name="recursive">Whether to list subdirectories recursively.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entries.</returns>
    public IAsyncEnumerable<EnvironmentFile> ListAllFilesAsync(string environmentId, string? path = null,
        bool? recursive = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        return PaginateAsync<ListEnvironmentFilesResponse, EnvironmentFile>(
            (token, ct) => ListFilesAsync(environmentId, path, recursive, pageSize, token, ct),
            page => page.Files, page => page.NextPageToken, cancellationToken);
    }

    /// <summary>
    /// Downloads a file from an environment workspace
    /// (<c>GET /environments/{environment}/files/{path}?alt=media</c>).
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <param name="path">The file path relative to the workspace root.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The file content.</returns>
    public async Task<byte[]> DownloadFileAsync(string environmentId, string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("The file path is required.", nameof(path));
        var url = GetFileUrl(environmentId, path) + "?alt=media";
        Logger?.LogHttpRequest("GET", url, null);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendRawAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Uploads a local file into an environment workspace (see
    /// <see cref="UploadFileAsync(string, string, Stream, string?, bool?, bool?, Action{double}?, CancellationToken)"/>).
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <param name="path">The destination path relative to the workspace root.</param>
    /// <param name="filePath">The local file.</param>
    /// <param name="mimeType">The MIME type (guessed from the file name when omitted).</param>
    /// <param name="overwrite">Whether to overwrite an existing file.</param>
    /// <param name="extract">Whether to treat the file as a tar/tar.gz archive and unpack it into <paramref name="path"/>.</param>
    /// <param name="progressCallback">Optional upload progress callback (percentage).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The uploaded file(s).</returns>
    public async Task<ListEnvironmentFilesResponse> UploadFileAsync(string environmentId, string path,
        string filePath, string? mimeType = null, bool? overwrite = null, bool? extract = null,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException($"{filePath} is not a valid file path.", filePath);
        mimeType ??= MimeTypeMap.GetMimeType(filePath);
#if NET6_0_OR_GREATER
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        await using var _ = stream.ConfigureAwait(false);
#else
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
#endif
        return await UploadFileAsync(environmentId, path, stream, mimeType, overwrite, extract, progressCallback,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads a file into an environment workspace with the resumable upload protocol: a <c>start</c> request to
    /// <c>PUT /upload/{version}/environments/{environment}/files/{path}</c> followed by an <c>upload, finalize</c>
    /// request with the bytes to the returned session URL.
    /// </summary>
    /// <param name="environmentId">The environment ID.</param>
    /// <param name="path">The destination path relative to the workspace root.</param>
    /// <param name="content">The content, read from its current position. Like the other upload methods of this SDK, the stream is disposed once the upload request completes.</param>
    /// <param name="mimeType">The MIME type (default <c>application/octet-stream</c>).</param>
    /// <param name="overwrite">Whether to overwrite an existing file.</param>
    /// <param name="extract">Whether to treat the content as a tar/tar.gz archive and unpack it into <paramref name="path"/>.</param>
    /// <param name="progressCallback">Optional upload progress callback (percentage).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The uploaded file(s).</returns>
    public async Task<ListEnvironmentFilesResponse> UploadFileAsync(string environmentId, string path,
        Stream content, string? mimeType = null, bool? overwrite = null, bool? extract = null,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(content);
#else
        if (content == null) throw new ArgumentNullException(nameof(content));
#endif
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("The destination path is required.", nameof(path));
        mimeType = string.IsNullOrEmpty(mimeType) ? "application/octet-stream" : mimeType;

        // The resumable protocol needs the total size up front, so non-seekable streams are buffered.
        var stream = content;
        MemoryStream? buffered = null;
        if (!content.CanSeek)
        {
            buffered = new MemoryStream();
#if NET6_0_OR_GREATER
            await content.CopyToAsync(buffered, cancellationToken).ConfigureAwait(false);
#else
            await content.CopyToAsync(buffered).ConfigureAwait(false);
#endif
            buffered.Position = 0;
            stream = buffered;
        }

        try
        {
            var size = stream.Length - stream.Position;
            var query = new List<KeyValuePair<string, string?>>
            {
                new("extract", FormatBool(extract)),
                new("overwrite", FormatBool(overwrite))
            };
            var startUrl =
                $"{Platform.GetBaseUrl(false)}/upload/{Platform.GetApiVersion()}/{Collection}/" +
                $"{Uri.EscapeDataString(NormalizeId(Collection, environmentId))}/files/{EscapePath(path)}" +
                BuildQuery(query);

            string uploadUrl;
            Logger?.LogHttpRequest("PUT", startUrl, null);
            using (var startRequest = new HttpRequestMessage(HttpMethod.Put, startUrl))
            {
                startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Protocol", "resumable");
                startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Command", "start");
                startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Header-Content-Length",
                    size.ToString(CultureInfo.InvariantCulture));
                startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Header-Content-Type", mimeType);
                using var startResponse = await SendRawAsync(startRequest, HttpCompletionOption.ResponseContentRead,
                    cancellationToken).ConfigureAwait(false);
                if (!startResponse.Headers.TryGetValues("X-Goog-Upload-URL", out var values))
                    throw new GenerativeAIException("Failed to upload file to the environment.",
                        "Upload URL was not returned from the upload request.");
                uploadUrl = values.First();
            }

            using var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            uploadRequest.Content = new ProgressStreamContent(stream, progressCallback ?? (_ => { }))
            {
                Headers = { ContentLength = size }
            };
            uploadRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Command", "upload, finalize");
            uploadRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Offset", "0");
            using var uploadResponse = await SendRawAsync(uploadRequest, HttpCompletionOption.ResponseContentRead,
                cancellationToken).ConfigureAwait(false);
            var json = await ReadContentAsStringAsync(uploadResponse.Content, cancellationToken).ConfigureAwait(false);
            Logger?.LogSuccessfulHttpResponse(startUrl, json);
            return ParseUploadResponse(json);
        }
        finally
        {
            if (buffered != null)
            {
#if NET6_0_OR_GREATER
                await buffered.DisposeAsync().ConfigureAwait(false);
#else
                buffered.Dispose();
#endif
            }
        }
    }

    /// <summary>
    /// Normalizes the upload response like python-genai: a file list, a single file, or <c>{"file": {...}}</c>.
    /// </summary>
    private ListEnvironmentFilesResponse ParseUploadResponse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new ListEnvironmentFilesResponse();

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return new ListEnvironmentFilesResponse();
        }

        if (node is not JsonObject obj)
            return new ListEnvironmentFilesResponse();
        if (obj["files"] is JsonArray)
            return obj.Deserialize(InteractionJson.GetTypeInfo<ListEnvironmentFilesResponse>(Options)) ??
                   new ListEnvironmentFilesResponse();

        var fileNode = obj.ContainsKey("name") || obj.ContainsKey("path") ? obj : obj["file"] as JsonObject;
        if (fileNode == null)
            return new ListEnvironmentFilesResponse();
        var file = fileNode.Deserialize(InteractionJson.GetTypeInfo<EnvironmentFile>(Options));
        return new ListEnvironmentFilesResponse { Files = file == null ? new() : new() { file } };
    }

    private string GetFileUrl(string environmentId, string? path)
    {
        return $"{GetResourceUrl(Collection, environmentId)}/files/{EscapePath(path)}";
    }

    private static string EscapePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return string.Join("/", path!.TrimStart('/').Split('/').Select(Uri.EscapeDataString));
    }
}
