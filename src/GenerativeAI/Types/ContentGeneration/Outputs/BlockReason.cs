using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Specifies the reason why the prompt was blocked.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#BlockReason">BlockReason Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BlockReason
{
    /// <summary>
    /// Default value. This value is unused.
    /// </summary>
    BLOCK_REASON_UNSPECIFIED,

    /// <summary>
    /// Prompt was blocked due to safety reasons. Inspect <see cref="PromptFeedback.SafetyRatings">SafetyRatings</see> to understand which safety category blocked it.
    /// </summary>
    SAFETY,

    /// <summary>
    /// Prompt was blocked due to unknown reasons.
    /// </summary>
    OTHER,

    /// <summary>
    /// Prompt was blocked due to the terms which are included from the terminology blocklist.
    /// </summary>
    BLOCKLIST,

    /// <summary>
    /// Prompt was blocked due to prohibited content.
    /// </summary>
    PROHIBITED_CONTENT,

    /// <summary>
    /// Candidates blocked due to unsafe image generation content.
    /// </summary>
    IMAGE_SAFETY
}