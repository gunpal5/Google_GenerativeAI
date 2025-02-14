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
    public string ApiKey { get; }

    /// <summary>
    /// Gets the Access Token for authenticating with Google AI APIs.
    /// This token is used for OAuth2-based authentication and is typically
    /// required when an API Key alone is insufficient for certain protected resources.
    /// </summary>
    public string? AccessToken { get; }

    /// <summary>
    /// Represents the credentials required to authenticate with Google AI Generative APIs.
    /// Manages the API Key and optional Access Token necessary for making API requests.
    /// </summary>
    public GoogleAICredentials(string apiKey,string? accessToken = null)
    {
        this.ApiKey = apiKey;
        this.AccessToken = accessToken;
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
        if(string.IsNullOrEmpty(ApiKey) && string.IsNullOrEmpty(this.AccessToken))
            throw new Exception("API Key or Access Token is required to call the API.");
    }
}