using GenerativeAI.Core;

namespace GenerativeAI.Web;

/// <summary>
/// Defines the configuration options for GenerativeAI services.
/// </summary>
public interface IGenerativeAIOptions
{
    /// <summary>
    /// Gets or sets the Google Cloud project ID.
    /// </summary>
    public string? ProjectId { get; set; }
    
    /// <summary>
    /// Gets or sets the Google Cloud region.
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// Gets or sets the authenticator for Google services.
    /// </summary>
    public IGoogleAuthenticator? Authenticator { get; set; }
    
    /// <summary>
    /// Gets or sets the Google AI credentials.
    /// </summary>
    public GoogleAICredentials? Credentials { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use Vertex AI.
    /// </summary>
    public bool? IsVertex { get; set; }
    
    /// <summary>
    /// Gets or sets the AI model name.
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use express mode.
    /// </summary>
    public bool? ExpressMode { get; set; }
    
    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    public string? ApiVersion { get; set; }
}

/// <summary>
/// Configuration options for GenerativeAI services.
/// </summary>
public class GenerativeAIOptions:IGenerativeAIOptions
{
    /// <summary>
    /// Gets or sets the Google Cloud project ID.
    /// </summary>
    public string? ProjectId { get; set; }
    
    /// <summary>
    /// Gets or sets the Google Cloud region.
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// Gets or sets the authenticator for Google services.
    /// </summary>
    public IGoogleAuthenticator? Authenticator { get; set; }
    
    /// <summary>
    /// Gets or sets the Google AI credentials.
    /// </summary>
    public GoogleAICredentials? Credentials { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use Vertex AI.
    /// </summary>
    public bool? IsVertex { get; set; }
    
    /// <summary>
    /// Gets or sets the AI model name.
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use express mode.
    /// </summary>
    public bool? ExpressMode { get; set; }
    
    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    public string? ApiVersion { get; set; }
}