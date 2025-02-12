using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The state of the tuned model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tuning#State">See Official API Documentation</seealso>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TuningState
{
    /// <summary>
    /// The default value. This value is unused.
    /// </summary>
    STATE_UNSPECIFIED,
    /// <summary>
    /// The model is being created.
    /// </summary>
    CREATING,
    /// <summary>
    /// The model is ready to be used.
    /// </summary>
    ACTIVE,
    /// <summary>
    /// The model failed to be created.
    /// </summary>
    FAILED
}