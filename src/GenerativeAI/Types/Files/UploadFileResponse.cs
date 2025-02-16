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
}