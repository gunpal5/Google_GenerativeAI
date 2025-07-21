namespace GenerativeAI.Types;

/// <summary>
/// Represents the response containing details about a file uploaded through the File API.
/// </summary>
/// <seealso href="https://ai.google.dev/api/files">Official API Documentation</seealso>
public class UploadFileResponse
{
    /// <summary>
    /// Metadata for the created file.
    /// </summary>
    public RemoteFile File { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadFileResponse"/> class with the specified file metadata.
    /// </summary>
    /// <param name="file">Metadata for the created file.</param>
    public UploadFileResponse(RemoteFile file)
    {
        File = file;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadFileResponse"/> class for JSON deserialization.
    /// </summary>
    public UploadFileResponse() : this(new RemoteFile()) { }
}