using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GeminiModel
{
    /// <summary>
    /// Gets or sets the timeout value, in seconds, used for checking the state of a uploaded file.
    /// </summary>
    /// <remarks>
    /// This property determines the maximum duration to wait while verifying the state of operations related to files, such as uploads or processing.
    /// </remarks>
    public int TimeoutForFileStateCheck { get; set; } = 5 * 60;//Seconds

    /// <summary>
    /// Uploads a file asynchronously to the API and tracks its upload progress.
    /// </summary>
    /// <param name="filePath">The local path of the file to be uploaded.</param>
    /// <param name="progressCallback">An optional callback to receive updates on upload progress as a percentage.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="RemoteFile"/> object representing the uploaded file on the API.</returns>
    public async Task<RemoteFile> UploadFileAsync(string filePath, Action<double>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        return await Files.UploadFileAsync(filePath, progressCallback, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves metadata of a file asynchronously from the API.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file whose metadata is to be retrieved.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="RemoteFile"/> object containing metadata details of the requested file.</returns>
    public async Task<RemoteFile> GetFileAsync(string fileId, 
        CancellationToken cancellationToken = default)
    {
        return await Files.GetFileAsync(fileId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for a remote file to reach the "Active" state within a specified time limit.
    /// </summary>
    /// <param name="file">The remote file whose state is being monitored.</param>
    /// <param name="maxSeconds">The maximum number of seconds to wait for the file to become "Active".</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AwaitForFileStateActive(RemoteFile file, int maxSeconds = 5 * 60, CancellationToken cancellationToken = default)
    {
        await Files.AwaitForFileStateActiveAsync(file, maxSeconds, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Appends a file to the provided request based on its size and MIME type.
    /// </summary>
    /// <param name="filePath">The path of the file to be appended.</param>
    /// <param name="request">The content generation request to which the file will be appended.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. Throws exceptions if the file does not meet the requirements.</returns>
    private async Task AppendFile(string filePath, GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var info = new FileInfo(filePath);
        if (!info.Exists)
            throw new FileNotFoundException("File not found.", filePath);
        var mimeType = MimeTypeMap.GetMimeType(filePath);
        if (info.Length < InlineMimeTypes.MaxInlineSize && InlineMimeTypes.AllowedMimeTypes.Contains(mimeType))
        {
            request.AddInlineFile(filePath);
        }
        else if (info.Length < FilesConstants.MaxUploadFileSize && FilesConstants.SupportedMimeTypes.Contains(mimeType))
        {
            var file = await UploadFileAsync(filePath, null, cancellationToken).ConfigureAwait(false);
            await AwaitForFileStateActive(file, TimeoutForFileStateCheck, cancellationToken).ConfigureAwait(false);
            request.AddRemoteFile(file);
        }
        else
        {
            throw new NotSupportedException("File type not supported.");
        }
    }
}