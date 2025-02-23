using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// Provides functionality for image-to-text generative AI tasks including image captioning
/// and visual question answering (VQA). Offers methods to interact with both local image files
/// and images hosted on cloud storage.
/// </summary>
/// <remarks>
/// This class integrates with the AI platform defined by <see cref="IPlatformAdapter"/> and
/// provides access to the platform's image text processing capabilities.
/// Inherited from <see cref="BaseClient"/>, it uses HTTP communication and logging mechanisms defined at a higher level.
/// </remarks>
public class ImageTextModel: BaseClient
{
    /// <summary>
    /// Represents a client for interacting with image-to-text models, including functionality for image captioning and visual question answering.
    /// </summary>
    /// <remarks>
    /// This class provides various methods to generate captions or answer questions about images from local files or cloud storage.
    /// </remarks>
    public ImageTextModel(IPlatformAdapter platform, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, httpClient, logger)
    {
    }
    /// <summary>
    /// Generate image captions for a given image.
    /// </summary>
    /// <param name="request">The <see cref="ImageCaptioningRequest"/> containing the image and parameters.</param>
    /// <param name="cancellationToken">The CancellationToken to cancel the operation.</param>
    /// <returns>The <see cref="ImageCaptioningResponse"/> containing the predicted captions.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
    public async Task<ImageCaptioningResponse?> GenerateImageCaptionAsync(ImageCaptioningRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/models/imagetext:predict";
        
        return await SendAsync<ImageCaptioningRequest, ImageCaptioningResponse>(url, request, HttpMethod.Post, cancellationToken);
    }
    
    /// <summary>
    /// Predicts the answer to a question about an image using the Imagen for Captioning &amp; VQA model.
    /// </summary>
    /// <param name="request">The <see cref="VqaRequest"/> containing the image and question.</param>
    /// <param name="cancellationToken">The CancellationToken to cancel the operation.</param>
    /// <returns>The <see cref="VqaResponse"/> containing the predicted answers.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
    public async Task<VqaResponse?> VisualQuestionAnsweringAsync(VqaRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/models/imagetext:predict";
        
        return await SendAsync<VqaRequest, VqaResponse>(url, request, HttpMethod.Post, cancellationToken);
    }

    /// <summary>
    /// Generates image captions for a local image file.
    /// </summary>
    /// <param name="imagePath">The file path of the local image to be processed.</param>
    /// <param name="parameters">Optional parameters to configure the image captioning request.</param>
    /// <param name="cancellationToken">The CancellationToken to cancel the operation.</param>
    /// <returns>The <see cref="ImageCaptioningResponse"/> containing the predicted captions.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
    public async Task<ImageCaptioningResponse?> GenerateImageCaptionFromLocalFileAsync(string imagePath,
        ImageCaptioningParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        var request = new ImageCaptioningRequest();
        request.AddLocalImage(imagePath);
        request.Parameters = parameters;
        return await GenerateImageCaptionAsync(request, cancellationToken);
    }

    /// <summary>
    /// Generates image captions for an image stored in Google Cloud Storage (GCS).
    /// </summary>
    /// <param name="imageUri">The Google Cloud Storage URI of the image to be processed.</param>
    /// <param name="parameters">Optional parameters for image captioning customization.</param>
    /// <param name="cancellationToken">The CancellationToken to cancel the operation.</param>
    /// <returns>The <see cref="ImageCaptioningResponse"/> containing the predicted captions.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/image-captioning">See Official API Documentation</seealso>
    public async Task<ImageCaptioningResponse?> GenerateImageCaptionFromGcsImageAsync(string imageUri,
        ImageCaptioningParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ImageCaptioningRequest();
        request.AddGcsImage(imageUri);
        request.Parameters = parameters;
        return await GenerateImageCaptionAsync(request, cancellationToken);
    }


    /// <summary>
    /// Processes a visual question answering task on a local image file.
    /// </summary>
    /// <param name="prompt">The question or query to be answered based on the image content.</param>
    /// <param name="imagePath">The file path of the local image to be analyzed.</param>
    /// <param name="parameters">Optional parameters to configure the visual question answering request.</param>
    /// <param name="cancellationToken">The CancellationToken to cancel the operation.</param>
    /// <returns>The <see cref="VqaResponse"/> containing the answer to the specified question.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
    public async Task<VqaResponse?> VisualQuestionAnsweringFromLocalFileAsync(string prompt, string imagePath,
        VqaParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        var request = new VqaRequest();
        request.AddLocalImage(prompt, imagePath);
        request.Parameters = parameters;
        return await VisualQuestionAnsweringAsync(request, cancellationToken);
    }

    /// <summary>
    /// Performs visual question answering for an image stored in Google Cloud Storage (GCS).
    /// </summary>
    /// <param name="prompt">The question or prompt to be applied to the image.</param>
    /// <param name="imageUri">The Google Cloud Storage URI of the image to be analyzed.</param>
    /// <param name="parameters">Optional parameters for visual question answering customization.</param>
    /// <param name="cancellationToken">The CancellationToken to cancel the operation.</param>
    /// <returns>The <see cref="VqaResponse"/> containing the results of the visual question answering process.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
    public async Task<VqaResponse?> VisualQuestionAnsweringFromGcsFileAsync(string prompt, string imageUri,
        VqaParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var request = new VqaRequest();
        request.AddGcsImage(prompt, imageUri);
        request.Parameters = parameters;
        return await VisualQuestionAnsweringAsync(request, cancellationToken);
    }
}