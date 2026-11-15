using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Voice activity signal sent by the server.
/// </summary>
public class VoiceActivity
{
    /// <summary>
    /// The type of the voice activity signal.
    /// </summary>
    [JsonPropertyName("type")]
    public VoiceActivityType? VoiceActivityType { get; set; }

    /// <summary>
    /// The time voice activity was detected in audio time, relative to the start of the audio stream
    /// (a duration string such as <c>"1.5s"</c>).
    /// </summary>
    [JsonPropertyName("audioOffset")]
    public string? AudioOffset { get; set; }
}

/// <summary>
/// The type of the voice activity signal.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VoiceActivityType>))]
public enum VoiceActivityType
{
    /// <summary>
    /// Unspecified voice activity type.
    /// </summary>
    TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// Start of voice activity.
    /// </summary>
    ACTIVITY_START = 1,

    /// <summary>
    /// End of voice activity.
    /// </summary>
    ACTIVITY_END = 2
}

/// <summary>
/// Voice activity detection (VAD) signal sent by the server. Allowlisted only.
/// </summary>
public class VoiceActivityDetectionSignal
{
    /// <summary>
    /// The type of the VAD signal.
    /// </summary>
    [JsonPropertyName("vadSignalType")]
    public VadSignalType? VadSignalType { get; set; }
}

/// <summary>
/// The type of the VAD signal.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VadSignalType>))]
public enum VadSignalType
{
    /// <summary>
    /// Unspecified VAD signal type.
    /// </summary>
    VAD_SIGNAL_TYPE_UNSPECIFIED = 0,

    /// <summary>
    /// Start of sentence signal.
    /// </summary>
    VAD_SIGNAL_TYPE_SOS = 1,

    /// <summary>
    /// End of sentence signal.
    /// </summary>
    VAD_SIGNAL_TYPE_EOS = 2
}
