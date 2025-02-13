namespace GenerativeAI;

public class InlineMimeTypes
{
    public const long MaxInlineSize = 1024 * 1024 * 20; // 20 MB
    public static readonly HashSet<string> AllowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/heif", "image/heic", "image/webp",
        "audio/wav", "audio/mp3", "audio/mpeg", "audio/aiff", "audio/aac", "audio/ogg", "audio/flac"
    };
}