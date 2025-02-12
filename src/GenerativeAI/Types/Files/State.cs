using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// States for the lifecycle of a File.
/// </summary>
/// <seealso href="https://ai.google.dev/api/files#State">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileState
{
    /// <summary>
    /// The default value. This value is used if the state is omitted.
    /// </summary>
    STATE_UNSPECIFIED = 0,

    /// <summary>
    /// File is being processed and cannot be used for inference yet.
    /// </summary>
    PROCESSING = 1,

    /// <summary>
    /// File is processed and available for inference.
    /// </summary>
    ACTIVE = 2,

    /// <summary>
    /// File failed processing.
    /// </summary>
    FAILED = 3,
}
