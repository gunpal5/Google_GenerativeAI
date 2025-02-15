namespace GenerativeAI.Core;

/// <summary>
/// Provides credentials required for API authentication, supporting both user and service account scenarios.
/// </summary>
public sealed class CredentialConfiguration : ClientSecrets
{
    private string _projectId;

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