namespace GenerativeAI.Core;

/// <summary>
/// Defines the contract for an authenticator to manage authentication processes for Google services.
/// </summary>
public interface IGoogleAuthenticator
{
    /// <summary>
    /// Asynchronously retrieves an access token needed for authentication.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the authentication tokens.</returns>
    Task<AuthTokens?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously refreshes the access token using the provided token information.
    /// </summary>
    /// <param name="token">The existing authentication token to be refreshed.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the refreshed authentication tokens.</returns>
    Task<AuthTokens?> RefreshAccessTokenAsync(AuthTokens token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously validates the provided access token.
    /// </summary>
    /// <param name="accessToken">The access token to be validated.</param>
    /// <param name="throwError">Indicates whether an error should be thrown if validation fails.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// Returns the authentication tokens if the access token is valid; otherwise, null if throwError is false.
    /// </returns>
    Task<AuthTokens?> ValidateAccessTokenAsync(string accessToken, bool throwError = true, CancellationToken cancellationToken = default);
}