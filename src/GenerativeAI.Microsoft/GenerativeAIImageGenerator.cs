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
using GenerativeAI.Microsoft.Extensions;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft;

/// <summary>
/// Implements Microsoft.Extensions.AI.IImageGenerator using the Google_GenerativeAI SDK by
/// creating a GenerateContentRequest that requests image modality and forwarding it to
/// <see cref="GenerativeModel.GenerateContentAsync(GenerateContentRequest, CancellationToken)"/>.
/// </summary>
public sealed class GenerativeAIImageGenerator : IImageGenerator
{
    /// <summary>
    /// Underlying GenerativeModel instance.
    /// </summary>
    public GenerativeModel model { get; }

    /// <summary>
    /// Creates a new instance using an API key and optional model name.
    /// </summary>
    public GenerativeAIImageGenerator(string apiKey, string modelName = GoogleAIModels.Gemini2FlashPreviewImageGeneration)
    {
        model = new GenerativeModel(apiKey, modelName);
    }

    /// <summary>
    /// Creates a new instance using a platform adapter and optional model name.
    /// </summary>
    public GenerativeAIImageGenerator(IPlatformAdapter adapter, string modelName = GoogleAIModels.Gemini2FlashPreviewImageGeneration)
    {
        model = new GenerativeModel(adapter, modelName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

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

        var genRequest = ToGenerateContentRequest(request, options);
        var resp = await model.GenerateContentAsync(genRequest, cancellationToken).ConfigureAwait(false);
        return ToImageGenerationResponse(resp);
    }

    // Convert the Microsoft request/options into a model-specific GenerateContentRequest
    private GenerateContentRequest ToGenerateContentRequest(ImageGenerationRequest request, ImageGenerationOptions? options)
    {
        List<Part> parts = [];
        // Add prompt text (if any)
        if (!string.IsNullOrEmpty(request.Prompt))
        {
            parts.Add(new(request.Prompt!));
        }

        // If original images provided (image edit scenario), add them as parts
        if (request.OriginalImages != null)
        {
            foreach (var aiContent in request.OriginalImages)
            {
                parts.Add(aiContent.ToPart()!);
            }
        }

        // Configure generation to request images        
        GenerationConfig generationConfig = options?.RawRepresentationFactory?.Invoke(this) as GenerationConfig ?? new();
        generationConfig.CandidateCount = options?.Count ?? 1;

        // We must request both text and image modalities to get images back
        generationConfig.ResponseModalities = new List<Modality> { Modality.TEXT, Modality.IMAGE };

        if (options != null)
        {
            if (!string.IsNullOrEmpty(options.MediaType))
                generationConfig.ResponseMimeType = options.MediaType;

            // Map requested image size (basic heuristic)
            if (options.ImageSize.HasValue)
            {
                var sz = options.ImageSize.Value;
                if (sz.Width >= 1024 || sz.Height >= 1024)
                    generationConfig.MediaResolution = MediaResolution.MEDIA_RESOLUTION_HIGH;
                else if (sz.Width >= 512 || sz.Height >= 512)
                    generationConfig.MediaResolution = MediaResolution.MEDIA_RESOLUTION_MEDIUM;
                else
                    generationConfig.MediaResolution = MediaResolution.MEDIA_RESOLUTION_LOW;
            }
        }

        return new GenerateContentRequest()
        {
            GenerationConfig = generationConfig,
            Contents = [new() { Parts = parts }]
        };
    }


    // Convert the model response to ImageGenerationResponse
    private static ImageGenerationResponse ToImageGenerationResponse(GenerateContentResponse? resp)
    {
        var aiContents = resp?.Candidates?.FirstOrDefault()?.Content?.Parts.ToAiContents();
        return new ImageGenerationResponse(aiContents) { RawRepresentation = resp };
    }
}
#pragma warning restore MEAI001
