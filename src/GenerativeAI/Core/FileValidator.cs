namespace GenerativeAI.Core;

/// <summary>
/// Provides methods to validate files for inline use based on size and MIME type constraints.
/// </summary>
public class FileValidator
{
    /// <summary>
    /// Validates the specified file for inline use.
    /// </summary>
    /// <param name="filePath">The full path to the file to validate.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist at the given path.</exception>
    /// <exception cref="ArgumentException">Thrown when the file exceeds the maximum size allowed for inline use 
    /// or its MIME type is not allowed.</exception>
    public static void ValidateInlineFile(string filePath)
    {
        var info = new FileInfo(filePath);
            
        if(!info.Exists)
            throw new FileNotFoundException("File not found", filePath);
        if(info.Length > InlineMimeTypes.MaxInlineSize)
            throw new ArgumentException($"File size {info.Length} is too large for inline. Use File Upload instead", nameof(filePath));
            
        var mimeType = MimeTypeMap.GetMimeType(filePath);
        if(!InlineMimeTypes.AllowedMimeTypes.Contains(mimeType))
            throw new ArgumentException($"File type {mimeType} is not allowed for inline", nameof(filePath));
    }
}