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
    /// <summary>
    /// Gets detailed information about the error encountered during Generative AI operations.
    /// </summary>
    /// <remarks>
    /// This property provides a description or context-specific details regarding the exception,
    /// which can be useful for debugging or understanding the nature of the problem.
    /// </remarks>
    public string Details { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeAIException"/> class.
    /// </summary>
    public GenerativeAIException() : this("A Generative AI error occurred", "")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeAIException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public GenerativeAIException(string message) : this(message, "")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerativeAIException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public GenerativeAIException(string message, Exception innerException) : this(message, "")
    {
    }

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