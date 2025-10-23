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
}