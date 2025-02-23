using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// A client for interacting with the Imagen API to generate images.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class ImagenModel : BaseClient
{
    string _modelName;

    /// <summary>
    /// A client for interacting with the Imagen API to generate images.
    /// </summary>
    /// <param name="platform">The platform adapter providing platform-specific API operations.</param>
    /// <param name="modelName">The name of the model to be employed for image generation.</param>
    /// <param name="httpClient">Optional <see cref="HttpClient"/> instance for making HTTP requests.</param>
    /// <param name="logger">Optional <see cref="ILogger"/> instance for logging operations.</param>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
    public ImagenModel(IPlatformAdapter platform, string modelName, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
        this._modelName = modelName;
    }

    /// <summary>
    /// Generates images based on the provided <see cref="GenerateImageRequest"/>.
    /// </summary>
    /// <param name="request">The <see cref="GenerateImageRequest"/> containing the prompt and parameters.</param>
    /// <returns>A <see cref="GenerateImageResponse"/> containing the generated images.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
    public async Task<GenerateImageResponse?> GenerateImagesAsync(GenerateImageRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{_modelName.ToModelId()}:predict";
        return await SendAsync<GenerateImageRequest, GenerateImageResponse>(url, request, HttpMethod.Post,cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Generates images based on the provided prompt, optional image source, and generation parameters.
    /// </summary>
    /// <param name="prompt">The text prompt describing the desired image content.</param>
    /// <param name="imageSource">An optional <see cref="ImageSource"/> used as a reference for generating the image.</param>
    /// <param name="parameters">Optional <see cref="ImageGenerationParameters"/> that define additional generation settings.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="GenerateImageResponse"/> containing the generated images.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
    public async Task<GenerateImageResponse?> GenerateImagesAsync(string prompt, ImageSource? imageSource = null, ImageGenerationParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateImageRequest();
        request.AddPrompt(prompt, imageSource);
        request.AddParameters(parameters);
        return await GenerateImagesAsync(request, cancellationToken);
    }
}