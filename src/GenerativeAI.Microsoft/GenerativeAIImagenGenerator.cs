#pragma warning disable MEAI001
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Core;
using GenerativeAI.Types;
using GenerativeAI.Clients;
using GenerativeAI.Microsoft.Extensions;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft;

/// <summary>
/// Implements Microsoft.Extensions.AI.IImageGenerator by creating an ImagenModel via GenAI.CreateImageModel
/// and calling <see cref="GenerativeAI.Clients.ImagenModel.GenerateImagesAsync(GenerateImageRequest, CancellationToken)"/>.
/// </summary>
public sealed class GenerativeAIImagenGenerator : IImageGenerator
{
    /// <summary>
    /// Underlying ImagenModel instance created from the provided GenAI factory.
    /// </summary>
    public ImagenModel model { get; }

    /// <summary>
    /// Creates a new instance using an API key and optional model name.
    /// </summary>
    public GenerativeAIImagenGenerator(string apiKey, string modelName = GoogleAIModels.Imagen.Imagen3Generate002):
        this(new GoogleAi(apiKey), modelName)
    { }
    

    /// <summary>
    /// Creates a new instance using an existing <see cref="GenAI"/> factory and optional model name.
    /// </summary>
    public GenerativeAIImagenGenerator(GenAI genai, string modelName = GoogleAIModels.Imagen.Imagen3Generate002)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(genai);
#else
        if (genai == null) throw new ArgumentNullException(nameof(genai));
#endif
        model = genai.CreateImageModel(modelName);
    }

    /// <inheritdoc/>
    public void Dispose()
    { }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceKey == null && serviceType?.IsInstanceOfType(this) == true)
            return this;
        return null;
    }

    /// <inheritdoc/>
    public async Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request,
        ImageGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif

        var imgRequest = ToGenerateImageRequest(request, options);
        var resp = await model.GenerateImagesAsync(imgRequest, cancellationToken).ConfigureAwait(false);
        return ToImageGenerationResponse(resp);
    }

    // Convert Microsoft ImageGenerationRequest + options to a GenerateImageRequest
    private GenerateImageRequest ToGenerateImageRequest(ImageGenerationRequest request, ImageGenerationOptions? options)
    {
        var imgRequest = new GenerateImageRequest();
        var instances = new List<ImageGenerationInstance>();

        if (request.OriginalImages != null && request.OriginalImages.Any())
        {
            instances.AddRange(request.OriginalImages.Select(content => new ImageGenerationInstance
            {
                Prompt = request.Prompt,
                Image = ConvertAiContentToImageSource(content)
            }));
        }
        else
        {
            instances.Add(new ImageGenerationInstance { Prompt = request.Prompt });
        }

        ImageGenerationParameters parameters = options?.RawRepresentationFactory?.Invoke(this) as ImageGenerationParameters ?? new();
        parameters.SampleCount = options?.Count ?? 1;

        if (options != null)
        {
            if (!string.IsNullOrEmpty(options.MediaType))
            {
                parameters.OutputOptions = new OutputOptions { MimeType = options.MediaType };                
            }

            if (options.ImageSize.HasValue)
            {
                var sz = options.ImageSize.Value;
                parameters.AspectRatio = $"{sz.Width}:{sz.Height}";

            }
        }

        return new GenerateImageRequest
        {
            Instances = instances,
            Parameters = parameters
        };
    }

    // Convert model response to Microsoft ImageGenerationResponse
    private static ImageGenerationResponse ToImageGenerationResponse(GenerateImageResponse? resp)
    {
        var contents = new List<AIContent>();
        if (resp?.Predictions != null)
        {
            foreach (var pred in resp.Predictions)
            {
                if (!string.IsNullOrEmpty(pred.BytesBase64Encoded))
                {
                    var data = Convert.FromBase64String(pred.BytesBase64Encoded);
                    contents.Add(new DataContent(data, pred.MimeType ?? "image/png"));
                }
            }
        }

        return new ImageGenerationResponse(contents) { RawRepresentation = resp };
    }

    private static ImageSource? ConvertAiContentToImageSource(AIContent content)
    {
        if (content == null) return null;

        if (content is DataContent dc)
        {
            return new ImageSource { BytesBase64Encoded = Convert.ToBase64String(dc.Data.ToArray()) };
        }

        if (content is UriContent uc)
        {
            var uriVal = uc.Uri?.ToString();

            // Only treat known GCS URIs as storage references for Imagen API.
            if (uriVal?.StartsWith("gs://", StringComparison.OrdinalIgnoreCase) == true || 
                uriVal?.IndexOf("storage.googleapis.com", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new ImageSource { GcsUri = uriVal };
            }
        }

        return null;
    }
}
#pragma warning restore MEAI001
