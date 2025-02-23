using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for the <see cref="GenerateImageRequest"/> class.
/// </summary>
public static class GenerateImageRequestExtensions
{
    /// <summary>
    /// Adds a prompt and optional image source to the <see cref="GenerateImageRequest"/>.
    /// </summary>
    /// <param name="request">The <see cref="GenerateImageRequest"/> to modify.</param>
    /// <param name="prompt">The text prompt for image generation.</param>
    /// <param name="source">The optional image source to use as additional context.</param>
    public static void AddPrompt(this GenerateImageRequest request, string prompt, ImageSource? source = null)
    {
        if (request.Instances == null)
        {
            request.Instances = new List<ImageGenerationInstance>();
        }

        request.Instances.Add(new ImageGenerationInstance()
        {
            Prompt = prompt,
            Image = source
        });
    }

    /// <summary>
    /// Adds image generation parameters to the <see cref="GenerateImageRequest"/>.
    /// </summary>
    /// <param name="request">The <see cref="GenerateImageRequest"/> to modify.</param>
    /// <param name="parameters">The image generation parameters to set.</param>
    public static void AddParameters(this GenerateImageRequest request, ImageGenerationParameters? parameters)
    {
        request.Parameters = parameters;
    }
}