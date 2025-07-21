using System.Security.Authentication;
using GenerativeAI.Core;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using ClientSecrets = Google.Apis.Auth.OAuth2.ClientSecrets;

namespace GenerativeAI.Authenticators;

/// <summary>
/// Authenticator that uses Google OAuth2 for authentication with Google services.
/// </summary>
public class GoogleOAuthAuthenticator:BaseAuthenticator
{
    private string _clientFile = "client_secret.json";
    
    private ICredential _credential;
    private string _tokenFile = "token.json";
    
    /// <summary>
    /// Initializes a new instance of the GoogleOAuthAuthenticator class with the specified credential file.
    /// </summary>
    /// <param name="credentialFile">Path to the client secret JSON file. If null, uses default "client_secret.json".</param>
    public GoogleOAuthAuthenticator(string? credentialFile)
    {
        var secrets = GetClientSecrets(credentialFile??_clientFile);   
        _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            ScopesConstants.Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(_tokenFile)).Result;
        
    }
    
    private ClientSecrets GetClientSecrets(string credentialFile)
    {
        ClientSecrets clientSecrets = null;
       
        if (File.Exists(credentialFile))
        {
            using var stream = File.OpenRead(credentialFile);
            clientSecrets = GoogleClientSecrets.FromStreamAsync(stream).Result.Secrets;
        }
        else throw new FileNotFoundException("Client secret file not found.");

        return clientSecrets;
    }

    /// <summary>
    /// Gets an access token asynchronously using the configured OAuth2 credentials.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that returns access token information.</returns>
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

    /// <summary>
    /// Refreshes an access token asynchronously using the provided token information.
    /// </summary>
    /// <param name="token">The token information to refresh.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that returns refreshed token information.</returns>
    public override Task<AuthTokens> RefreshAccessTokenAsync(AuthTokens token, CancellationToken cancellationToken = default)
    {
        return base.RefreshAccessTokenAsync(token, cancellationToken);
    }
}