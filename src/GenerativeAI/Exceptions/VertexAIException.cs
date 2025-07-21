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
    /// <param name="message">The exception message.</param>
    /// <param name="status">The detailed RPC status information.</param>
    public VertexAIException(string message, GoogleRpcStatus status):base(message)
    {
        Status = status;
    }
    
}