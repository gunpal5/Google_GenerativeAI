using System.Security.Authentication;
using System.Text.Json;
using GenerativeAI.Core;

namespace GenerativeAI.Authenticators;

public abstract class BaseAuthenticator : IGoogleAuthenticator
{
    public virtual Task<AuthTokens> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual Task<AuthTokens> RefreshAccessTokenAsync(AuthTokens token,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthTokens?> ValidateAccessTokenAsync(string accessToken, bool throwError = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));
        var token = await GetTokenInfo(accessToken);
        if (token != null)
            return token;
        else if (throwError)
            throw new AuthenticationException("Provided access token is invalid.");
        return null;
    }

    protected async Task<AuthTokens?> GetTokenInfo(string token)
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://oauth2.googleapis.com/tokeninfo?access_token=" + token);

        if (response.IsSuccessStatusCode)
        {
            var info = await response.Content.ReadAsStringAsync();


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