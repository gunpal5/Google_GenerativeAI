using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a long-running operation fails or encounters an error.
/// </summary>
/// <remarks>
/// This exception provides access to detailed status information about the error, encapsulated in the <see cref="GoogleRpcStatus"/> object.
/// It is used to relay error information related to an operation executed within the generative AI engine or related systems.
/// </remarks>
public class VertexAIException:Exception
{
    /// <summary>
    /// Gets or sets the detailed RPC status information about the error.
    /// </summary>
    public GoogleRpcStatus Status { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAIException"/> class.
    /// </summary>
    public VertexAIException() : this("A Vertex AI error occurred", new GoogleRpcStatus())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAIException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public VertexAIException(string message) : this(message, new GoogleRpcStatus())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAIException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public VertexAIException(string message, Exception innerException) : this(message, new GoogleRpcStatus())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAIException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="status">The detailed RPC status information.</param>
    public VertexAIException(string message, GoogleRpcStatus status):base(message)
    {
        Status = status;
    }
}