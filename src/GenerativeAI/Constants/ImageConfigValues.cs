namespace GenerativeAI;

/// <summary>
/// Provides common values for image generation configuration.
/// These constants are provided for convenience and discoverability but are not restrictive;
/// any valid string value accepted by the API can be used.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/image-generation">See Official API Documentation</seealso>
public static class ImageConfigValues
{
    /// <summary>
    /// Common aspect ratios for image generation.
    /// </summary>
    public static class AspectRatios
    {
        /// <summary>Square aspect ratio (1:1).</summary>
        public const string Square = "1:1";

        /// <summary>Landscape aspect ratio (16:9), common for widescreen displays.</summary>
        public const string Landscape16x9 = "16:9";

        /// <summary>Portrait aspect ratio (9:16), common for mobile/vertical displays.</summary>
        public const string Portrait9x16 = "9:16";

        /// <summary>Landscape aspect ratio (4:3), traditional display format.</summary>
        public const string Landscape4x3 = "4:3";

        /// <summary>Portrait aspect ratio (3:4).</summary>
        public const string Portrait3x4 = "3:4";
    }

    /// <summary>
    /// Output resolutions for image generation.
    /// Only supported on certain models like gemini-3-pro-image-preview.
    /// </summary>
    public static class ImageSizes
    {
        /// <summary>1K resolution output.</summary>
        public const string Size1K = "1K";

        /// <summary>2K resolution output.</summary>
        public const string Size2K = "2K";

        /// <summary>4K resolution output.</summary>
        public const string Size4K = "4K";
    }

    /// <summary>
    /// Output MIME types for generated images.
    /// </summary>
    public static class OutputMimeTypes
    {
        /// <summary>PNG format (lossless compression). This is the default.</summary>
        public const string Png = "image/png";

        /// <summary>JPEG format (lossy compression, supports compression quality setting).</summary>
        public const string Jpeg = "image/jpeg";
    }
}
