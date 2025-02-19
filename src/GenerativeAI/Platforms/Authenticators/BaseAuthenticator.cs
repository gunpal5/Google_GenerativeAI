using System.Security.Authentication;
using System.Text.Json;
using GenerativeAI.Core;

namespace GenerativeAI.Authenticators;

/// <summary>
/// An abstract base class for implementing authenticators for Google APIs.
/// </summary>
public abstract class BaseAuthenticator : IGoogleAuthenticator
{
    /// <summary>
    /// Retrieves an access token for the Google API.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An instance of <see cref="AuthTokens"/> representing the access token.</returns>
    public virtual Task<AuthTokens?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Refreshes an expired access token to obtain a new one.
    /// </summary>
    /// <param name="token">The expired <see cref="AuthTokens"/> instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An instance of <see cref="AuthTokens"/> representing the refreshed access token.</returns>
    public virtual Task<AuthTokens?> RefreshAccessTokenAsync(AuthTokens token,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates the provided access token and determines its validity.
    /// </summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <param name="throwError">If true, throws an exception for an invalid token. Defaults to true.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A valid <see cref="AuthTokens"/> instance if the token is valid, or null if invalid and <paramref name="throwError"/> is false.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="accessToken"/> is null or empty.</exception>
    /// <exception cref="AuthenticationException">Thrown if the token is invalid and <paramref name="throwError"/> is true.</exception>
    public async Task<AuthTokens?> ValidateAccessTokenAsync(string accessToken, bool throwError = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));
        var token = await GetTokenInfo(accessToken).ConfigureAwait(false);
        if (token != null)
            return token;
        else if (throwError)
            throw new AuthenticationException("Provided access token is invalid.");
        return null;
    }

    /// <summary>
    /// Retrieves token information for the given access token by querying Google's token info endpoint.
    /// </summary>
    /// <param name="token">The access token to retrieve information for.</param>
    /// <returns>
    /// A valid <see cref="AuthTokens"/> instance with the token data if successful, or null if unsuccessful.
    /// </returns>
    protected async Task<AuthTokens?> GetTokenInfo(string token)
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://oauth2.googleapis.com/tokeninfo?access_token=" + token).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var info = await response.Content.ReadAsStringAsync().ConfigureAwait(false);


            var doc = JsonDocument.Parse(info);
            doc.RootElement.TryGetProperty("expires_in", out var expiresIn);
            var expiresInSeconds = 0;
            if (expiresIn.ValueKind == JsonValueKind.Number)
                expiresInSeconds = (int)expiresIn.GetInt32();
            else if (expiresIn.ValueKind == JsonValueKind.String)
                expiresInSeconds = int.Parse(expiresIn.GetString());
            else
                return null;
            return new AuthTokens(token, expiryTime: DateTime.UtcNow.AddSeconds(expiresInSeconds));

        }

        return null;
    }
}