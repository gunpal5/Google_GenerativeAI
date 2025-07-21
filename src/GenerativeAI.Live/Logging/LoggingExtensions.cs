using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace GenerativeAI.Live.Logging;

/// <summary>
/// Provides extension methods for logging events related to the MultiModal Live Client operations.
/// </summary>
/// <remarks>
/// These methods provide structured logging for events such as connection attempts, message processing,
/// and WebSocket related errors or closures.
/// </remarks>
public static partial class MultiModalLiveClientLoggingExtensions
{
    /// <summary>
    /// Logs an informational message indicating an attempt to connect to the MultiModal Live API.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used to perform the logging.</param>
    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Attempting to connect to MultiModal Live API.")]
    public static partial void LogConnectionAttempt(this ILogger logger);

    /// Logs a message indicating that a successful connection to the MultiModal Live API has been established.
    /// <param name="logger">The logger instance used to log the connection establishment message.</param>
    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Successfully connected to MultiModal Live API.")]
    public static partial void LogConnectionEstablished(this ILogger logger);

    /// Logs an error when the WebSocket connection is closed unexpectedly due to an error.
    /// <param name="logger">The logger instance used to log the message.</param>
    /// <param name="errorType">The type of disconnection that occurred.</param>
    /// <param name="exception">The exception associated with the disconnection, providing additional error details.</param>
    [LoggerMessage(EventId = 102, Level = LogLevel.Error, Message = "WebSocket connection closed with error: {ErrorType}")]
    public static partial void LogConnectionClosedWithError(this ILogger logger, DisconnectionType errorType, Exception exception);

    /// <summary>
    /// Logs an information message indicating that the WebSocket connection was closed normally.
    /// </summary>
    /// <param name="logger">The instance of <see cref="ILogger" /> used to log the message.</param>
    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "WebSocket connection closed normally.")]
    public static partial void LogConnectionClosed(this ILogger logger);

    /// Logs information about a message received through the WebSocket.
    /// <param name="logger">The logger instance used to log the message.</param>
    /// <param name="messageType">The type of the WebSocket message received.</param>
    [LoggerMessage(EventId = 103, Level = LogLevel.Debug, Message = "Received message: {MessageType}")]
    public static partial void LogMessageReceived(this ILogger logger, WebSocketMessageType messageType);

    /// Logs an event indicating that a message has been sent with the specified message type.
    /// <param name="logger">The ILogger instance used for logging.</param>
    /// <param name="messageType">The type or content of the message that was sent.</param>
    [LoggerMessage(EventId = 104, Level = LogLevel.Debug, Message = "Message sent: {MessageType}")]
    public static partial void LogMessageSent(this ILogger logger, string messageType);

    /// Logs debug level information when an audio chunk is received.
    /// This method records the details of the received audio chunk, including
    /// the sample rate, whether it contains a header, and the length of the buffer.
    /// <param name="logger">
    /// The <see cref="ILogger"/> instance used for logging the message.
    /// </param>
    /// <param name="sampleRate">
    /// The sample rate of the received audio chunk.
    /// </param>
    /// <param name="hasHeader">
    /// Indicates whether the audio chunk includes a header.
    /// </param>
    /// <param name="bufferLength">
    /// The length of the audio buffer in bytes.
    /// </param>
    [LoggerMessage(EventId = 105, Level = LogLevel.Debug, Message = "Audio chunk received. Sample Rate: {SampleRate}, Has Header: {HasHeader}, Buffer Length: {BufferLength}")]
    public static partial void LogAudioChunkReceived(this ILogger logger, int sampleRate, bool hasHeader, int bufferLength);

    /// Logs a debug message indicating that audio data has been successfully received and processed.
    /// <param name="logger">
    /// The logger instance used to write the log entry.
    /// </param>
    /// <param name="bufferLength">
    /// The total length of the audio data buffer that was received.
    /// </param>
    [LoggerMessage(EventId = 106, Level = LogLevel.Debug, Message = "Audio receive completed. Total Buffer Length: {BufferLength}")]
    public static partial void LogAudioReceiveCompleted(this ILogger logger, int bufferLength);

    /// <summary>
    /// Logs a warning that the generation process was interrupted.
    /// </summary>
    /// <param name="logger">The instance of the <see cref="ILogger"/> used to log the message.</param>
    [LoggerMessage(EventId = 107, Level = LogLevel.Warning, Message = "Generation interrupted.")]
    public static partial void LogGenerationInterrupted(this ILogger logger);

    /// Logs an error that has occurred during operation.
    /// <param name="logger">
    /// The `ILogger` instance used to log the error.
    /// </param>
    /// <param name="exception">
    /// The exception that provides details about the error that occurred.
    /// </param>
    /// <param name="message">
    /// A message describing the error context or details.
    /// </param>
    [LoggerMessage(EventId = 108, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void LogErrorOccurred(this ILogger logger, Exception exception, string message);

    /// Logs a debug message indicating a setup message has been sent.
    /// <param name="logger">The logger instance to record the log entry.</param>
    [LoggerMessage(EventId = 109, Level = LogLevel.Debug, Message = "Setup message sent.")]
    public static partial void LogSetupSent(this ILogger logger);

    /// Logs a debug-level message indicating that a client content message has been sent.
    /// <param name="logger">
    /// The <see cref="ILogger"/> instance used for logging.
    /// </param>
    [LoggerMessage(EventId = 110, Level = LogLevel.Debug, Message = "Client content message sent.")]
    public static partial void LogClientContentSent(this ILogger logger);

    /// Logs a message indicating that a tool response message has been sent.
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    [LoggerMessage(EventId = 111, Level = LogLevel.Debug, Message = "Tool response message sent.")]
    public static partial void LogToolResponseSent(this ILogger logger);

    /// Logs information about the invocation of a specified function.
    /// <param name="logger">The logger instance used to log the message.</param>
    /// <param name="functionName">The name of the function being called.</param>
    [LoggerMessage(EventId = 112, Level = LogLevel.Information, Message = "Calling function: {FunctionName}")]
    public static partial void LogFunctionCall(this ILogger logger, string functionName);

    /// <summary>
    /// Logs an error message indicating that the WebSocket connection was closed due to an invalid payload.
    /// </summary>
    /// <param name="logger">The logger to log the message to.</param>
    /// <param name="closeStatusDescription">The description of the close status that caused the connection to close.</param>
    [LoggerMessage(EventId = 113, Level = LogLevel.Error, Message = "WebSocket connection closed caused by invalid payload: {CloseStatusDescription}")]
    public static partial void LogConnectionClosedWithInvalidPyload(this ILogger logger, string closeStatusDescription);
}