namespace GenerativeAI.Exceptions;

/// <summary>
/// Represents an exception specific to errors encountered within Generative AI operations.
/// </summary>
/// <remarks>
/// This exception is typically thrown when an error occurs during the usage of Generative AI-based systems or processes.
/// </remarks>
/// <example>
/// Example usage scenarios include handling service-specific errors or internal processing failures related to Generative AI.
/// </example>
public class GenerativeAIException : Exception
{
    public string Details { get; private set; }

    /// <summary>
    /// Represents an exception that occurs during operations related to Generative AI.
    /// </summary>
    /// <remarks>
    /// This exception is intended to capture errors specific to Generative AI operations, providing additional details
    /// about the context of the error for debugging or logging purposes.
    /// </remarks>
    public GenerativeAIException(string message, string details) : base(message)
    {
        Details = details;
    }
}