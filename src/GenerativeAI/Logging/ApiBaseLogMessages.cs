using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Logging;

/// <summary>
/// Extensions for logging <see cref="ApiBase"/> actions.
/// </summary>
/// <remarks>
/// This extension uses the <see cref="LoggerMessageAttribute"/> to
/// generate highly-performant logging code at compile time.
/// Use these methods for structured, consistent, and efficient logging.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static partial class ApiBaseLogMessages
{
    /// <summary>
    /// Logs a debug message when a GET request is initiated.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="url">The URL of the API endpoint being requested.</param>
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Sending GET request to URL: {Url}")]
    public static partial void LogGetRequest(this ILogger logger, string url);

    /// <summary>
    /// Logs a warning message when a GET request returns a non-success HTTP status code.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="statusCode">The HTTP status code returned by the server.</param>
    /// <param name="url">The URL of the API endpoint that returned the status code.</param>
    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "Received non-success status code: {StatusCode} for URL: {Url}")]
    public static partial void LogNonSuccessStatusCode(this ILogger logger, int statusCode, string url);

    /// <summary>
    /// Logs an informational message when a GET request completes successfully.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="url">The URL of the API endpoint that responded successfully.</param>
    /// <param name="content">The content of the response returned by the API.</param>
    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Successful response from {Url}: {Content}")]
    public static partial void LogSuccessfulGetResponse(this ILogger logger, string url, string content);

    /// <summary>
    /// Logs a warning message when a GET request is canceled.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "GET request was canceled.")]
    public static partial void LogGetRequestCanceled(this ILogger logger);

    /// <summary>
    /// Logs an informational message when an HTTP request (e.g., POST, PUT, or PATCH) is initiated.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="httpMethod">The HTTP method used for the request (e.g., POST, PUT, or PATCH).</param>
    /// <param name="url">The URL of the API endpoint being requested.</param>
    /// <param name="payload">The payload data sent as part of the request body.</param>
    [LoggerMessage(EventId = 5, Level = LogLevel.Debug,
        Message = "Sending {HttpMethod} request to URL: {Url} with payload: {Payload}")]
    public static partial void LogHttpRequest(this ILogger logger, string httpMethod, string url, object? payload);

    /// <summary>
    /// Logs an informational message when an HTTP request completes successfully.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="url">The URL of the API endpoint that responded successfully.</param>
    /// <param name="content">The content of the response returned by the API.</param>
    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Successful response from {Url}: {Content}")]
    public static partial void LogSuccessfulHttpResponse(this ILogger logger, string url, string? content);

    /// <summary>
    /// Logs a warning message when an HTTP request (e.g., POST, PUT, or PATCH) is canceled.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "HTTP request was canceled.")]
    public static partial void LogHttpRequestCanceled(this ILogger logger);

    /// <summary>
    /// Logs an error message when an exception occurs during execution.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging.</param>
    /// <param name="exceptionMessage">The message of the exception that was thrown.</param>
    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "An exception occurred: {ExceptionMessage}")]
    public static partial void LogException(this ILogger logger, string exceptionMessage);
}