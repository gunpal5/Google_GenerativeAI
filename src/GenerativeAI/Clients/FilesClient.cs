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
/// A client for interacting with the Gemini API Files endpoint.
/// </summary>
/// <seealso href="https://ai.google.dev/api/files">See Official API Documentation</seealso>
public class FileClient : BaseClient
{
    /// <summary>
    /// A client for interacting with the Gemini API Files endpoint.
    /// </summary>
    /// <remarks>
    /// Provides methods for uploading, retrieving, listing, and deleting files, as well as awaiting file state changes.
    /// </remarks>
    public FileClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform,
        httpClient, logger)
    {
    }

    /// <summary>
    /// Uploads a file to the remote server and creates a <see cref="RemoteFile"/> object.
    /// </summary>
    /// <param name="filePath">The path to the file to upload.</param>
    /// <param name="progressCallback">An optional callback to report the upload progress.</param>
    /// <param name="cancellationToken">An optional token to cancel the upload operation.</param>
    /// <returns>The uploaded file's information as a <see cref="RemoteFile"/> object.</returns>
    public async Task<RemoteFile> UploadFileAsync(string filePath, Action<double>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = Platform.GetBaseUrl(false);
        var apiVersion = Platform.GetApiVersion();
        var url = $"{baseUrl}/upload/{apiVersion}/files?alt=json&uploadType=multipart";

        //Validate File
        ValidateFile(filePath);

        var request = new UploadFileRequest()
        {
            File = new UploadFileInformation()
            {
                DisplayName = Path.GetFileNameWithoutExtension(filePath)
            }
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
        MultipartContent? multipart = null;
        StringContent? stringContent = null;
        ProgressStreamContent? progressContent = null;

        try
        {
            multipart = new MultipartContent("related");
            stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            multipart.Add(stringContent);

            progressContent = new ProgressStreamContent(file, progressCallback)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(filePath)),
                    ContentLength = file.Length
                }
            };
            multipart.Add(progressContent);

            httpMessage.Content = multipart;
            // After setting content, ownership is transferred to httpMessage
            multipart = null;
            stringContent = null;
            progressContent = null;

            await Platform.AddAuthorizationAsync(httpMessage, false, cancellationToken).ConfigureAwait(false);
            var response = await HttpClient.SendAsync(httpMessage, cancellationToken).ConfigureAwait(false);
            await CheckAndHandleErrors(response, url).ConfigureAwait(false);

            var fileResponse = await Deserialize<UploadFileResponse>(response).ConfigureAwait(false);
            if (fileResponse?.File == null)
                throw new InvalidOperationException(
                    "Failed to upload file. The server response did not contain file information.");
            return fileResponse.File;
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
    /// Asynchronously uploads a file stream to the remote server and returns the uploaded file's metadata.
    /// </summary>
    /// <param name="stream">The file stream to be uploaded.</param>
    /// <param name="displayName">The display name for the uploaded file.</param>
    /// <param name="mimeType">The MIME type of the file to be uploaded.</param>
    /// <param name="progressCallback">
    /// An optional callback function to monitor the upload progress, with the progress represented as a percentage.
    /// </param>
    /// <param name="cancellationToken">A token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="RemoteFile"/> object representing the metadata of the uploaded file.</returns>
    public async Task<RemoteFile> UploadStreamAsync(Stream stream, string displayName, string mimeType,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(stream);
#else
        if (stream == null) throw new ArgumentNullException(nameof(stream));
#endif
        var baseUrl = Platform.GetBaseUrl(false);
        var apiVersion = Platform.GetApiVersion();
        var url = $"{baseUrl}/upload/{apiVersion}/files?alt=json&uploadType=multipart";

        //Validate File
        ValidateStream(stream, mimeType);

        var request = new UploadFileRequest()
        {
            File = new UploadFileInformation()
            {
                DisplayName = displayName
            }
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
#pragma warning disable CA2000 // Objects are disposed properly via HttpRequestMessage ownership transfer
        var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);
        MultipartContent? multipart = null;
        StringContent? stringContent = null;
        ProgressStreamContent? progressContent = null;
        
        try
        {
            multipart = new MultipartContent("related");
            stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            multipart.Add(stringContent);
            
            progressContent = new ProgressStreamContent(stream, progressCallback)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(mimeType),
                    ContentLength = stream.Length
                }
            };
            multipart.Add(progressContent);
            
            httpMessage.Content = multipart;
            // After setting content, ownership is transferred to httpMessage
            multipart = null;
            stringContent = null;
            progressContent = null;
            
            await Platform.AddAuthorizationAsync(httpMessage, false, cancellationToken).ConfigureAwait(false);
            var response = await HttpClient.SendAsync(httpMessage, cancellationToken).ConfigureAwait(false);
            await CheckAndHandleErrors(response, url).ConfigureAwait(false);

            var fileResponse = await Deserialize<UploadFileResponse>(response).ConfigureAwait(false);
            if (fileResponse?.File == null)
                throw new InvalidOperationException("Failed to upload file. The server response did not contain file information.");
            return fileResponse.File;
        }
        catch
        {
            // HttpRequestMessage disposal will handle content disposal
            httpMessage.Dispose();
            throw;
        }
#pragma warning restore CA2000
    }

    private static void ValidateStream(Stream stream, string mimeType)
    {
        if (stream.Length > FilesConstants.MaxUploadFileSize)
            throw new FileTooLargeException("stream");
        ValidateMimeType(mimeType);
    }

    private static void ValidateFile(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
            throw new FileNotFoundException("File not found", filePath);
        if (fileInfo.Length > FilesConstants.MaxUploadFileSize)
            throw new FileTooLargeException(filePath);
        var mimeType = MimeTypeMap.GetMimeType(fileInfo.Extension);
        ValidateMimeType(mimeType);
    }

    /// <summary>
    /// Validates whether the specified MIME type is among the supported types for the API.
    /// </summary>
    /// <param name="mimeType">The MIME type to validate.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when the provided MIME type is not recognized as a supported type.
    /// </exception>
    private static void ValidateMimeType(string mimeType)
    {
        if (mimeType == null)
        {
            throw new ArgumentNullException(nameof(mimeType), "MIME type cannot be null.");
        }

        var supportedMimeTypes = FilesConstants.SupportedMimeTypes;

        if (!supportedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"The provided MIME type '{mimeType}' is not supported by the system.");
        }
    }


    /// <summary>
    /// Gets the metadata for the given <see cref="RemoteFile"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="RemoteFile"/> to get.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="RemoteFile"/> information.</returns>
    /// <seealso href="https://ai.google.dev/api/files#method:-files.get">See Official API Documentation</seealso>
    public async Task<RemoteFile> GetFileAsync(string name,CancellationToken cancellationToken = default)
    {
        var baseUrl = Platform.GetBaseUrl();

        var url = $"{baseUrl}/{name.ToFileId()}";
        return await GetAsync<RemoteFile>(url,cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously retrieves a list of metadata for <see cref="RemoteFile"/>s owned by the requesting project.
    /// </summary>
    /// <param name="pageSize">The maximum number of <see cref="RemoteFile"/>s to return per request. Defaults to 10 if not specified, with a maximum limit of 100.</param>
    /// <param name="pageToken">An optional token for retrieving the next page of <see cref="RemoteFile"/>s from a previous call.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains a <see cref="ListFilesResponse"/> object with the list of <see cref="RemoteFile"/>s and associated metadata.</returns>
    public async Task<ListFilesResponse> ListFilesAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
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
        var url = $"{Platform.GetBaseUrl()}/files{queryString}";

        return await GetAsync<ListFilesResponse>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a remote file by its name asynchronously.
    /// </summary>
    /// <param name="name">The name of the remote file to be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteFileAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = Platform.GetBaseUrl();

        var url = $"{baseUrl}/{name.ToFileId()}";
        await DeleteAsync(url,cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads the content of a generated file (for example a generated video) into memory, like
    /// <c>files.download</c> of the google-genai SDK. Gemini Developer API only.
    /// </summary>
    /// <param name="name">The file name (<c>files/{id}</c> or <c>{id}</c>) or its download/URI URL.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The file content.</returns>
    /// <remarks>Files created by upload can't be downloaded; check <see cref="RemoteFile.DownloadUri"/> or <see cref="RemoteFile.Source"/>.</remarks>
    /// <exception cref="NotSupportedException">Called on Vertex AI.</exception>
    public async Task<byte[]> DownloadFileAsync(string name, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await DownloadFileAsync(name, buffer, cancellationToken).ConfigureAwait(false);
        return buffer.ToArray();
    }

    /// <summary>
    /// Downloads the content of a generated file into memory. Gemini Developer API only.
    /// </summary>
    /// <param name="file">The file to download. It must have a <see cref="RemoteFile.DownloadUri"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The file content.</returns>
    /// <exception cref="ArgumentException">The file has no download URI (uploaded files can't be downloaded).</exception>
    public async Task<byte[]> DownloadFileAsync(RemoteFile file, CancellationToken cancellationToken = default)
    {
        return await DownloadFileAsync(GetDownloadableName(file), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Streams the content of a generated file to <paramref name="destination"/> without buffering the whole file
    /// in memory. Gemini Developer API only.
    /// </summary>
    /// <param name="name">The file name (<c>files/{id}</c> or <c>{id}</c>) or its download/URI URL.</param>
    /// <param name="destination">A writable stream that receives the file content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the content has been written.</returns>
    /// <exception cref="NotSupportedException">Called on Vertex AI.</exception>
    public async Task DownloadFileAsync(string name, Stream destination, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(destination);
#else
        if (destination == null) throw new ArgumentNullException(nameof(destination));
#endif
        if (Platform is not GoogleAIPlatformAdapter)
            throw new NotSupportedException("Downloading files is only supported on the Gemini Developer API.");

        var url = $"{Platform.GetBaseUrl()}/files/{ToDownloadFileName(name)}:download?alt=media";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        await Platform.AddAuthorizationAsync(request, false, cancellationToken).ConfigureAwait(false);
        using var response = await HttpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);
#if NET5_0_OR_GREATER
        using var content = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await content.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
#else
        using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await content.CopyToAsync(destination, 81920, cancellationToken).ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Streams the content of a generated file to <paramref name="destination"/>. Gemini Developer API only.
    /// </summary>
    /// <param name="file">The file to download. It must have a <see cref="RemoteFile.DownloadUri"/>.</param>
    /// <param name="destination">A writable stream that receives the file content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the content has been written.</returns>
    /// <exception cref="ArgumentException">The file has no download URI (uploaded files can't be downloaded).</exception>
    public async Task DownloadFileAsync(RemoteFile file, Stream destination, CancellationToken cancellationToken = default)
    {
        await DownloadFileAsync(GetDownloadableName(file), destination, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads the content of a generated file to a local file path. Gemini Developer API only.
    /// </summary>
    /// <param name="name">The file name (<c>files/{id}</c> or <c>{id}</c>) or its download/URI URL.</param>
    /// <param name="destinationPath">The local file path to write to (created or overwritten).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    public async Task DownloadFileToPathAsync(string name, string destinationPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path cannot be null or empty.", nameof(destinationPath));
        using var stream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await DownloadFileAsync(name, stream, cancellationToken).ConfigureAwait(false);
    }

    private static string GetDownloadableName(RemoteFile file)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(file);
#else
        if (file == null) throw new ArgumentNullException(nameof(file));
#endif
        if (file.DownloadUri == null)
            throw new ArgumentException(
                "Only generated files can be downloaded, uploaded files can't be downloaded. You can tell which " +
                "files are downloadable by checking the Source or DownloadUri property.", nameof(file));
        return file.Name ?? file.DownloadUri;
    }

    /// <summary>
    /// Extracts the file id the same way as <c>t_file_name</c> of the google-genai SDK.
    /// </summary>
    private static string ToDownloadFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("File name is required.", nameof(name));

        if (name.StartsWith("https://", StringComparison.Ordinal))
        {
            var index = name.IndexOf("files/", StringComparison.Ordinal);
            if (index < 0)
                throw new ArgumentException($"Could not extract file name from URI: {name}", nameof(name));
            var suffix = name.Substring(index + "files/".Length);
            var length = 0;
            while (length < suffix.Length && ((suffix[length] >= 'a' && suffix[length] <= 'z') ||
                                              (suffix[length] >= '0' && suffix[length] <= '9')))
            {
                length++;
            }

            if (length == 0)
                throw new ArgumentException($"Could not extract file name from URI: {name}", nameof(name));
            return suffix.Substring(0, length);
        }

        return name.StartsWith("files/", StringComparison.Ordinal) ? name.Substring("files/".Length) : name;
    }

    /// <summary>
    /// Registers Google Cloud Storage files with the Files service so they can be used in requests, like
    /// <c>files.register_files</c> of the google-genai SDK. Gemini Developer API only.
    /// </summary>
    /// <param name="uris">The Cloud Storage URIs to register, e.g. <c>gs://bucket/object</c>.</param>
    /// <param name="accessToken">
    /// An OAuth 2.0 access token with read access to the objects. When omitted, the platform's access token
    /// (or its authenticator / Application Default Credentials) is used.
    /// </param>
    /// <param name="quotaProjectId">Optional quota project, sent as the <c>x-goog-user-project</c> header.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The registered files.</returns>
    /// <exception cref="NotSupportedException">Called on Vertex AI.</exception>
    public async Task<RegisterFilesResponse> RegisterFilesAsync(IEnumerable<string> uris, string? accessToken = null,
        string? quotaProjectId = null, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(uris);
#else
        if (uris == null) throw new ArgumentNullException(nameof(uris));
#endif
        if (Platform is not GoogleAIPlatformAdapter)
            throw new NotSupportedException("Registering files is only supported on the Gemini Developer API.");

        var url = $"{Platform.GetBaseUrl()}/files:register";
        var payload = new RegisterFilesRequest { Uris = uris.ToList() };

        if (SerializerOptions == null)
            throw new InvalidOperationException("SerializerOptions is not initialized");
#if NET11_0_OR_GREATER
        var typeInfo = SerializerOptions.GetTypeInfo<RegisterFilesRequest>();
#else
        var typeInfo = SerializerOptions.GetTypeInfo(typeof(RegisterFilesRequest));
#endif
        var json = JsonSerializer.Serialize(payload, typeInfo);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        if (string.IsNullOrEmpty(accessToken))
        {
            await Platform.AddAuthorizationAsync(request, true, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await Platform.AddAuthorizationAsync(request, false, cancellationToken).ConfigureAwait(false);
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        }

        if (!string.IsNullOrEmpty(quotaProjectId))
        {
            request.Headers.Remove("x-goog-user-project");
            request.Headers.TryAddWithoutValidation("x-goog-user-project", quotaProjectId);
        }

        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await CheckAndHandleErrors(response, url).ConfigureAwait(false);
        return await Deserialize<RegisterFilesResponse>(response).ConfigureAwait(false)
               ?? throw new InvalidOperationException("Failed to deserialize the files:register response.");
    }

    /// <summary>
    /// Awaits until a file reaches the "ACTIVE" state or times out after the specified duration.
    /// </summary>
    /// <param name="file">The file to monitor for the state transition.</param>
    /// <param name="maxSeconds">The maximum time, in seconds, to wait for the file to become active.</param>
    /// <param name="cancellationToken">A token to cancel the waiting operation.</param>
    /// <exception cref="GenerativeAIException">Thrown when the file encounters a processing error.</exception>
    /// <returns>An awaitable task that completes when the file reaches the "ACTIVE" state.</returns>
    public async Task AwaitForFileStateActiveAsync(RemoteFile file, int maxSeconds, CancellationToken cancellationToken)
    {
        if (file?.Name == null)
            throw new ArgumentNullException(nameof(file), "File or file name cannot be null");
            
        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (sw.Elapsed.TotalSeconds < maxSeconds)
        {
            var remoteFile = await GetFileAsync(file.Name, cancellationToken).ConfigureAwait(false);
            if(remoteFile.State == FileState.ACTIVE)
            {
                return;
            }
            else if (remoteFile.State != FileState.PROCESSING)
            {
                throw new GenerativeAIException("There was an error processing the file.", remoteFile.Error?.Message ?? "Unknown error");
            }
            
            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }
        sw.Stop();
    }
}