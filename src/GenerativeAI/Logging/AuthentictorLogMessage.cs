using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Logging;

/// <summary>
/// Extensions for logging OAuth authentication process actions.
/// </summary>
/// <remarks>
/// This extension uses the <see cref="LoggerMessageAttribute"/> to
/// generate highly-performant logging code at compile time.
/// Use these methods for structured, consistent, and efficient logging.
/// </remarks>
[ExcludeFromCodeCoverage]
public static partial class AuthentictorLogMessage
{
    /// <summary>
    /// Logs an informational message when an authentication process starts.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Authentication process started.")]
    public static partial void LogAuthenticationStarted(this ILogger logger);

    /// <summary>
    /// Logs an informational message when an authentication process ends successfully.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Authentication process ended successfully.")]
    public static partial void LogAuthenticationEndedSuccessfully(this ILogger logger);

    /// <summary>
    /// Logs a warning message when an authentication process fails to complete.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="reason">The reason the process failed.</param>
    [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "Authentication process failed. Reason: {Reason}")]
    public static partial void LogAuthenticationFailed(this ILogger logger, string reason);
}