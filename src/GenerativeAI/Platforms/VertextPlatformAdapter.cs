using System.Text.Json;
using GenerativeAI.Authenticators;
using GenerativeAI.Core;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

public class VertextPlatformAdapter : IPlatformAdapter
{
    private string BaseUrl { get; set; } = BaseUrls.VertexAI;
    public string ProjectId { get; private set; }
    public string Region { get; private set; }
    public bool? ExpressMode { get; private set; }
    public string ApiVersion { get; set; }

    public string Publisher { get; set; } = "google";

    public string? CredentialFile { get; set; }
    public GoogleAICredentials? Credentials { get; set; }
    private IGoogleAuthenticator? Authenticator { get; set; }
    private ILogger? Logger { get; set; }

    bool ValidateAccessToken { get; set; } = true;
    //Todo write a default constructor

    // public VertextPlatformAdapter(string projectId, string region,
    //     string apiVersion = ApiVersions.v1Beta1)
    // {
    //     this.Authenticator = new GoogleCloudAdcAuthenticator();
    // }

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

    public VertextPlatformAdapter(string? projectId = null, string? region = null, bool expressMode = false,
        string? apiKey = null,
        string? accessToken = null,
        string ApiVersion = ApiVersions.v1Beta1,
        IGoogleAuthenticator? authenticator = null,
        string credentialsFile = null,
        bool validateAccessToken = true,
        ILogger? logger = null)
    {
        this.ProjectId = projectId ?? EnvironmentVariables.GOOGLE_PROJECT_ID;
        this.Region = region ?? EnvironmentVariables.GOOGLE_REGION;
        this.ExpressMode = expressMode;
        this.ApiVersion = ApiVersion;
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
            if(string.IsNullOrEmpty(accessToken))
                this.Authenticator = new GoogleCloudAdcAuthenticator(credentialsFile, logger);
        }
            
        else this.Authenticator = authenticator;
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            this.Credentials = new GoogleAICredentials(apiKey, accessToken);
        }
    }

    private CredentialConfiguration? GetCredentialsFromFile(string? credentialsFile)
    {
        if (string.IsNullOrEmpty(credentialsFile))
            return null;
        CredentialConfiguration? credentials = null;
        if (File.Exists(credentialsFile))
        {
            var options = DefaultSerializerOptions.Options;
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            var file = File.ReadAllText(credentialsFile);
            credentials = JsonSerializer.Deserialize<CredentialConfiguration>(file, options);
        }

        return credentials;
    }

    public async Task AddAuthorizationAsync(HttpRequestMessage request, bool requireAccessToken,
        CancellationToken cancellationToken = default)
    {
        if (this.Credentials == null || this.Credentials.AuthToken == null)
        {
            await this.AuthorizeAsync(cancellationToken);
        }

        if (ExpressMode != true && this.Credentials != null && this.Credentials.AuthToken != null &&
            !this.Credentials.AuthToken.Validate())
        {
            if (this.Authenticator != null)
            {
                var newToken =
                    await this.Authenticator.RefreshAccessTokenAsync(this.Credentials.AuthToken, cancellationToken);
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

        await this.ValidateCredentialsAsync(cancellationToken);

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
                var token = await adcAuthenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken, true,cancellationToken);
                // this.Credentials.AuthToken.AccessToken = token.AccessToken;
                this.Credentials.AuthToken.ExpiryTime = token.ExpiryTime;
            }
            else
            {
                var token = await this.Authenticator.ValidateAccessTokenAsync(Credentials.AuthToken.AccessToken,false,cancellationToken);
                if (token != null)
                {
                    //this.Credentials.AuthToken.AccessToken = token.AccessToken;
                    this.Credentials.AuthToken.ExpiryTime = token.ExpiryTime;
                }
                else
                {
                    var newToken = await this.Authenticator.GetAccessTokenAsync(cancellationToken);
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

    public string GetBaseUrl(bool appendVesion = true)
    {
        if (ExpressMode == true)
        {
            return $"{BaseUrls.VertexAIExpress}/{ApiVersion}/publishers/{Publisher}";
        }
#if NETSTANDARD2_0|| NET462_OR_GREATER
        var url = this.BaseUrl.Replace("{region}", Region)
            .Replace("{projectId}", ProjectId)
            .Replace("{version}", ApiVersion);
#else
        var url = this.BaseUrl.Replace("{region}", Region, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{projectId}", ProjectId, StringComparison.InvariantCultureIgnoreCase)
            .Replace("{version}", ApiVersion, StringComparison.InvariantCultureIgnoreCase);
#endif

        return $"{url}/publishers/{Publisher}";
    }

    public string GetBaseUrlForFile()
    {
        return $"{BaseUrls.GoogleGenerativeAI}";
    }

    public string CreateUrlForModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToModelId()}:{task}";
    }

    public string CreateUrlForTunedModel(string modelId, string task)
    {
        return $"{GetBaseUrl()}/{modelId.ToTunedModelId()}:{task}";
    }

    public string GetApiVersion()
    {
        return this.ApiVersion;
    }

    public object GetApiVersionForFile()
    {
        return ApiVersions.v1Beta;
    }
}