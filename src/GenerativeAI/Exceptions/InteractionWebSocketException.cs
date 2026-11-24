namespace GenerativeAI.Exceptions;

/// <summary>
/// [Experimental] Raised when a live interaction WebSocket connection or frame fails (python-genai
/// <c>APIWebSocketError</c>). Messages never include the peer's close reason or the underlying transport error, which
/// may echo credentials; use <see cref="CloseCode"/> and <see cref="HttpStatus"/> instead.
/// </summary>
public class InteractionWebSocketException : GenerativeAIException
{
    /// <summary>Initializes a new instance of the <see cref="InteractionWebSocketException"/> class.</summary>
    public InteractionWebSocketException() : this("WebSocket connection error.")
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InteractionWebSocketException"/> class.</summary>
    /// <param name="message">The error message.</param>
    public InteractionWebSocketException(string message) : base(message, string.Empty)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InteractionWebSocketException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public InteractionWebSocketException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InteractionWebSocketException"/> class.</summary>
    /// <param name="message">The error message.</param>
    /// <param name="closeCode">The WebSocket close code, when the connection was closed.</param>
    /// <param name="httpStatus">The HTTP status of a failed WebSocket upgrade, when known.</param>
    /// <param name="serverCode">A server error code, when known.</param>
    public InteractionWebSocketException(string message, int? closeCode, int? httpStatus = null,
        string? serverCode = null) : base(message, string.Empty)
    {
        CloseCode = closeCode;
        HttpStatus = httpStatus;
        ServerCode = serverCode;
    }

    /// <summary>
    /// The WebSocket close code (e.g. 1000, 1011), or <c>null</c> for protocol errors (invalid frames) and setup
    /// failures that did not close an established connection. 1006 means the connection was lost without a close frame.
    /// </summary>
    public int? CloseCode { get; }

    /// <summary>The HTTP status of a rejected WebSocket upgrade (e.g. 401), when the platform reports it.</summary>
    public int? HttpStatus { get; }

    /// <summary>A server error code, when known.</summary>
    public string? ServerCode { get; }
}
