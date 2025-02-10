using System.Text.Json.Serialization;

namespace GenerativeAI.Types.ContentGeneration.Config;

/// <summary>
/// Supported modalities of the response.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#Modality">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Modality
{
    /// <summary>
    /// Default value.
    /// </summary>
    MODALITY_UNSPECIFIED = 0,

    /// <summary>
    /// Indicates the model should return text.
    /// </summary>
    TEXT = 1,

    /// <summary>
    /// Indicates the model should return images.
    /// </summary>
    IMAGE = 2,

    /// <summary>
    /// Indicates the model should return audio.
    /// </summary>
    AUDIO = 3,
}