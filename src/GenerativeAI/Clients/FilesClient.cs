using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GenerativeAI.Constants;
using GenerativeAI.Core;
using GenerativeAI.Extensions;
using GenerativeAI.Platforms.Exceptions;
using GenerativeAI.Types.Files;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Gemini API Files endpoint.
/// </summary>
/// <seealso href="https://ai.google.dev/api/files">See Official API Documentation</seealso>
public class FileClient : BaseClient
{
    public FileClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform,
        httpClient, logger)
    {
    }

    /// <summary>
    /// Creates a <see cref="RemoteFile"/>.
    /// </summary>
    /// <param name="file">Metadata for the file to create.</param>
    /// <returns>The <see cref="RemoteFile"/> information.</returns>
    /// <seealso href="https://ai.google.dev/api/files#method:-media.upload">See Official API Documentation</seealso>
    public async Task<RemoteFile?> UploadFileAsync(string filePath, Action<double>? progressCallback = null)
    {
        var baseUrl = _platform.GetBaseUrl(false);
        var apiVersion = _platform.GetApiVersion();
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

        var json = JsonSerializer.Serialize(request, SerializerOptions);
        //Upload File
        using var file = File.OpenRead(filePath);
        var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);
        var multipart = new MultipartContent("related");
        multipart.Add(new StringContent(json, Encoding.UTF8, "application/json"));
        multipart.Add(new ProgressStreamContent(file, progressCallback)
        {
            Headers =
            {
                ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(filePath)),
                ContentLength = file.Length
            }
        });
        httpMessage.Content = multipart;
        _platform.AddAuthorization(httpMessage);
        var response = await HttpClient.SendAsync(httpMessage);
        await CheckAndHandleErrors(response, url);

        var fileResponse = await Deserialize<UploadFileResponse>(response);
        return fileResponse.File;
    }

    /// <summary>
    /// Uploads a file stream as a <see cref="RemoteFile"/> to the remote server.
    /// </summary>
    /// <param name="stream">The stream representing the file to upload.</param>
    /// <param name="displayName">The display name of the file being uploaded.</param>
    /// <param name="mimeType">The MIME type of the file being uploaded.</param>
    /// <param name="progressCallback">An optional callback to track the progress of the upload, represented as a percentage.</param>
    /// <returns>The uploaded <see cref="RemoteFile"/> information.</returns>
    /// <seealso href="https://ai.google.dev/api/files#method:-media.upload">See Official API Documentation</seealso>
    public async Task<RemoteFile?> UploadStreamAsync(Stream stream, string displayName, string mimeType,
        Action<double>? progressCallback = null)
    {
        var baseUrl = _platform.GetBaseUrl(false);
        var apiVersion = _platform.GetApiVersion();
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

        var json = JsonSerializer.Serialize(request, SerializerOptions);
        //Upload File

        var httpMessage = new HttpRequestMessage(HttpMethod.Post, url);
        var multipart = new MultipartContent("related");
        multipart.Add(new StringContent(json, Encoding.UTF8, "application/json"));
        multipart.Add(new ProgressStreamContent(stream, progressCallback)
        {
            Headers =
            {
                ContentType = new MediaTypeHeaderValue(mimeType),
                ContentLength = stream.Length
            }
        });
        httpMessage.Content = multipart;
        _platform.AddAuthorization(httpMessage);
        var response = await HttpClient.SendAsync(httpMessage);
        await CheckAndHandleErrors(response, url);

        var fileResponse = await Deserialize<UploadFileResponse>(response);
        return fileResponse.File;
    }

    private void ValidateStream(Stream stream, string mimeType)
    {
        if (stream.Length > FilesConstants.MaxUploadFileSize)
            throw new FileTooLargeException("stream");
        ValidateMimeType(mimeType);
    }

    private void ValidateFile(string filePath)
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
    private void ValidateMimeType(string mimeType)
    {
        if (mimeType == null)
        {
            throw new ArgumentNullException(nameof(mimeType), "MIME type cannot be null.");
        }

        var supportedMimeTypes = FilesConstants.RemoteFiles;

        if (!supportedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"The provided MIME type '{mimeType}' is not supported by the system.");
        }
    }


    /// <summary>
    /// Gets the metadata for the given <see cref="RemoteFile"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="RemoteFile"/> to get.</param>
    /// <returns>The <see cref="RemoteFile"/> information.</returns>
    /// <seealso href="https://ai.google.dev/api/files#method:-files.get">See Official API Documentation</seealso>
    public async Task<RemoteFile?> GetFileAsync(string name)
    {
        var baseUrl = _platform.GetBaseUrl();

        var url = $"{baseUrl}/{name.ToFileId()}";
        return await GetAsync<RemoteFile>(url);
    }

    /// <summary>
    /// Lists the metadata for <see cref="RemoteFile"/>s owned by the requesting project.
    /// </summary>
    /// <param name="pageSize">Maximum number of <see cref="RemoteFile"/>s to return per page. If unspecified, defaults to 10. Maximum <paramref name="pageSize"/> is 100.</param>
    /// <param name="pageToken">A page token from a previous <see cref="ListMyCustomFilesAsync"/> call.</param>
    /// <returns>A list of <see cref="RemoteFile"/>s.</returns>
    /// <seealso href="https://ai.google.dev/api/files#method:-files.list">See Official API Documentation</seealso>
    public async Task<ListFilesResponse?> ListFilesAsync(int? pageSize = null, string? pageToken = null)
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
        var url = $"{_platform.GetBaseUrl()}/files{queryString}";

        return await GetAsync<ListFilesResponse>(url);
    }

    /// <summary>
    /// Deletes the <see cref="RemoteFile"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="RemoteFile"/> to delete.</param>
    /// <seealso href="https://ai.google.dev/api/files#method:-files.delete">See Official API Documentation</seealso>
    public async Task DeleteFileAsync(string name)
    {
        var baseUrl = _platform.GetBaseUrl();

        var url = $"{baseUrl}/{name.ToFileId()}";
        await DeleteAsync(url);
    }
}