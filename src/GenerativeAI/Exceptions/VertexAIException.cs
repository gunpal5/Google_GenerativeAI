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
    public GoogleRpcStatus Status { get; set; }
    public VertexAIException(string message, GoogleRpcStatus status):base(message)
    {
        Status = status;
    }
    
}