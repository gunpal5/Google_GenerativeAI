namespace GenerativeAI.Core;

/// <summary>
/// Defines a contract for credential validation.
/// Implementing classes must provide a mechanism to validate credentials required for API usage or authentication.
/// </summary>
public interface ICredentials
{
    /// <summary>
    /// Validates the credentials for authentication or API usage.
    /// </summary>
    /// <exception cref="System.Exception">
    /// Thrown when the credentials are invalid or incomplete.
    /// </exception>
    void ValidateCredentials();
}