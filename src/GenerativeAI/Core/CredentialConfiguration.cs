namespace GenerativeAI.Core;

/// <summary>
/// Provides credentials required for API authentication, supporting both user and service account scenarios.
/// </summary>
public sealed class CredentialConfiguration : ClientSecrets
{
    private string _projectId = string.Empty;

    /// <summary>
    /// Represents a configuration for client credentials used in API authentication.
    /// </summary>
    /// <remarks>
    /// Encapsulates detailed information including web and installed application credentials, account details, and additional parameters like refresh tokens and domains.
    /// This class supports scenarios requiring user or service account authentication for API access.
    /// </remarks>
  
    public CredentialConfiguration(ClientSecrets web, ClientSecrets installed, string account, string refreshToken, string type, string universeDomain)
    {
        Web = web;
        Installed = installed;
        Account = account;
        RefreshToken = refreshToken;
        Type = type;
        UniverseDomain = universeDomain;
    }

    /// <summary>
    /// Represents the configuration for API authentication credentials.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate various properties required for authentication such as
    /// client secrets for web and installed applications, account details, and token information.
    /// It supports both user-based and service account authentication scenarios in API integrations.
    /// </remarks>
    public CredentialConfiguration():this(new ClientSecrets(),new ClientSecrets(), "","","","")
    {
        
    }

    /// <summary>
    /// Client secrets configured for web-based OAuth 2.0 flows.
    /// </summary>
    public ClientSecrets Web { get; set; }

    /// <summary>
    /// Client secrets configured for desktop or installed application flows.
    /// </summary>
    public ClientSecrets Installed { get; set; }

    /// <summary>
    /// Identifies the user or service account used by the application.
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// Stores a token that can be used to refresh access without requesting full credentials again.
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// Specifies the account type, such as a service account or user account.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Indicates the domain or environment scope for the application.
    /// </summary>
    public string UniverseDomain { get; set; }

    /// <summary>
    /// Represents the identifier of a project in the provider's platform.
    /// </summary>
    public string ProjectId
    {
        get => _projectId;
        set => _projectId = value;
    }

    /// <summary>
    /// Sets or retrieves the same identifier used for project-level operations and quotas.
    /// </summary>
    public string QuotaProjectId
    {
        get => _projectId;
        set => _projectId = value;
    }
}

/// <summary>
/// Defines the OAuth 2.0 client details, often sourced from a configuration file or similar resource.
/// </summary>
public class ClientSecrets
{
    /// <summary>
    /// Represents the essential OAuth 2.0 client credentials required for authentication.
    /// </summary>
    /// <remarks>
    /// Includes client identifier, client secret, redirect URIs, and token-related endpoints.
    /// This class encapsulates the details necessary to authenticate requests against an API or service.
    /// </remarks>
    public ClientSecrets(string clientId, string clientSecret, string[] redirectUris, string authUri, string authProviderX509CertUrl, string tokenUri)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        RedirectUris = redirectUris;
        AuthUri = authUri;
        AuthProviderX509CertUrl = authProviderX509CertUrl;
        TokenUri = tokenUri;
    }

    /// <summary>
    /// Encapsulates OAuth 2.0 client secret configurations required for authenticating against APIs or services.
    /// </summary>
    /// <remarks>
    /// This class holds critical authentication details including client ID, client secret, redirect URIs,
    /// authorization URI, certificate URL of the authentication provider, and token endpoint.
    /// These details are often used for secure communication with external services.
    /// </remarks>
    public ClientSecrets():this("","", [],"","","")
    {
        
    }

    /// <summary>
    /// A unique identifier for the client within an OAuth 2.0 flow.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// A secret key used during OAuth 2.0 credential exchange.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// An array of URIs which the identity provider can redirect to after user authentication.
    /// </summary>
    public string[] RedirectUris { get; set; }

    /// <summary>
    /// The authorization endpoint used during the OAuth 2.0 process.
    /// </summary>
    public string AuthUri { get; set; }

    /// <summary>
    /// The location of X.509 certificates for verifying token authenticity.
    /// </summary>
    public string AuthProviderX509CertUrl { get; set; }

    /// <summary>
    /// The token endpoint used for exchanging credentials in an OAuth 2.0 flow.
    /// </summary>
    public string TokenUri { get; set; }
}