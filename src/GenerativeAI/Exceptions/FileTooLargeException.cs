namespace GenerativeAI.Exceptions;

public class FileTooLargeException:Exception
{
    public FileTooLargeException(string fileName) : base($"File {fileName} is too large.")
    {
        
    }
}