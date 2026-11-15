using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for the Gemini API File Search stores and documents endpoints (managed retrieval-augmented generation).
/// </summary>
/// <remarks>
/// <para>
/// Typical flow: create a store with <see cref="CreateFileSearchStoreAsync(string?, string?, CancellationToken)"/>,
/// add files with <see cref="UploadToFileSearchStoreAsync(string, string, UploadToFileSearchStoreConfig?, Action{double}?, CancellationToken)"/>
/// or <see cref="ImportFileAsync"/>, wait for indexing with <see cref="WaitForOperationAsync"/>, then pass the store
/// name to the <see cref="FileSearch"/> tool (for example via <see cref="GenerativeModel.UseFileSearch(string[])"/>).
/// </para>
/// <para>File Search is only available on the Gemini Developer API; every method throws
/// <see cref="NotSupportedException"/> when the client is configured for Vertex AI.</para>
/// </remarks>
/// <seealso href="https://ai.google.dev/api/file-search/file-search-stores">See Official API Documentation</seealso>
public class FileSearchStoresClient : BaseClient
{
    private const string StorePrefix = "fileSearchStores/";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSearchStoresClient"/> class.
    /// </summary>
    /// <param name="platform">The platform adapter used for URLs and authorization.</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    /// <param name="logger">Optional logger.</param>
    public FileSearchStoresClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null)
        : base(platform, httpClient, logger)
    {
        // Make the File Search types resolvable (also under Native AOT, where reflection is unavailable).
        if (SerializerOptions != null && !SerializerOptions.IsReadOnly &&
            !SerializerOptions.TypeInfoResolverChain.Contains(FileSearchSerializerContext.Default))
        {
            SerializerOptions.TypeInfoResolverChain.Insert(0, FileSearchSerializerContext.Default);
        }
    }

    #region Stores

    /// <summary>
    /// Creates an empty File Search store.
    /// </summary>
    /// <param name="displayName">Optional human-readable display name (at most 512 characters).</param>
    /// <param name="embeddingModel">Optional embedding model, e.g. <c>gemini-embedding-001</c> or <c>models/gemini-embedding-001</c>. The service default is used when omitted.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The created <see cref="FileSearchStore"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.create">See Official API Documentation</seealso>
    public async Task<FileSearchStore> CreateFileSearchStoreAsync(string? displayName = null,
        string? embeddingModel = null, CancellationToken cancellationToken = default)
    {
        return await CreateFileSearchStoreAsync(new FileSearchStore
        {
            DisplayName = displayName,
            EmbeddingModel = embeddingModel
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a File Search store from the given definition (only <see cref="FileSearchStore.DisplayName"/> and
    /// <see cref="FileSearchStore.EmbeddingModel"/> are sent).
    /// </summary>
    /// <param name="store">The store definition.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The created <see cref="FileSearchStore"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.create">See Official API Documentation</seealso>
    public async Task<FileSearchStore> CreateFileSearchStoreAsync(FileSearchStore store,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(store);
#else
        if (store == null) throw new ArgumentNullException(nameof(store));
#endif
        EnsureSupported();
        var body = new FileSearchStore
        {
            DisplayName = store.DisplayName,
            EmbeddingModel = string.IsNullOrEmpty(store.EmbeddingModel) ? null : store.EmbeddingModel!.ToModelId()
        };
        var url = $"{Platform.GetBaseUrl()}/fileSearchStores";
        return await SendAsync<FileSearchStore, FileSearchStore>(url, body, HttpMethod.Post, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a File Search store.
    /// </summary>
    /// <param name="name">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The <see cref="FileSearchStore"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.get">See Official API Documentation</seealso>
    public async Task<FileSearchStore> GetFileSearchStoreAsync(string name, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url = $"{Platform.GetBaseUrl()}/{ToStoreName(name)}";
        return await GetAsync<FileSearchStore>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists the File Search stores owned by the project, one page at a time.
    /// </summary>
    /// <param name="pageSize">Maximum number of stores per page (the service caps this at 20).</param>
    /// <param name="pageToken">A <see cref="ListFileSearchStoresResponse.NextPageToken"/> from a previous call.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>One page of stores.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.list">See Official API Documentation</seealso>
    public async Task<ListFileSearchStoresResponse> ListFileSearchStoresAsync(int? pageSize = null,
        string? pageToken = null, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url = $"{Platform.GetBaseUrl()}/fileSearchStores{BuildPagingQuery(pageSize, pageToken)}";
        return await GetAsync<ListFileSearchStoresResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Enumerates all File Search stores, fetching subsequent pages as needed.
    /// </summary>
    /// <param name="pageSize">Optional page size used for each request.</param>
    /// <param name="cancellationToken">Token to cancel the enumeration.</param>
    /// <returns>An async sequence of every store.</returns>
    public async IAsyncEnumerable<FileSearchStore> ListAllFileSearchStoresAsync(int? pageSize = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? pageToken = null;
        do
        {
            var page = await ListFileSearchStoresAsync(pageSize, pageToken, cancellationToken).ConfigureAwait(false);
            if (page.FileSearchStores != null)
            {
                foreach (var store in page.FileSearchStores)
                    yield return store;
            }

            pageToken = page.NextPageToken;
        } while (!string.IsNullOrEmpty(pageToken));
    }

    /// <summary>
    /// Deletes a File Search store.
    /// </summary>
    /// <param name="name">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="force">When <c>true</c>, also deletes all documents in the store. When <c>false</c> or unset, the request fails if the store contains documents.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.delete">See Official API Documentation</seealso>
    public async Task DeleteFileSearchStoreAsync(string name, bool? force = null,
        CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url = $"{Platform.GetBaseUrl()}/{ToStoreName(name)}{BuildForceQuery(force)}";
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Adding files

    /// <summary>
    /// Uploads a local file directly into a File Search store, where it is chunked, embedded and indexed as a new document.
    /// </summary>
    /// <param name="fileSearchStoreName">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="filePath">Path of the local file to upload.</param>
    /// <param name="config">Optional display name, MIME type (inferred from the extension when unset), custom metadata and chunking settings.</param>
    /// <param name="progressCallback">Optional callback receiving upload progress as a percentage.</param>
    /// <param name="cancellationToken">Token to cancel the upload.</param>
    /// <returns>The long-running <see cref="FileSearchOperation"/> that tracks indexing; see <see cref="WaitForOperationAsync"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.uploadtofilesearchstore">See Official API Documentation</seealso>
    public async Task<FileSearchOperation> UploadToFileSearchStoreAsync(string fileSearchStoreName, string filePath,
        UploadToFileSearchStoreConfig? config = null, Action<double>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var effectiveConfig = CopyConfig(config);
        if (string.IsNullOrEmpty(effectiveConfig.MimeType))
            effectiveConfig.MimeType = MimeTypeMap.GetMimeType(filePath);

        using var stream = System.IO.File.OpenRead(filePath);
        return await UploadCoreAsync(fileSearchStoreName, stream, effectiveConfig, progressCallback, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads the contents of a stream directly into a File Search store as a new document.
    /// The stream is read from its current position to the end.
    /// </summary>
    /// <param name="fileSearchStoreName">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="stream">The data to upload. Like <see cref="FileClient.UploadStreamAsync"/>, the stream is disposed once the upload request completes.</param>
    /// <param name="mimeType">The MIME type of the data, e.g. <c>application/pdf</c> or <c>text/plain</c>.</param>
    /// <param name="config">Optional display name, custom metadata and chunking settings. <paramref name="mimeType"/> takes precedence over <see cref="UploadToFileSearchStoreConfig.MimeType"/>.</param>
    /// <param name="progressCallback">Optional callback receiving upload progress as a percentage.</param>
    /// <param name="cancellationToken">Token to cancel the upload.</param>
    /// <returns>The long-running <see cref="FileSearchOperation"/> that tracks indexing; see <see cref="WaitForOperationAsync"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.uploadtofilesearchstore">See Official API Documentation</seealso>
    public async Task<FileSearchOperation> UploadStreamToFileSearchStoreAsync(string fileSearchStoreName,
        Stream stream, string mimeType, UploadToFileSearchStoreConfig? config = null,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(stream);
#else
        if (stream == null) throw new ArgumentNullException(nameof(stream));
#endif
        if (string.IsNullOrEmpty(mimeType))
            throw new ArgumentException("MIME type cannot be null or empty.", nameof(mimeType));

        var effectiveConfig = CopyConfig(config);
        effectiveConfig.MimeType = mimeType;
        return await UploadCoreAsync(fileSearchStoreName, stream, effectiveConfig, progressCallback, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Imports a file previously uploaded with the Files API (see <see cref="FileClient"/>) into a File Search store.
    /// </summary>
    /// <param name="fileSearchStoreName">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="fileName">The Files API file name, either <c>files/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="config">Optional custom metadata and chunking settings.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The long-running <see cref="FileSearchOperation"/> that tracks indexing; see <see cref="WaitForOperationAsync"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/file-search-stores#method:-filesearchstores.importfile">See Official API Documentation</seealso>
    public async Task<FileSearchOperation> ImportFileAsync(string fileSearchStoreName, string fileName,
        ImportFileConfig? config = null, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        var request = new ImportFileRequest
        {
            FileName = fileName.ToFileId(),
            CustomMetadata = config?.CustomMetadata,
            ChunkingConfig = config?.ChunkingConfig
        };
        var url = $"{Platform.GetBaseUrl()}/{ToStoreName(fileSearchStoreName)}:importFile";
        return await SendAsync<ImportFileRequest, FileSearchOperation>(url, request, HttpMethod.Post, cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion

    #region Operations

    /// <summary>
    /// Gets the latest state of an upload or import operation.
    /// </summary>
    /// <param name="operationName">The operation name, e.g. <c>fileSearchStores/{store}/operations/{id}</c> or <c>fileSearchStores/{store}/upload/operations/{id}</c>.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The current <see cref="FileSearchOperation"/>.</returns>
    public async Task<FileSearchOperation> GetOperationAsync(string operationName,
        CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url = $"{Platform.GetBaseUrl()}/{ToChildName(operationName, nameof(operationName), "/operations/")}";
        return await GetAsync<FileSearchOperation>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls an upload or import operation until it is done.
    /// </summary>
    /// <param name="operation">The operation returned by an upload or import call.</param>
    /// <param name="pollInterval">Delay between polls (default 2 seconds).</param>
    /// <param name="timeout">Maximum time to wait (default 10 minutes).</param>
    /// <param name="cancellationToken">Token to cancel waiting.</param>
    /// <returns>The completed operation, whose <see cref="FileSearchOperation.Response"/> names the created document.</returns>
    /// <exception cref="GenerativeAIException">The operation finished with an error.</exception>
    /// <exception cref="TimeoutException">The operation did not finish within <paramref name="timeout"/>.</exception>
    public async Task<FileSearchOperation> WaitForOperationAsync(FileSearchOperation operation,
        TimeSpan? pollInterval = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(operation);
#else
        if (operation == null) throw new ArgumentNullException(nameof(operation));
#endif
        var interval = pollInterval ?? TimeSpan.FromSeconds(2);
        var maxWait = timeout ?? TimeSpan.FromMinutes(10);
        var stopwatch = Stopwatch.StartNew();
        var current = operation;

        while (current.Done != true)
        {
            if (string.IsNullOrEmpty(current.Name))
                throw new InvalidOperationException("The operation has no name and cannot be polled.");
            if (stopwatch.Elapsed >= maxWait)
                throw new TimeoutException($"Operation '{current.Name}' did not complete within {maxWait}.");

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            current = await GetOperationAsync(current.Name!, cancellationToken).ConfigureAwait(false);
        }

        if (current.Error != null)
        {
            throw new GenerativeAIException($"File Search operation '{current.Name}' failed.",
                current.Error.Message ?? $"Error code {current.Error.Code}");
        }

        return current;
    }

    #endregion

    #region Documents

    /// <summary>
    /// Gets a document from a File Search store.
    /// </summary>
    /// <param name="documentName">The full document name: <c>fileSearchStores/{store}/documents/{document}</c>.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The <see cref="FileSearchDocument"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/documents#method:-filesearchstores.documents.get">See Official API Documentation</seealso>
    public async Task<FileSearchDocument> GetDocumentAsync(string documentName,
        CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url = $"{Platform.GetBaseUrl()}/{ToChildName(documentName, nameof(documentName), "/documents/")}";
        return await GetAsync<FileSearchDocument>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists the documents in a File Search store, one page at a time.
    /// </summary>
    /// <param name="fileSearchStoreName">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="pageSize">Maximum number of documents per page (the service caps this at 20).</param>
    /// <param name="pageToken">A <see cref="ListFileSearchDocumentsResponse.NextPageToken"/> from a previous call.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>One page of documents.</returns>
    /// <seealso href="https://ai.google.dev/api/file-search/documents#method:-filesearchstores.documents.list">See Official API Documentation</seealso>
    public async Task<ListFileSearchDocumentsResponse> ListDocumentsAsync(string fileSearchStoreName,
        int? pageSize = null, string? pageToken = null, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url =
            $"{Platform.GetBaseUrl()}/{ToStoreName(fileSearchStoreName)}/documents{BuildPagingQuery(pageSize, pageToken)}";
        return await GetAsync<ListFileSearchDocumentsResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Enumerates all documents in a File Search store, fetching subsequent pages as needed.
    /// </summary>
    /// <param name="fileSearchStoreName">The store name, either <c>fileSearchStores/{id}</c> or just <c>{id}</c>.</param>
    /// <param name="pageSize">Optional page size used for each request.</param>
    /// <param name="cancellationToken">Token to cancel the enumeration.</param>
    /// <returns>An async sequence of every document in the store.</returns>
    public async IAsyncEnumerable<FileSearchDocument> ListAllDocumentsAsync(string fileSearchStoreName,
        int? pageSize = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? pageToken = null;
        do
        {
            var page = await ListDocumentsAsync(fileSearchStoreName, pageSize, pageToken, cancellationToken)
                .ConfigureAwait(false);
            if (page.Documents != null)
            {
                foreach (var document in page.Documents)
                    yield return document;
            }

            pageToken = page.NextPageToken;
        } while (!string.IsNullOrEmpty(pageToken));
    }

    /// <summary>
    /// Deletes a document from a File Search store.
    /// </summary>
    /// <param name="documentName">The full document name: <c>fileSearchStores/{store}/documents/{document}</c>.</param>
    /// <param name="force">When <c>true</c>, also deletes the document's chunks. When <c>false</c> or unset, the request fails if the document has chunks.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <seealso href="https://ai.google.dev/api/file-search/documents#method:-filesearchstores.documents.delete">See Official API Documentation</seealso>
    public async Task DeleteDocumentAsync(string documentName, bool? force = null,
        CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var url =
            $"{Platform.GetBaseUrl()}/{ToChildName(documentName, nameof(documentName), "/documents/")}{BuildForceQuery(force)}";
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Media

    /// <summary>
    /// Downloads media referenced by File Search grounding metadata.
    /// </summary>
    /// <param name="mediaId">The media name, in the form <c>fileSearchStores/{store}/media/{blob}</c>.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The raw media bytes.</returns>
    public async Task<byte[]> DownloadMediaAsync(string mediaId, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        var name = ToChildName(mediaId, nameof(mediaId), "/media/");
        var url = $"{Platform.GetBaseUrl()}/{name}?alt=media";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthorizationHeader(request, false, cancellationToken).ConfigureAwait(false);
        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);
#if NET5_0_OR_GREATER
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Runs the two-step resumable upload protocol used by <c>uploadToFileSearchStore</c>:
    /// a <c>start</c> request carrying the JSON metadata, followed by an <c>upload, finalize</c>
    /// request with the bytes to the session URL returned in <c>X-Goog-Upload-URL</c>.
    /// </summary>
    private async Task<FileSearchOperation> UploadCoreAsync(string fileSearchStoreName, Stream source,
        UploadToFileSearchStoreConfig config, Action<double>? progressCallback, CancellationToken cancellationToken)
    {
        EnsureSupported();
        var storeName = ToStoreName(fileSearchStoreName);

        // The resumable protocol needs the total size up front, so non-seekable (or partially read)
        // streams are buffered. The stream is disposed together with the upload request content.
        var stream = source;
        if (!source.CanSeek || source.Position != 0)
        {
#pragma warning disable CA2000 // Disposed via ProgressStreamContent when the upload request is disposed
            var buffered = new MemoryStream();
#pragma warning restore CA2000
#if NET6_0_OR_GREATER
            await source.CopyToAsync(buffered, cancellationToken).ConfigureAwait(false);
#else
            await source.CopyToAsync(buffered).ConfigureAwait(false);
#endif
            buffered.Position = 0;
            stream = buffered;
        }

        var size = stream.Length;
        var baseUrl = Platform.GetBaseUrl(false);
        var apiVersion = Platform.GetApiVersion();
        var startUrl = $"{baseUrl}/upload/{apiVersion}/{storeName}:uploadToFileSearchStore";

        if (SerializerOptions == null)
            throw new InvalidOperationException("SerializerOptions is not initialized");
#if NET11_0_OR_GREATER
        var typeInfo = SerializerOptions.GetTypeInfo<UploadToFileSearchStoreConfig>();
#else
        var typeInfo = SerializerOptions.GetTypeInfo(typeof(UploadToFileSearchStoreConfig));
#endif
        var json = JsonSerializer.Serialize(config, typeInfo);

        string uploadUrl;
        using (var startRequest = new HttpRequestMessage(HttpMethod.Post, startUrl))
        {
            startRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Protocol", "resumable");
            startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Command", "start");
            startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Header-Content-Length",
                size.ToString(System.Globalization.CultureInfo.InvariantCulture));
            startRequest.Headers.TryAddWithoutValidation("X-Goog-Upload-Header-Content-Type", config.MimeType);
            await AddAuthorizationHeader(startRequest, false, cancellationToken).ConfigureAwait(false);

            using var startResponse = await HttpClient.SendAsync(startRequest, cancellationToken).ConfigureAwait(false);
            await CheckAndHandleErrors(startResponse, startUrl).ConfigureAwait(false);
            if (!startResponse.Headers.TryGetValues("X-Goog-Upload-URL", out var values))
                throw new GenerativeAIException("Failed to upload file to File Search store.",
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
        await AddAuthorizationHeader(uploadRequest, false, cancellationToken).ConfigureAwait(false);

        using var uploadResponse = await HttpClient.SendAsync(uploadRequest, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(uploadResponse, startUrl).ConfigureAwait(false);
        var operation = await Deserialize<FileSearchOperation>(uploadResponse).ConfigureAwait(false);
        return operation ?? throw new InvalidOperationException(
            "Failed to upload file to File Search store. The server returned no operation.");
    }

    private void EnsureSupported()
    {
        if (Platform is VertextPlatformAdapter)
            throw new NotSupportedException("File Search stores are only supported by the Gemini Developer API, not Vertex AI.");
    }

    private static UploadToFileSearchStoreConfig CopyConfig(UploadToFileSearchStoreConfig? config)
    {
        return new UploadToFileSearchStoreConfig
        {
            MimeType = config?.MimeType,
            DisplayName = config?.DisplayName,
            CustomMetadata = config?.CustomMetadata,
            ChunkingConfig = config?.ChunkingConfig
        };
    }

    private static string ToStoreName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("File Search store name cannot be null or empty.", nameof(name));
        if (name.StartsWith(StorePrefix, StringComparison.Ordinal))
            return name;
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (name.Contains("/"))
#else
        if (name.Contains('/', StringComparison.Ordinal))
#endif
            throw new ArgumentException($"Invalid File Search store name: {name}", nameof(name));
        return StorePrefix + name;
    }

    private static string ToChildName(string name, string paramName, string segment)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Resource name cannot be null or empty.", paramName);
        var trimmed = name.TrimStart('/');
#if NETSTANDARD2_0 || NET462_OR_GREATER
        var hasSegment = trimmed.Contains(segment);
#else
        var hasSegment = trimmed.Contains(segment, StringComparison.Ordinal);
#endif
        if (!trimmed.StartsWith(StorePrefix, StringComparison.Ordinal) || !hasSegment)
            throw new ArgumentException(
                $"Invalid resource name '{name}'. Expected the form {StorePrefix}{{store}}{segment}{{id}}.", paramName);
        return trimmed;
    }

    private static string BuildPagingQuery(int? pageSize, string? pageToken)
    {
        var queryParams = new List<string>();
        if (pageSize.HasValue)
            queryParams.Add($"pageSize={pageSize.Value}");
        if (!string.IsNullOrEmpty(pageToken))
            queryParams.Add($"pageToken={Uri.EscapeDataString(pageToken!)}");
        return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
    }

    private static string BuildForceQuery(bool? force)
    {
        return force.HasValue ? $"?force={(force.Value ? "true" : "false")}" : string.Empty;
    }

    #endregion
}
