using GenerativeAI.Core;

namespace GenerativeAI;

/// <summary>
/// Represents the credentials required to authenticate with Google AI Generative APIs.
/// This class encapsulates the API key and an optional access token necessary for making authorized API calls.
/// </summary>
public class GoogleAICredentials : ICredentials
{
    /// <summary>
    /// Gets the API Key used to authenticate requests to Google AI Generative APIs.
    /// The API Key provides an easy way to access public resources or perform
    /// authorized operations without requiring OAuth2 tokens.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets the Access Token for authenticating with Google AI APIs.
    /// This token is used for OAuth2-based authentication and is typically
    /// required when an API Key alone is insufficient for certain protected resources.
    /// </summary>
    public AuthTokens? AuthToken { get; set; }

    /// <summary>
    /// Represents the credentials required to authenticate with Google AI Generative APIs.
    /// Manages the API Key and optional Access Token necessary for making API requests.
    /// </summary>
    public GoogleAICredentials(string apiKey,string? accessToken = null, DateTime? expiry = null)
    {
        this.ApiKey = apiKey;
        if(!string.IsNullOrEmpty(accessToken))
            this.AuthToken = new AuthTokens(accessToken, expiryTime:expiry);
    }

    /// <summary>
    /// Represents the credentials required to authenticate with Google AI Generative APIs.
    /// Responsible for managing both the API key and optional access tokens to enable secure communication with Google's services.
    /// </summary>
    public GoogleAICredentials()
    {
        
    }
    /// <summary>
    /// Validates the API credentials for the GoogleAICredentials instance.
    /// Ensures that either an API Key or an Access Token is provided.
    /// Throws an exception if both are null or empty.
    /// </summary>
    /// <exception cref="System.Exception">
    /// Thrown when neither the API Key nor the Access Token is provided.
    /// </exception>
    public void ValidateCredentials()
    {
        if(string.IsNullOrEmpty(ApiKey) && this.AuthToken !=null && this.AuthToken.Validate())
            throw new Exception("API Key or Access Token is required to call the API.");
    }
}