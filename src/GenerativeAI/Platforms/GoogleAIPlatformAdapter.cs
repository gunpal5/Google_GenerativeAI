using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// The GoogleAIPlatformAdapter class provides an implementation of the IPlatformAdapter interface
/// to integrate with Google AI Generative API. It handles authorization, URL generation, and
/// credential management for making requests to the Google AI platform.
/// </summary>
public class GoogleAIPlatformAdapter : IPlatformAdapter
{
    /// <summary>
    /// Gets the credentials used for authentication when making requests to the Google AI platform.
    /// This property provides an instance of <see cref="GoogleAICredentials"/> which encapsulates
    /// the API key and optionally an access token required for authorization.
    /// </summary>
    public GoogleAICredentials Credentials { get; private set; }

    /// <summary>
    /// Gets or sets the base URL used for making requests to the Google AI Generative API.
    /// By default, this property is initialized to the URL specified in <see cref="BaseUrls.GoogleGenerativeAI"/>.
    /// It serves as the foundational endpoint for constructing resource-specific URLs.
    /// </summary>
    public string BaseUrl { get; set; } = BaseUrls.GoogleGenerativeAI;

    /// <summary>
    /// Gets or sets the API version used for constructing API request URLs in the integration
    /// with the Google AI platform. This property must be set to a valid version string defined in <see cref="ApiVersions"/>.
    /// </summary>
    public string ApiVersion { get; set; } = ApiVersions.v1Beta;

    public IGoogleAuthenticator? Authenticator { get; set; }
    bool ValidateAccessToken { get; set; } = true;
    ILogger? Logger { get; set; }

    /// Represents an adapter to interact with the Google GenAI Platform.
    /// This class is responsible for managing API calls, constructing URLs,
    /// adding authorization headers, and handling versioning for the Google GenAI API.
    public GoogleAIPlatformAdapter(string? googleApiKey, string apiVersion = ApiVersions.v1Beta,
        string? accessToken = null, IGoogleAuthenticator? authenticator = null, bool validateAccessToken = true,
        ILogger? logger = null)
    {
        googleApiKey = googleApiKey ?? EnvironmentVariables.GOOGLE_API_KEY;
        if(string.IsNullOrEmpty(googleApiKey))
            throw new Exception("API Key is required for Google Gemini AI.");
        Credentials = new GoogleAICredentials(googleApiKey);
        this.ApiVersion = apiVersion;
        this.Authenticator = authenticator;
        if (!string.IsNullOrEmpty(accessToken))
            Credentials.AuthToken = new AuthTokens(accessToken);
        this.ValidateAccessToken = validateAccessToken;
        this.Logger = logger;
    }

    /// <inheritdoc/>
    public async Task AddAuthorizationAsync(HttpRequestMessage request, bool requireAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (!requireAccessToken)
        {
            await this.ValidateCredentialsAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(Credentials.ApiKey))
                request.Headers.Add("x-goog-api-key", Credentials.ApiKey);
            if (this.Credentials.AuthToken != null && this.Credentials.AuthToken.Validate())
                request.Headers.Add("Authorization", "Bearer " + Credentials.AuthToken.AccessToken);
        }
        else
        {
            if (this.Credentials == null || this.Credentials.AuthToken == null)
            {
                await this.AuthorizeAsync(cancellationToken).ConfigureAwait(false);
            }

            if (this.Credentials != null && this.Credentials.AuthToken != null &&
                !this.Credentials.AuthToken.Validate())
            {
                if (this.Authenticator != null)
                {
                    var newToken =
                        await this.Authenticator.RefreshAccessTokenAsync(this.Credentials.AuthToken, cancellationToken).ConfigureAwait(false);
                    if (newToken != null)
                    {
                        this.Credentials.AuthToken.AccessToken = newToken.AccessToken;
                        this.Credentials.AuthToken.ExpiryTime = newToken.ExpiryTime;
                    }
                    else
                    {
                        Logger?.LogError("Error while refreshing access token. Please try again.");
                        throw new UnauthorizedAccessException("Error while refreshing access token. Please try again.");
                    }
                }
            }

            await this.ValidateCredentialsAsync(true, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(Credentials?.ApiKey))
                request.Headers.Add("x-goog-api-key", Credentials.ApiKey);
            if (this.Credentials?.AuthToken != null && !string.IsNullOrEmpty(Credentials.AuthToken.AccessToken))
                request.Headers.Add("Authorization", "Bearer " + Credentials.AuthToken.AccessToken);
        }
    }
    /// <inheritdoc/>
    public async Task ValidateCredentialsAsync(CancellationToken cancellationToken = default)
    {
        await ValidateCredentialsAsync(false, cancellationToken).ConfigureAwait(false);
        
    }

    /// <inheritdoc/>
    public async Task ValidateCredentialsAsync(bool requireAccessToken, CancellationToken cancellationToken = default)
    {
        if (!requireAccessToken)
        {
            Credentials.ValidateCredentials();
        }
        else
        {
            if (this.Credentials == null)
                throw new Exception("Credentials are required for Google Gemini AI.");
            if (ValidateAccessToken && this.Credentials.AuthToken != null &&
                !this.Credentials.AuthToken.ExpiryTime.HasValue)
            {
                if (this.Authenticator == null)
                {
                    var adcAuthenticator = new GoogleCloudAdcAuthenticator();
                    var token = await adcAuthenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken, true,
                        cancellationToken).ConfigureAwait(false);
                    // this.Credentials.AuthToken.AccessToken = token.AccessToken;
                    this.Credentials.AuthToken.ExpiryTime = token?.ExpiryTime;
                }
                else
                {
                    var token = await this.Authenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken,
                        false, cancellationToken).ConfigureAwait(false);
                    if (token != null)
                    {
                        //this.Credentials.AuthToken.AccessToken = token.AccessToken;
                        this.Credentials.AuthToken.ExpiryTime = token.ExpiryTime;
                    }
                    else
                    {
                        var newToken = await this.Authenticator.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                        if (newToken != null)
                        {
                            this.Credentials.AuthToken.AccessToken = newToken.AccessToken;
                            this.Credentials.AuthToken.ExpiryTime = newToken.ExpiryTime;
                        }
                        else throw new UnauthorizedAccessException("Access token is invalid.");
                    }
                }
            }

            var credentialsAuthToken = this.Credentials.AuthToken;
            if (credentialsAuthToken != null && !credentialsAuthToken.Validate())
            {
                throw new UnauthorizedAccessException("Access token is invalid.");
            }
        }
    }

    /// <inheritdoc/>
    public async Task AuthorizeAsync(CancellationToken cancellationToken = default)
    {
        //Authorize Through ADC
        if (this.Authenticator == null)
            this.Authenticator = new GoogleCloudAdcAuthenticator();

        var existingToken = this.Credentials?.AuthToken;

        var token = await Authenticator.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        if (this.Credentials == null)
            this.Credentials = new GoogleAICredentials("", token.AccessToken, token.ExpiryTime);
        else
        {
            if (this.Credentials.AuthToken == null)
                this.Credentials.AuthToken = token;
            else
            {
                this.Credentials.AuthToken.AccessToken = token.AccessToken;
                this.Credentials.AuthToken.ExpiryTime = token.ExpiryTime;
            }
        }
    }

    /// <inheritdoc/>
    public string GetBaseUrl(bool appendVesion = true, bool appendPublisher = true)
    {
        if (appendVesion)
            return $"{BaseUrl}/{GetApiVersion()}";
        return BaseUrl;
    }
    /// <inheritdoc/>
    public string GetBaseUrlForFile()
    {
        return GetBaseUrl();
    }

    /// <inheritdoc/>
    public string CreateUrlForModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToModelId()}:{task}";
    }

    /// <inheritdoc/>
    public string CreateUrlForTunedModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToTunedModelId()}:{task}";
    }

    /// <inheritdoc/>
    public string GetApiVersion()
    {
        if(string.IsNullOrEmpty(ApiVersion))
            ApiVersion = ApiVersions.v1Beta;
        return ApiVersion;
    }

   
    /// <inheritdoc/>
    public string GetApiVersionForFile()
    {
        return ApiVersion;
    }

  
    /// <inheritdoc />
    public void SetAuthenticator(IGoogleAuthenticator authenticator)
    {
       this.Authenticator = authenticator;
    }

    public string GetMultiModalLiveUrl(string version = "v1alpha")
    {
        return $"{BaseUrls.GoogleMultiModalLive.Replace("{version}",version)}?key={this.Credentials.ApiKey}";
    }
    
    /// <inheritdoc />
    public async Task<AuthTokens?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return null;
    }

    public string? GetMultiModalLiveModalName(string modelName)
    {
        return modelName.ToModelId();
    }
}