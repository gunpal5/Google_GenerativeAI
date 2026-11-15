using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Media resolution for a single part. Overrides <see cref="GenerationConfig.MediaResolution"/> for that part.
/// </summary>
public class PartMediaResolution
{
    /// <summary>
    /// The resolution level to use for this part's media.
    /// </summary>
    [JsonPropertyName("level")]
    public PartMediaResolutionLevel? Level { get; set; }

    /// <summary>
    /// The number of tokens to use for this part's media, as an alternative to <see cref="Level"/>.
    /// </summary>
    [JsonPropertyName("numTokens")]
    public int? NumTokens { get; set; }
}

/// <summary>
/// Resolution level for the media in a single part.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PartMediaResolutionLevel>))]
public enum PartMediaResolutionLevel
{
    /// <summary>
    /// The media resolution is unspecified.
    /// </summary>
    MEDIA_RESOLUTION_UNSPECIFIED,

    /// <summary>
    /// Low media resolution.
    /// </summary>
    MEDIA_RESOLUTION_LOW,

    /// <summary>
    /// Medium media resolution.
    /// </summary>
    MEDIA_RESOLUTION_MEDIUM,

    /// <summary>
    /// High media resolution.
    /// </summary>
    MEDIA_RESOLUTION_HIGH,

    /// <summary>
    /// Ultra-high media resolution. Only valid on individual parts.
    /// </summary>
    MEDIA_RESOLUTION_ULTRA_HIGH
}

/// <summary>
/// How the model processes the media in a part.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MediaProcessing>))]
public enum MediaProcessing
{
    /// <summary>
    /// Media processing is unspecified.
    /// </summary>
    MEDIA_PROCESSING_UNSPECIFIED,

    /// <summary>
    /// The media is processed once as static input.
    /// </summary>
    STATIC,

    /// <summary>
    /// The model can actively inspect the media (for example zooming or seeking) while reasoning.
    /// </summary>
    AGENTIC
}

/// <summary>
/// Speaker and style metadata for speech generated in a part.
/// </summary>
public class SpeechMetadata
{
    /// <summary>
    /// The speaker of the speech.
    /// </summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>
    /// The speaking style.
    /// </summary>
    [JsonPropertyName("style")]
    public string? Style { get; set; }
}
