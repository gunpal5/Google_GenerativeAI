namespace GenerativeAI.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a file exceeds the maximum allowed size.
/// </summary>
public class FileTooLargeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTooLargeException"/> class with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the file that is too large.</param>
    public FileTooLargeException(string fileName) : base($"File {fileName} is too large.")
    {
        
    }
}