using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Content Part modality.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#Modality">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Modality
{
    /// <summary>
    /// Unspecified modality.
    /// </summary>
    MODALITY_UNSPECIFIED = 0,

    /// <summary>
    /// Plain text.
    /// </summary>
    TEXT = 1,

    /// <summary>
    /// Image.
    /// </summary>
    IMAGE = 2,

    /// <summary>
    /// Video.
    /// </summary>
    VIDEO = 3,

    /// <summary>
    /// Audio.
    /// </summary>
    AUDIO = 4,

    /// <summary>
    /// Document, e.g. PDF.
    /// </summary>
    DOCUMENT = 5,
}