namespace GenerativeAI.Microsoft;

/// <summary>
/// Provides constant keys for additional properties used in GenerativeAI Microsoft integration.
/// </summary>
public static class AdditionalPropertiesKeys
{
    /// <summary>
    /// Key used to indicate whether to include thoughts in the response.
    /// </summary>
    public const string ThinkingConfigIncludeThoughts = "IncludeThoughts";

    /// <summary>
    /// Key used to indicate the thinking budget in tokens.
    /// </summary>
    public const string ThinkingBudget = "ThinkingBudget";

    /// <summary>
    /// Key used to specify response modalities (e.g., text, image).
    /// </summary>
    public const string ResponseModalities = "ResponseModalities";

    /// <summary>
    /// Key used to specify the aspect ratio for image generation.
    /// </summary>
    public const string ImageConfigAspectRatio = "AspectRatio";

    /// <summary>
    /// Key used to specify the output resolution for image generation ("1K", "2K", "4K").
    /// </summary>
    public const string ImageConfigImageSize = "ImageSize";

    /// <summary>
    /// Key used to specify the output MIME type for image generation ("image/png", "image/jpeg").
    /// </summary>
    public const string ImageOutputOptionsMimeType = "MimeType";

    /// <summary>
    /// Key used to specify the compression quality for image generation (0-100).
    /// </summary>
    public const string ImageOutputOptionsCompressionQuality = "CompressionQuality";
}