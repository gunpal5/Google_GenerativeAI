namespace GenerativeAI.Core;

public class FileValidator
{
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