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
        var baseUrl = Platform.GetBaseUrl();

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
        var url = $"{Platform.GetBaseUrl()}/models{queryString}";

        return await GetAsync<ListModelsResponse>(url,cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates a tuned model's display name, description and/or default checkpoint (PATCH), like
    /// <c>models.update</c> of the google-genai SDK.
    /// </summary>
    /// <param name="model">
    /// The model to update: <c>tunedModels/{id}</c> on the Gemini API; <c>models/{id}</c> or
    /// <c>projects/{project}/locations/{location}/models/{id}</c> on Vertex AI.
    /// </param>
    /// <param name="config">The fields to update. Only the fields that are set are sent.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The updated <see cref="Model"/>.</returns>
    /// <seealso href="https://ai.google.dev/api/tuning#method:-tunedmodels.patch">See Official API Documentation</seealso>
    public async Task<Model> UpdateModelAsync(string model, UpdateModelConfig config,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config);
#else
        if (config == null) throw new ArgumentNullException(nameof(config));
#endif
        var url = ResolveModelUrl(model);
        return await SendAsync<UpdateModelConfig, Model>(url, config, new HttpMethod("PATCH"), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a tuned model, like <c>models.delete</c> of the google-genai SDK.
    /// </summary>
    /// <param name="model">
    /// The model to delete: <c>tunedModels/{id}</c> on the Gemini API; <c>models/{id}</c> or
    /// <c>projects/{project}/locations/{location}/models/{id}</c> on Vertex AI.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that completes when the model was deleted (or the deletion was accepted).</returns>
    /// <seealso href="https://ai.google.dev/api/tuning#method:-tunedmodels.delete">See Official API Documentation</seealso>
    public async Task DeleteModelAsync(string model, CancellationToken cancellationToken = default)
    {
        var url = ResolveModelUrl(model);
        await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the tokens and token ids of the given contents (Vertex AI only), like <c>models.compute_tokens</c>
    /// of the google-genai SDK. Use <c>CountTokensAsync</c> of <see cref="GenerativeModel"/> to count tokens on
    /// both platforms.
    /// </summary>
    /// <param name="model">The model, e.g. <c>gemini-2.5-flash</c>, <c>publishers/google/models/...</c> or a full resource name.</param>
    /// <param name="contents">The contents to tokenize.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The <see cref="ComputeTokensResponse"/> with the tokens info for each content.</returns>
    /// <exception cref="NotSupportedException">Called on the Gemini Developer API.</exception>
    public async Task<ComputeTokensResponse> ComputeTokensAsync(string model, IEnumerable<Content> contents,
        CancellationToken cancellationToken = default)
    {
        if (Platform is GoogleAIPlatformAdapter)
            throw new NotSupportedException(
                "computeTokens is only supported on Vertex AI, not on the Gemini Developer API.");
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(contents);
#else
        if (contents == null) throw new ArgumentNullException(nameof(contents));
#endif
        var url = $"{ResolveModelUrl(model)}:computeTokens";
        var request = new ComputeTokensRequest { Contents = contents.ToList() };
        return await SendAsync<ComputeTokensRequest, ComputeTokensResponse>(url, request, HttpMethod.Post,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves a model name to its URL the same way as <c>t_model</c> of the google-genai SDK.
    /// </summary>
    private string ResolveModelUrl(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model name cannot be null or empty.", nameof(model));
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (model.Contains("..") || model.Contains("?") || model.Contains("&"))
#else
        if (model.Contains("..", StringComparison.Ordinal) || model.Contains('?', StringComparison.Ordinal) ||
            model.Contains('&', StringComparison.Ordinal))
#endif
            throw new ArgumentException("Invalid model parameter.", nameof(model));

        if (Platform is GoogleAIPlatformAdapter)
        {
            var name = model.StartsWith("models/", StringComparison.Ordinal) ||
                       model.StartsWith("tunedModels/", StringComparison.Ordinal)
                ? model
                : $"models/{model}";
            return $"{Platform.GetBaseUrl()}/{name}";
        }

        if (model.StartsWith("projects/", StringComparison.Ordinal))
            return $"{Platform.GetBaseUrl(true, false, false, false)}/{model}";

        string resource;
        if (model.StartsWith("models/", StringComparison.Ordinal) ||
            model.StartsWith("publishers/", StringComparison.Ordinal))
        {
            resource = model;
        }
        else
        {
#if NETSTANDARD2_0 || NET462_OR_GREATER
            var slash = model.IndexOf('/');
#else
            var slash = model.IndexOf('/', StringComparison.Ordinal);
#endif
            resource = slash >= 0
                ? $"publishers/{model.Substring(0, slash)}/models/{model.Substring(slash + 1)}"
                : $"publishers/google/models/{model}";
        }

        return $"{Platform.GetBaseUrl(true, false)}/{resource}";
    }
}
