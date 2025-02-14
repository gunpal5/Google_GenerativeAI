using System.Diagnostics;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GenerativeModel
{
    public FileClient Files { get; set; }

    /// <summary>
    /// Gets or sets the timeout value, in seconds, used for checking the state of a uploaded file.
    /// </summary>
    /// <remarks>
    /// This property determines the maximum duration to wait while verifying the state of operations related to files, such as uploads or processing.
    /// </remarks>
    public int TimeoutForFileStateCheck { get; set; } = 5 * 60;//Seconds
    public async Task<RemoteFile> UploadFileAsync(string filePath, Action<double>? progressCallback,
        CancellationToken cancellationToken = default)
    {
        return await Files.UploadFileAsync(filePath, progressCallback, cancellationToken);
    }
    
    public async Task<RemoteFile> GetFileAsync(string fileId, 
        CancellationToken cancellationToken = default)
    {
        return await Files.GetFileAsync(fileId, cancellationToken);
    }

    public async Task AwaitForFileStateActive(RemoteFile file, int maxSeconds = 5 * 60, CancellationToken cancellationToken = default)
    {
       await Files.AwaitForFileStateActiveAsync(file, maxSeconds, cancellationToken);
    }
    private async Task AppendFile(string filePath, GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var info = new FileInfo(filePath);
        var mimeType = MimeTypeMap.GetMimeType(filePath);
        if (info.Length < InlineMimeTypes.MaxInlineSize && InlineMimeTypes.AllowedMimeTypes.Contains(mimeType))
        {
            request.AddInlineFile(filePath);
        }
        else if (info.Length < FilesConstants.MaxUploadFileSize && FilesConstants.SupportedMimeTypes.Contains(mimeType))
        {
            var file = await UploadFileAsync(filePath, null, cancellationToken);
            await AwaitForFileStateActive(file, TimeoutForFileStateCheck, cancellationToken);
            request.AddRemoteFile(file);
        }
        else
        {
            throw new NotSupportedException("File type not supported.");
        }
    }
}