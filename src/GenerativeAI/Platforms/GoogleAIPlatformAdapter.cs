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

    IGoogleAuthenticator? Authenticator { get; set; }
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

    /// <summary>
    /// Adds the required authorization headers to the provided HTTP request message.
    /// This includes both API key and OAuth2 Bearer token as applicable.
    /// </summary>
    /// <param name="request">The HTTP request message to which the authorization headers will be added.</param>
    /// <param name="requireAccessToken"></param>
    /// <param name="cancellationToken"></param>
    public async Task AddAuthorizationAsync(HttpRequestMessage request, bool requireAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (!requireAccessToken)
        {
            await this.ValidateCredentialsAsync(cancellationToken).ConfigureAwait(true);
            if (!string.IsNullOrEmpty(Credentials.ApiKey))
                request.Headers.Add("x-goog-api-key", Credentials.ApiKey);
            if (this.Credentials.AuthToken != null && this.Credentials.AuthToken.Validate())
                request.Headers.Add("Authorization", "Bearer " + Credentials.AuthToken.AccessToken);
        }
        else
        {
            if (this.Credentials == null || this.Credentials.AuthToken == null)
            {
                await this.AuthorizeAsync(cancellationToken).ConfigureAwait(true);
            }

            if (this.Credentials != null && this.Credentials.AuthToken != null &&
                !this.Credentials.AuthToken.Validate())
            {
                if (this.Authenticator != null)
                {
                    var newToken =
                        await this.Authenticator.RefreshAccessTokenAsync(this.Credentials.AuthToken, cancellationToken).ConfigureAwait(true);
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

            await this.ValidateCredentialsAsync(true, cancellationToken).ConfigureAwait(true);

            if (!string.IsNullOrEmpty(Credentials.ApiKey))
                request.Headers.Add("x-goog-api-key", Credentials.ApiKey);
            if (this.Credentials.AuthToken != null && !string.IsNullOrEmpty(Credentials.AuthToken.AccessToken))
                request.Headers.Add("Authorization", "Bearer " + Credentials.AuthToken.AccessToken);
        }
    }

    public async Task ValidateCredentialsAsync(CancellationToken cancellationToken = default)
    {
        await ValidateCredentialsAsync(false, cancellationToken).ConfigureAwait(true);
        
    }

    /// Validates the current credentials by ensuring that the API Key or
    /// Access Token is present. If neither is available, an exception is thrown.
    /// <param name="cancellationToken"></param>
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
                        cancellationToken).ConfigureAwait(true);
                    // this.Credentials.AuthToken.AccessToken = token.AccessToken;
                    this.Credentials.AuthToken.ExpiryTime = token.ExpiryTime;
                }
                else
                {
                    var token = await this.Authenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken,
                        false, cancellationToken).ConfigureAwait(true);
                    if (token != null)
                    {
                        //this.Credentials.AuthToken.AccessToken = token.AccessToken;
                        this.Credentials.AuthToken.ExpiryTime = token.ExpiryTime;
                    }
                    else
                    {
                        var newToken = await this.Authenticator.GetAccessTokenAsync(cancellationToken).ConfigureAwait(true);
                        if (newToken != null)
                        {
                            this.Credentials.AuthToken.AccessToken = newToken.AccessToken;
                            this.Credentials.AuthToken.ExpiryTime = newToken.ExpiryTime;
                        }
                        else throw new UnauthorizedAccessException("Access token is invalid.");
                    }
                }
            }

            if (!this.Credentials.AuthToken.Validate())
            {
                throw new UnauthorizedAccessException("Access token is invalid.");
            }
        }
    }

    /// Asynchronously authorizes the adapter for interaction with the Google GenAI API.
    /// This process ensures the platform adapter is ready to make API calls by setting up any necessary authentication mechanisms.
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. Defaults to CancellationToken.None.
    /// </param>
    /// <return>
    /// A task representing the asynchronous operation.
    /// </return>
    public async Task AuthorizeAsync(CancellationToken cancellationToken = default)
    {
        //Authorize Through ADC
        if (this.Authenticator == null)
            this.Authenticator = new GoogleCloudAdcAuthenticator();

        var existingToken = this.Credentials?.AuthToken;

        var token = await Authenticator.GetAccessTokenAsync(cancellationToken);

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

    /// <summary>
    /// Constructs and retrieves the base URL for the Google Generative AI platform,
    /// optionally appending the API version to the URL.
    /// </summary>
    /// <param name="appendVesion">A boolean value indicating whether the API version should be appended to the base URL.</param>
    /// <returns>The base URL for the Google Generative AI platform, with or without the API version appended, based on the parameter value.</returns>
    public string GetBaseUrl(bool appendVesion = true)
    {
        if (appendVesion)
            return $"{BaseUrl}/{ApiVersion}";
        return BaseUrl;
    }

    public string GetBaseUrlForFile()
    {
        return GetBaseUrl();
    }

    /// <summary>
    /// Constructs a URL for a specific AI model and task by appending the model ID and task
    /// to the base URL and API version.
    /// </summary>
    /// <param name="modelId">The identifier of the model for which the URL is being constructed.</param>
    /// <param name="task">The specific task associated with the model (e.g., generateText).</param>
    /// <returns>A string representing the complete URL for the given model and task.</returns>
    public string CreateUrlForModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToModelId()}:{task}";
    }

    /// Constructs a URL for interacting with a tuned model on the Google Generative AI platform.
    /// This method appends the base URL, model ID (transformed into a tuned model identifier),
    /// and the specified task to form the complete endpoint.
    /// <param name="modelId">The identifier of the model to be used.</param>
    /// <param name="task">The specific task or operation to be performed on the model.</param>
    /// <return>Returns the fully constructed URL string for the tuned model endpoint.</return>
    public string CreateUrlForTunedModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToTunedModelId()}:{task}";
    }

    /// Retrieves the current API version being used by the Google AI Platform Adapter.
    /// This version determines the API endpoint to which requests are directed.
    /// <return>The API version string.</return>
    public string GetApiVersion()
    {
        return ApiVersion;
    }

    public object GetApiVersionForFile()
    {
        return ApiVersion;
    }
}