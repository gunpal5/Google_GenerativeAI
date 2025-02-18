namespace GenerativeAI;

/// <summary>
/// Contains constants and configurations for inline file handling, 
/// including file size limitations and allowed MIME types.
/// </summary>
public static class InlineMimeTypes
{
    /// <summary>
    /// Maximum size (in bytes) for inline files (20 MB).
    /// </summary>
    public const long MaxInlineSize = 1024 * 1024 * 20; // 20 MB

    /// <summary>
    /// A set of allowed MIME types for inline file handling.
    /// </summary>
    public static readonly HashSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/heif", "image/heic", "image/webp",
        "audio/wav", "audio/mp3", "audio/mpeg", "audio/aiff", "audio/aac", "audio/ogg", "audio/flac"
    };
}