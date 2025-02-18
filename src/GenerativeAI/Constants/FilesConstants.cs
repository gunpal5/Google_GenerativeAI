namespace GenerativeAI;

/// <summary>
/// Defines a collection of constants representing supported MIME types for various file operations within the API.
/// </summary>
public static class FilesConstants
{
    /// <summary>
    /// Represents the maximum allowed file size, in bytes, for file uploads within the system.
    /// Any file exceeding this size will result in an error or exception.
    /// </summary>
    /// <remarks>
    /// This constant is utilized across various methods to validate the file size before processing or uploading.
    /// It ensures compliance with API or system-imposed size limitations for handling file uploads.
    /// </remarks>
    /// <seealso cref="GenerativeAI.Clients.FileClient.ValidateFile"/>
    /// <seealso cref="GenerativeAI.Clients.FileClient.ValidateStream"/>
    /// <seealso cref="GenerativeAI.GeminiModel.AppendFile"/>
    public const long MaxUploadFileSize = 2147483648;
    /// <summary>
    /// Gets the collection of all supported MIME types.
    /// </summary>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/vision">Official Documentation for Vision</seealso>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/audio">Official Documentation for Audio</seealso>
    /// <seealso jref="https://ai.google.dev/gemini-api/docs/document-processing">Official Documentation for Document Processing</seealso>
    public static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Image formats
        "image/jpeg", "image/png", "image/heif", "image/heic", "image/webp",

        // Audio formats
        "audio/wav", "audio/mp3", "audio/mpeg", "audio/aiff", "audio/aac", "audio/ogg", "audio/flac",

        // Video formats
        "video/mp4", "video/mpeg", "video/mov", "video/avi", "video/x-flv", "video/mpg",
        "video/webm", "video/wmv", "video/3gpp",

        // Document and text formats
        "application/pdf", "application/x-javascript", "text/javascript", "application/x-python",
        "text/x-python", "text/plain", "text/html", "text/css", "text/md", "text/csv", "text/xml", "text/rtf"
    };
}