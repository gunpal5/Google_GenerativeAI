using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Extension methods for adding image data to a <see cref="VqaRequest"/> object.
/// </summary>
public static class VqaRequestExtensions
{
    /// <summary>
    /// Adds an image to the <see cref="VqaRequest"/> from a local file with a specified prompt.
    /// </summary>
    /// <param name="request">The <see cref="VqaRequest"/> to which the image will be added.</param>
    /// <param name="prompt">The textual prompt associated with the image.</param>
    /// <param name="imagePath">The file path of the local image to be added.</param>
    /// <exception cref="ArgumentException">Thrown if the file does not exist or cannot be accessed.</exception>
    public static void AddLocalImage(this VqaRequest request, string prompt, string imagePath)
    {
        var imageMime = MimeTypeMap.GetMimeType(imagePath);
        var imageContent = Convert.ToBase64String(File.ReadAllBytes(imagePath));

        if (request.Instances == null)
            request.Instances = new List<VqaInstance>();
        request.Instances.Add(new VqaInstance()
        {
            Image = new VqaImage()
            {
                BytesBase64Encoded = imageContent,
                MimeType = imageMime,
            },
            Prompt = prompt
        });
    }

    /// <summary>
    /// Adds an image to the <see cref="VqaRequest"/> from a Google Cloud Storage (GCS) URI.
    /// </summary>
    /// <param name="request">The <see cref="VqaRequest"/> to which the image will be added.</param>
    /// <param name="prompt">The prompt or question associated with the image.</param>
    /// <param name="imageUri">The URI of the image stored in Google Cloud Storage.</param>
    public static void AddGcsImage(this VqaRequest request, string prompt, string imageUri)
    {
        var imageMime = MimeTypeMap.GetMimeType(imageUri);

        if (request.Instances == null)
            request.Instances = new List<VqaInstance>();
        request.Instances.Add(new VqaInstance()
        {
            Image = new VqaImage()
            {
                MimeType = imageMime,
                GcsUri = imageUri,
            },
            Prompt = prompt
        });
    }
}