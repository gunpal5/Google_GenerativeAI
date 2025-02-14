using GenerativeAI.Core;

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
    public GoogleAICredentials Credentials { get; }

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

    /// Represents an adapter to interact with the Google GenAI Platform.
    /// This class is responsible for managing API calls, constructing URLs,
    /// adding authorization headers, and handling versioning for the Google GenAI API.
    public GoogleAIPlatformAdapter(string googleApiKey,string apiVersion = ApiVersions.v1Beta)
    {
        Credentials = new GoogleAICredentials(googleApiKey);
        this.ApiVersion = apiVersion;
    }

    /// <summary>
    /// Adds the required authorization headers to the provided HTTP request message.
    /// This includes both API key and OAuth2 Bearer token as applicable.
    /// </summary>
    /// <param name="request">The HTTP request message to which the authorization headers will be added.</param>
    public void AddAuthorization(HttpRequestMessage request)
    {
        this.ValidateCredentials();
        if(!string.IsNullOrEmpty(Credentials.ApiKey))
            request.Headers.Add("x-goog-api-key",Credentials.ApiKey);
        if(!string.IsNullOrEmpty(Credentials.AccessToken))
            request.Headers.Add("Authorization","Bearer "+Credentials.AccessToken);
    }

    /// Validates the current credentials by ensuring that the API Key or
    /// Access Token is present. If neither is available, an exception is thrown.
    public void ValidateCredentials()
    {
        Credentials.ValidateCredentials();
    }

    /// Asynchronously authorizes the adapter for interaction with the Google GenAI API.
    /// This process ensures the platform adapter is ready to make API calls by setting up any necessary authentication mechanisms.
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. Defaults to CancellationToken.None.
    /// </param>
    /// <return>
    /// A task representing the asynchronous operation.
    /// </return>
    public Task AuthorizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Constructs and retrieves the base URL for the Google Generative AI platform,
    /// optionally appending the API version to the URL.
    /// </summary>
    /// <param name="appendVesion">A boolean value indicating whether the API version should be appended to the base URL.</param>
    /// <returns>The base URL for the Google Generative AI platform, with or without the API version appended, based on the parameter value.</returns>
    public string GetBaseUrl(bool appendVesion = true)
    {
        if(appendVesion)
            return $"{BaseUrl}/{ApiVersion}";
        return BaseUrl;
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
}