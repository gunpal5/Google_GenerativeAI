using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Extension methods for configuring <see cref="ImageCaptioningRequest"/> objects
/// with image data.
/// </summary>
public static class ImageCaptioningRequestExtensions
{
    /// <summary>
    /// Adds a local image to the <see cref="ImageCaptioningRequest"/> by reading the image
    /// from the specified file path and encoding it as a base64 string.
    /// </summary>
    /// <param name="request">The <see cref="ImageCaptioningRequest"/> to add the image to.</param>
    /// <param name="imagePath">The file path of the local image to add.</param>
    public static void AddLocalImage(this ImageCaptioningRequest request, string imagePath)
    {
        var mimeType = MimeTypeMap.GetMimeType(imagePath);
        var getBytes = Convert.ToBase64String(File.ReadAllBytes(imagePath));

        if (request.Instances == null)
            request.Instances = new List<ImageInstance>();
        request.Instances.Add(new ImageInstance()
        {
            Image = new ImageData()
            {
                BytesBase64Encoded = getBytes,
                MimeType = mimeType,
            }
        });
    }

    /// <summary>
    /// Adds a Google Cloud Storage (GCS) image to the <see cref="ImageCaptioningRequest"/> 
    /// using the specified GCS URI.
    /// </summary>
    /// <param name="request">The <see cref="ImageCaptioningRequest"/> to add the image to.</param>
    /// <param name="imageUri">The GCS URI of the image to add.</param>
    public static void AddGcsImage(this ImageCaptioningRequest request, string imageUri)
    {
        var mimeType = MimeTypeMap.GetMimeType(imageUri);

        if (request.Instances == null)
            request.Instances = new List<ImageInstance>();
        request.Instances.Add(new ImageInstance()
        {
            Image = new ImageData()
            {
                MimeType = mimeType,
                GcsUri = imageUri,
            }
        });
    }
}