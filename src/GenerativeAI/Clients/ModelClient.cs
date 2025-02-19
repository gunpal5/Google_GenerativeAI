using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Gemini API Models endpoint.
/// </summary>
/// <seealso href="https://ai.google.dev/api">See Official API Documentation</seealso>
public class ModelClient : BaseClient
{
    /// <summary>
    /// A client for interacting with the Gemini API Models endpoint, providing methods to retrieve and list
    /// available models for use with the Generative AI platform.
    /// </summary>
    /// <param name="platform">The platform adapter used to manage API endpoints and authentication.</param>
    /// <param name="httpClient">The HTTP client instance used for making API requests.</param>
    /// <param name="logger">An optional logger instance for logging operations.</param>
    /// <remarks>
    /// The <see cref="ModelClient"/> serves as an abstraction to interact with the API's model-related
    /// endpoints. It is responsible for retrieving model metadata and listing available models on the platform.
    /// </remarks>
    /// <seealso href="https://ai.google.dev/api">See Official API Documentation</seealso>
    public ModelClient(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform,
        httpClient, logger)
    {
    }

    /// <summary>
    /// Gets information about a specific <see cref="Model"/> such as its version number, token limits,
    /// parameters and other metadata.
    /// </summary>
    /// <param name="name">The resource name of the model.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="Model"/> information.</returns>
    /// <seealso href="https://ai.google.dev/api/models#method:-models.get">See Official API Documentation</seealso>
    public async Task<Model> GetModelAsync(string name, CancellationToken cancellationToken = default)
    {
        var baseUrl = _platform.GetBaseUrl();

        var url = $"{baseUrl}/{name.ToModelId()}";
        return await GetAsync<Model>(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists the <see cref="Model"/>s available through the Gemini API.
    /// </summary>
    /// <param name="pageSize">The maximum number of <see cref="Model"/>s to return per page.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListModelsAsync"/> call, to retrieve the next page of results.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="ListModelsResponse"/> containing a list of <see cref="Model"/>s and associated metadata.</returns>
    /// <seealso href="https://ai.google.dev/api/models#method:-models.list">See Official API Documentation</seealso>
    public async Task<ListModelsResponse> ListModelsAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (pageSize.HasValue)
        {
            queryParams.Add($"pageSize={pageSize.Value}");
        }

        if (!string.IsNullOrEmpty(pageToken))
        {
            queryParams.Add($"pageToken={pageToken}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
        var url = $"{_platform.GetBaseUrl()}/models{queryString}";

        return await GetAsync<ListModelsResponse>(url,cancellationToken).ConfigureAwait(false);
    }
}
