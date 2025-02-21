namespace GenerativeAI.Core;

/// <summary>
/// Represents authentication tokens used to access secure APIs or resources.
/// </summary>
/// <remarks>
/// This class contains the necessary tokens and metadata required for authentication.
/// It typically includes the access token, refresh token, and their expiration time.
/// It can be used in conjunction with authenticators to manage secure access.
/// </remarks>
public class AuthTokens
{
    /// <summary>
    /// Gets or sets the access token used for authenticating requests to secure APIs or resources.
    /// </summary>
    /// <remarks>
    /// The access token is a string issued by an authentication provider, often having a limited lifetime.
    /// It is essential for granting access to protected endpoints or resources and is typically included
    /// in the authorization header of HTTP requests.
    /// </remarks>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token used to obtain a new access token when the current one expires.
    /// </summary>
    /// <remarks>
    /// The refresh token is a long-lived token issued by the authentication provider. It allows
    /// the client to request a new access token without requiring the user to re-authenticate.
    /// This property is essential for maintaining seamless access to protected resources
    /// in scenarios where the access token's lifetime is limited.
    /// </remarks>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the token.
    /// </summary>
    /// <remarks>
    /// The ExpiryTime property indicates the exact date and time when the token becomes invalid and can no longer be used.
    /// It is important to monitor this value to refresh or renew the token before it expires, ensuring uninterrupted access
    /// to the required secure APIs or resources. Expiration time is typically provided by the authentication provider.
    /// </remarks>
    public DateTime? ExpiryTime { get; set; }

    /// Represents authentication tokens, including an access token, refresh token, and an optional expiry time.
    public AuthTokens(string accessToken, string? refreshToken = null, DateTime? expiryTime = null)
    {
        this.AccessToken = accessToken;
        this.RefreshToken = refreshToken;
        this.ExpiryTime = expiryTime;
    }

    /// Validates the authentication token by checking if the access token is present and,
    /// if applicable, whether the token has expired.
    /// <returns>True if the access token is valid, otherwise false.</returns>
    public bool Validate()
    {
        if(string.IsNullOrEmpty(AccessToken))
            return false;
        
        if(ExpiryTime.HasValue && ExpiryTime.Value < DateTime.UtcNow)
            return false;
        return true;
    }
}