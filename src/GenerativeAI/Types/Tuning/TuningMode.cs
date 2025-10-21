using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning mode enumeration.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TuningMode>))]
public enum TuningMode
{
    /// <summary>
    /// Tuning mode is unspecified.
    /// </summary>
    TUNING_MODE_UNSPECIFIED = 0,

    /// <summary>
    /// Full fine-tuning mode. All model parameters are updated.
    /// </summary>
    TUNING_MODE_FULL = 1,

    /// <summary>
    /// PEFT (Parameter-Efficient Fine-Tuning) adapter tuning mode.
    /// Only adapter parameters are updated while base model remains frozen.
    /// </summary>
    TUNING_MODE_PEFT_ADAPTER = 2
}
