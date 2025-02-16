namespace GenerativeAI.Core;

public interface IGoogleAuthenticator
{
    Task<AuthTokens> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<AuthTokens> RefreshAccessTokenAsync(AuthTokens token, CancellationToken cancellationToken = default);
    Task<AuthTokens?> ValidateAccessTokenAsync(string accessToken, bool throwError = true,
        CancellationToken cancellationToken = default);
}