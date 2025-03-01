using System.Text.Json;
using GenerativeAI;
using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

/// <summary>
/// The VertextPlatformAdapter class provides authentication, configuration, and utility methods for interacting with Vertex AI.
/// </summary>
public class VertextPlatformAdapter : IPlatformAdapter
{
    /// <summary>
    /// Base URL used for API requests.
    /// </summary>
    private string BaseUrl { get; set; } = BaseUrls.VertexAI;

    /// <summary>
    /// The Google Cloud Project ID.
    /// </summary>
    public string? ProjectId { get; private set; }

    /// <summary>
    /// The region in which the Vertex AI service is hosted.
    /// </summary>
    public string Region { get; private set; }

    /// <summary>
    /// Indicates whether Express Mode is enabled.
    /// </summary>
    public bool? ExpressMode { get; private set; }

    /// <summary>
    /// The API version to use when making requests.
    /// </summary>
    public string ApiVersion { get; set; }

    /// <summary>
    /// Publisher information, defaulting to "google".
    /// </summary>
    public string Publisher { get; set; } = "google";

    /// <summary>
    /// The path to the credentials file.
    /// </summary>
    public string? CredentialFile { get; set; }

    /// <summary>
    /// The credentials object containing API key and access token.
    /// </summary>
    public GoogleAICredentials? Credentials { get; set; }

    /// <summary>
    /// The authenticator interface responsible for obtaining access tokens.
    /// </summary>
    public IGoogleAuthenticator? Authenticator { get; set; }

    /// <summary>
    /// Logger instance for diagnostic information.
    /// </summary>
    private ILogger? Logger { get; set; }

    /// <summary>
    /// Determines if access token validation should be performed.
    /// </summary>
    private bool ValidateAccessToken { get; set; } = true;

    /// <summary>
    /// Constructor for setting mandatory fields for Vertex AI operations.
    /// </summary>
    /// <param name="projectId">The Google Cloud Project ID.</param>
    /// <param name="region">The region for Vertex AI.</param>
    /// <param name="authenticator">The authenticator to be used for authentication.</param>
    /// <param name="apiVersion">The API version to use. Defaults to v1Beta1.</param>
    /// <param name="validateAccessToken">Boolean flag for validating the access token.</param>
    /// <param name="logger">Optional logger instance for diagnostics.</param>
    public VertextPlatformAdapter(string projectId, string region, IGoogleAuthenticator authenticator,
        string apiVersion = ApiVersions.v1Beta1,
        bool validateAccessToken = true,
        ILogger? logger = null)
    {
        this.ProjectId = projectId;
        this.Region = region;
        this.ApiVersion = apiVersion;
        if (authenticator == null)
            throw new Exception("Authenticator is required for Vertex AI.");
        this.Authenticator = authenticator;
    }

    /// <summary>
    /// Constructor for initializing the adapter using environment variables or manual parameters.
    /// </summary>
    /// <param name="projectId">The Google Cloud Project ID.</param>
    /// <param name="region">The region for Vertex AI.</param>
    /// <param name="expressMode">Flag to denote if Express Mode should be used.</param>
    /// <param name="apiKey">Optional API key for authentication.</param>
    /// <param name="accessToken">Optional access token for authentication.</param>
    /// <param name="apiVersion">The API version to use. Defaults to v1Beta1.</param>
    /// <param name="authenticator">Optional authenticator instance for custom token management.</param>
    /// <param name="credentialsFile">Path to the credentials file.</param>
    /// <param name="validateAccessToken">Whether access tokens should be validated before use.</param>
    /// <param name="logger">Optional logger instance for diagnostics.</param>
    public VertextPlatformAdapter(string? projectId = null, string? region = null, bool expressMode = false,
        string? apiKey = null,
        string? accessToken = null,
        string apiVersion = ApiVersions.v1Beta1,
        IGoogleAuthenticator? authenticator = null,
        string? credentialsFile = null,
        bool validateAccessToken = true,
        ILogger? logger = null)
    {
        this.ProjectId = projectId ?? EnvironmentVariables.GOOGLE_PROJECT_ID;
        this.Region = region ?? EnvironmentVariables.GOOGLE_REGION;
        this.ExpressMode = expressMode;
        this.ApiVersion = apiVersion;
        this.Authenticator = authenticator;
        accessToken = accessToken ?? EnvironmentVariables.GOOGLE_ACCESS_TOKEN;
        apiKey = apiKey ?? EnvironmentVariables.GOOGLE_API_KEY;

        credentialsFile = credentialsFile ?? EnvironmentVariables.GOOGLE_WEB_CREDENTIALS;

        if (string.IsNullOrEmpty(credentialsFile))
        {
            credentialsFile = EnvironmentVariables.GOOGLE_APPLICATION_CREDENTIALS;
        }

        if (string.IsNullOrEmpty(credentialsFile))
        {
            credentialsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "gcloud",
                "application_default_credentials.json");
        }

        if (string.IsNullOrEmpty(ProjectId))
        {
            var configuration = GetCredentialsFromFile(credentialsFile);
            if (configuration == null)
                throw new Exception("No configuration found for Vertex AI.");
            projectId = configuration.ProjectId;
            this.ProjectId = projectId;
            this.CredentialFile = credentialsFile;
        }

        if (expressMode == true)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API Key is required for Vertex AI Express.");
        }

        if (authenticator == null)
        {
            if (string.IsNullOrEmpty(accessToken))
                this.Authenticator = new GoogleCloudAdcAuthenticator(credentialsFile, logger);
        }

        else this.Authenticator = authenticator;

        if (!string.IsNullOrEmpty(accessToken) || !string.IsNullOrEmpty(apiKey))
        {
            this.Credentials = new GoogleAICredentials(apiKey, accessToken);
        }
    }

    /// <summary>
    /// Retrieves credentials configuration from a file.
    /// </summary>
    /// <param name="credentialsFile">Path to the credentials file.</param>
    /// <returns>A CredentialConfiguration object if successful, otherwise null.</returns>
    private CredentialConfiguration? GetCredentialsFromFile(string? credentialsFile)
    {
        if (string.IsNullOrEmpty(credentialsFile))
            return null;
        CredentialConfiguration? credentials = null;
        if (File.Exists(credentialsFile))
        {
            var options = DefaultSerializerOptions.Options;
            #if NET7_0_OR_GREATER
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            #elif NET6_0 || NET5_0
            options.PropertyNamingPolicy = new JsonSnakeCaseLowerNamingPolicy();
            #endif 
            var file = File.ReadAllText(credentialsFile);
            credentials = JsonSerializer.Deserialize<CredentialConfiguration>(file, options);
        }

        return credentials;
    }

    /// <summary>
    /// Adds authorization headers to an HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request message to be modified.</param>
    /// <param name="requireAccessToken">Indicates if an access token is required.</param>
    /// <param name="cancellationToken">Optional cancellation token for task cancellation.</param>
    public async Task AddAuthorizationAsync(HttpRequestMessage request, bool requireAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (this.Credentials == null || this.Credentials.AuthToken == null)
        {
            await this.AuthorizeAsync(cancellationToken).ConfigureAwait(false);
        }

        if (ExpressMode != true && this.Credentials != null && this.Credentials.AuthToken != null &&
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

        await this.ValidateCredentialsAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(Credentials.ApiKey))
            request.Headers.Add("x-goog-api-key", Credentials.ApiKey);
        if (this.Credentials.AuthToken != null && !string.IsNullOrEmpty(Credentials.AuthToken.AccessToken))
            request.Headers.Add("Authorization", "Bearer " + Credentials.AuthToken.AccessToken);
        if (!string.IsNullOrEmpty(ProjectId))
        {
            if (request.Headers.Contains("x-goog-user-project"))
            {
                request.Headers.Remove("x-goog-user-project");
            }

            request.Headers.Add("x-goog-user-project", ProjectId);
        }
    }

    /// <summary>
    /// Validates the credentials and refreshes the token if necessary.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for task cancellation.</param>
    public async Task ValidateCredentialsAsync(CancellationToken cancellationToken = default)
    {
        if (this.Credentials == null)
            throw new Exception("Credentials are required for Vertex AI.");
        if (ValidateAccessToken && this.Credentials.AuthToken != null &&
            !this.Credentials.AuthToken.ExpiryTime.HasValue)
        {
            if (this.Authenticator == null)
            {
                var adcAuthenticator = new GoogleCloudAdcAuthenticator();
                var token = await adcAuthenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken, true, cancellationToken).ConfigureAwait(false);
                this.Credentials.AuthToken.ExpiryTime = token?.ExpiryTime;
            }
            else
            {
                var token = await this.Authenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken, false, cancellationToken).ConfigureAwait(false);
                if (token != null)
                {
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

    /// <summary>
    /// Authorizes the request by generating or refreshing the access token.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for task cancellation.</param>
    public async Task AuthorizeAsync(CancellationToken cancellationToken = default)
    {
        // Authorize Through ADC
        if (this.Authenticator == null)
            this.Authenticator = new GoogleCloudAdcAuthenticator();

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

    /// <summary>
    /// Constructs the base URL for API requests, optionally appending the version.
    /// </summary>
    /// <param name="appendVesion">Indicates whether to append the version to the URL.</param>
    /// <returns>The constructed base URL string.</returns>
    public string GetBaseUrl(bool appendVesion = true, bool appendPublisher = true)
    {
        if (ExpressMode == true)
        {
            if(appendPublisher)
                return $"{BaseUrls.VertexAIExpress}/{ApiVersion}/publishers/{Publisher}";
            else return $"{BaseUrls.VertexAIExpress}/{ApiVersion}";
        }
#if NETSTANDARD2_0 || NET462_OR_GREATER
        var url = this.BaseUrl.Replace("{region}", Region)
            .Replace("{projectId}", ProjectId)
            .Replace("{version}", ApiVersion);
#else
        var url = this.BaseUrl.Replace("{region}", Region, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{projectId}", ProjectId, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{version}", ApiVersion, StringComparison.InvariantCultureIgnoreCase);
#endif

        if(appendPublisher)
            return $"{url}/publishers/{Publisher}";
        else return url;
    }

    /// <summary>
    /// Gets the base URL specifically for file-related operations.
    /// </summary>
    /// <returns>The constructed base URL string for file operations.</returns>
    public string GetBaseUrlForFile()
    {
        return $"{BaseUrls.GoogleGenerativeAI}";
    }

    /// <summary>
    /// Generates the URL for a specific model and task.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <param name="task">The specific task to perform.</param>
    /// <returns>The constructed URL as a string.</returns>
    public string CreateUrlForModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToModelId()}:{task}";
    }

    /// <summary>
    /// Generates the URL for a specific tuned model and task.
    /// </summary>
    /// <param name="modelId">The tuned model ID.</param>
    /// <param name="task">The specific task to perform.</param>
    /// <returns>The constructed URL as a string.</returns>
    public string CreateUrlForTunedModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToTunedModelId()}:{task}";
    }

    /// <summary>
    /// Retrieves the current API version being used.
    /// </summary>
    /// <returns>The API version as a string.</returns>
    public string GetApiVersion()
    {
        return this.ApiVersion;
    }

    /// <summary>
    /// Retrieves the API version specifically for file operations.
    /// </summary>
    /// <returns>The API version as an object (v1Beta by default).</returns>
    public string GetApiVersionForFile()
    {
        return ApiVersions.v1Beta;
    }

    public void SetAuthenticator(IGoogleAuthenticator authenticator)
    {
        this.Authenticator = authenticator;
    }

    public string GetMultiModalLiveUrl(string version = "v1alpha")
    {
        return BaseUrls.VertexMultiModalLive.Replace("{version}", "v1beta1").Replace("{location}", Region).Replace("{projectId}",ProjectId);
    }

    
    /// <inheritdoc />
    public async Task<AuthTokens?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if(this.Credentials == null || this.Credentials.AuthToken == null)
            await this.AuthorizeAsync(cancellationToken).ConfigureAwait(false);
        if(this.Credentials.AuthToken != null && this.Credentials.AuthToken.Validate() == false)
            throw new UnauthorizedAccessException("Unable to get access token. Please try again.");
        return this.Credentials.AuthToken;
    }

    public string? GetMultiModalLiveModalName(string modelName)
    {
       var transformed = "projects/{project}/locations/{location}/publishers/google/{model}";
//        var transformed = "publishers/google/{model}";
        //var transformed = "{model}";
        var id = transformed.Replace("{project}", ProjectId).Replace("{location}", Region).Replace("{model}", modelName.ToModelId());
        return id;

    }
}