namespace GenerativeAI.Core;

/// <summary>
/// Defines an interface for platform-specific operations required to integrate with external APIs.
/// </summary>
public interface IPlatformAdapter
{
    /// <summary>
    /// Gets or sets the authenticator responsible for managing authentication processes with Google services.
    /// </summary>
    /// <remarks>
    /// This property is used to handle authentication tasks, such as obtaining
    /// and refreshing access tokens, that are necessary to interact with Google APIs.
    /// </remarks>
    public IGoogleAuthenticator? Authenticator { get; set; }
    /// <summary>
    /// Adds the necessary authorization headers to the given HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which authorization headers will be added.</param>
    /// <param name="requireAccessToken">Specifies whether an access token is required for the request.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAuthorizationAsync(HttpRequestMessage request, bool requireAccessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates user credentials before making API requests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ValidateCredentialsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Authorizes the user by acquiring necessary tokens or credentials.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AuthorizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the base URL of the platform's API.
    /// </summary>
    /// <param name="appendVesion">Determines whether to append the API version to the URL.</param>
    /// <param name="appendPublisher">Determines whether to append the publisher identifier to the URL.</param>
    /// <returns>The base URL as a string.</returns>
    string GetBaseUrl(bool appendVesion = true, bool appendPublisher = true);

    /// <summary>
    /// Retrieves the base URL for file-related API operations.
    /// </summary>
    /// <returns>The base URL for file operations as a string.</returns>
    string GetBaseUrlForFile();

    /// <summary>
    /// Constructs a URL for a specific AI model operation.
    /// </summary>
    /// <param name="modelId">The unique identifier of the model.</param>
    /// <param name="task">The specific task for the model (e.g., training or evaluation).</param>
    /// <returns>The constructed URL as a string.</returns>
    string CreateUrlForModel(string modelId, string task);

    /// <summary>
    /// Constructs a URL for operations involving a fine-tuned model.
    /// </summary>
    /// <param name="modelId">The unique identifier of the fine-tuned model.</param>
    /// <param name="task">The specific task involving the fine-tuned model.</param>
    /// <returns>The constructed URL as a string.</returns>
    string CreateUrlForTunedModel(string modelId, string task);

    /// <summary>
    /// Retrieves the current API version used by the platform.
    /// </summary>
    /// <returns>The API version as a string.</returns>
    string GetApiVersion();

    /// <summary>
    /// Retrieves the API version related to file operations.
    /// </summary>
    /// <returns>The API version for file operations.</returns>
    string GetApiVersionForFile();


    /// <summary>
    /// Sets the Google authenticator responsible for managing authentication processes for Google services.
    /// </summary>
    /// <param name="authenticator">The Google authenticator to be used for handling authentication operations.</param>
    void SetAuthenticator(IGoogleAuthenticator authenticator);

    string GetMultiModalLiveUrl(string version = "v1alpha");

    /// <summary>
    /// Retrieves the access token required for making authenticated API requests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if needed.</param>
    /// <returns>A task that represents the asynchronous operation, containing the authentication tokens.</returns>
    Task<AuthTokens?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    string? GetMultiModalLiveModalName(string modelName);
}