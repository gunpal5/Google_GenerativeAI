namespace GenerativeAI.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a file exceeds the maximum allowed size.
/// </summary>
public class FileTooLargeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileTooLargeException"/> class.
    /// </summary>
    public FileTooLargeException() : base("File is too large.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTooLargeException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FileTooLargeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTooLargeException"/> class with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the file that is too large.</param>
    public FileTooLargeException(string fileName) : base($"File {fileName} is too large.")
    {
        
    }
}