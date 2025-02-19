using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GenerativeAI.Core;
using Google.Apis.Auth.OAuth2;

namespace GenerativeAI.Authenticators;

public class GoogleServiceAccountAuthenticator : BaseAuthenticator
{
    private readonly List<string> _scopes =
    [
        "https://www.googleapis.com/auth/cloud-platform",
        "https://www.googleapis.com/auth/generative-language.retriever",
        "https://www.googleapis.com/auth/generative-language.tuning",
    ];

    private string _clientFile = "client_secret.json";
    private string _tokenFile = "token.json";
    private string _certificateFile = "key.p12";
    private string _certificatePassphrase;

    private ServiceAccountCredential _credential;

    /// <summary>
    /// Authenticator class for Google services using a service account.
    /// </summary>
    public GoogleServiceAccountAuthenticator(string serviceAccountEmail, string? certificate = null,
        string? passphrase = null)
    {
        var x509Certificate = new X509Certificate2(
            certificate ?? _certificateFile,
            passphrase ?? _certificatePassphrase,
            X509KeyStorageFlags.Exportable);
        _credential = new ServiceAccountCredential(
            new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = _scopes
            }.FromCertificate(x509Certificate));
    }
    
    /// <summary>
    /// Authenticator class for Google services using a service account.
    /// </summary>
    public GoogleServiceAccountAuthenticator(string? credentialFile)
    {
        using var stream = File.OpenRead(credentialFile ?? _clientFile);
        _credential = ServiceAccountCredential.FromServiceAccountData(stream);
        _credential.Scopes = _scopes;
    }

    public override async Task<AuthTokens> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await _credential.GetAccessTokenForRequestAsync(cancellationToken:cancellationToken).ConfigureAwait(false);

        if(string.IsNullOrEmpty(token))
            throw new AuthenticationException("Failed to get access token.");
        var tokenInfo = await GetTokenInfo(token).ConfigureAwait(false);
        if(tokenInfo == null)
            throw new AuthenticationException("Failed to get access token.");
        return tokenInfo;
    }

    public override async Task<AuthTokens> RefreshAccessTokenAsync(AuthTokens token, CancellationToken cancellationToken = default)
    {
        return await GetAccessTokenAsync(cancellationToken:cancellationToken).ConfigureAwait(false);
    }
}