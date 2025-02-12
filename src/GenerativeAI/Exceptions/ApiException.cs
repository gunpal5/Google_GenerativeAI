using System.Text.Json.Serialization;

namespace GenerativeAI.Exceptions;

/// <summary>
/// Represents an exception that occurs during API interactions.
/// </summary>
/// <remarks>
/// This exception is thrown when an error is encountered while interacting with an API. It encapsulates
/// details such as the error code, error message, and error status to provide more context about
/// the failure.
/// </remarks>
public class ApiException : Exception
{
    /// <summary>
    /// Gets the error code associated with the exception.
    /// </summary>
    /// <remarks>
    /// The error code provides a numerical representation of the specific error that occurred,
    /// aiding in identifying and categorizing the issue.
    /// </remarks>
    [JsonPropertyName("code")]
    public int ErrorCode { get; }

    /// <summary>
    /// Gets the error message that describes the exception.
    /// This message provides detailed information about the error encountered during the API operation.
    /// </summary>
    [JsonPropertyName("message")]
    public string ErrorMessage { get; }

    /// <summary>
    /// Gets the status of the error represented as a string.
    /// </summary>
    /// <remarks>
    /// This property contains a textual representation of the status of the error.
    /// It is typically used to provide additional context or categorization of the error,
    /// such as "BadRequest", "Unauthorized", or similar status indicators.
    /// </remarks>
    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string ErrorStatus { get; }

    /// <summary>
    /// Represents an exception that occurs when a platform-specific API operation fails.
    /// </summary>
    /// <remarks>
    /// This exception includes additional details such as the error code,
    /// error message, and error status associated with the API failure.
    /// </remarks>
    public ApiException(int errorCode, string errorMessage, string errorStatus)
        : base($"{errorStatus} (Code: {errorCode}): {errorMessage}")
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ErrorStatus = errorStatus;
    }
    
}