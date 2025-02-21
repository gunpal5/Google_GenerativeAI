using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace GenerativeAI.Live.Logging;

public static partial class MultiModalLiveClientLoggingExtensions
{
    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Attempting to connect to MultiModal Live API.")]
    public static partial void LogConnectionAttempt(this ILogger logger);

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Successfully connected to MultiModal Live API.")]
    public static partial void LogConnectionEstablished(this ILogger logger);

    [LoggerMessage(EventId = 102, Level = LogLevel.Error, Message = "WebSocket connection closed with error: {ErrorType}")]
    public static partial void LogConnectionClosedWithError(this ILogger logger, DisconnectionType errorType, Exception exception);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "WebSocket connection closed normally.")]
    public static partial void LogConnectionClosed(this ILogger logger);

    [LoggerMessage(EventId = 103, Level = LogLevel.Debug, Message = "Received message: {MessageType}")]
    public static partial void LogMessageReceived(this ILogger logger, WebSocketMessageType messageType);

    [LoggerMessage(EventId = 104, Level = LogLevel.Debug, Message = "Message sent: {MessageType}")]
    public static partial void LogMessageSent(this ILogger logger, string messageType);

    [LoggerMessage(EventId = 105, Level = LogLevel.Debug, Message = "Audio chunk received. Sample Rate: {SampleRate}, Has Header: {HasHeader}, Buffer Length: {BufferLength}")]
    public static partial void LogAudioChunkReceived(this ILogger logger, int sampleRate, bool hasHeader, int bufferLength);

    [LoggerMessage(EventId = 106, Level = LogLevel.Debug, Message = "Audio receive completed. Total Buffer Length: {BufferLength}")]
    public static partial void LogAudioReceiveCompleted(this ILogger logger, int bufferLength);

    [LoggerMessage(EventId = 107, Level = LogLevel.Warning, Message = "Generation interrupted.")]
    public static partial void LogGenerationInterrupted(this ILogger logger);

    [LoggerMessage(EventId = 108, Level = LogLevel.Error, Message = "{Message}")]
    public static partial void LogErrorOccurred(this ILogger logger, Exception exception, string message);

    [LoggerMessage(EventId = 109, Level = LogLevel.Debug, Message = "Setup message sent.")]
    public static partial void LogSetupSent(this ILogger logger);

    [LoggerMessage(EventId = 110, Level = LogLevel.Debug, Message = "Client content message sent.")]
    public static partial void LogClientContentSent(this ILogger logger);

    [LoggerMessage(EventId = 111, Level = LogLevel.Debug, Message = "Tool response message sent.")]
    public static partial void LogToolResponseSent(this ILogger logger);

    [LoggerMessage(EventId = 112, Level = LogLevel.Information, Message = "Calling function: {FunctionName}")]
    public static partial void LogFunctionCall(this ILogger logger, string functionName);
}