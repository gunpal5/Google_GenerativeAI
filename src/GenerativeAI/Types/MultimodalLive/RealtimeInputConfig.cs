using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configures the realtime input behavior in <see cref="BidiGenerateContentSetup.RealtimeInputConfig"/>.
/// </summary>
public class RealtimeInputConfig
{
    /// <summary>
    /// If not set, automatic activity detection is enabled by default. If automatic voice detection is disabled,
    /// the client must send activity signals (<see cref="BidiGenerateContentRealtimeInput.ActivityStart"/> and
    /// <see cref="BidiGenerateContentRealtimeInput.ActivityEnd"/>).
    /// </summary>
    [JsonPropertyName("automaticActivityDetection")]
    public AutomaticActivityDetection? AutomaticActivityDetection { get; set; }

    /// <summary>
    /// Defines what effect activity has.
    /// </summary>
    [JsonPropertyName("activityHandling")]
    public ActivityHandling? ActivityHandling { get; set; }

    /// <summary>
    /// Defines which input is included in the user's turn.
    /// </summary>
    [JsonPropertyName("turnCoverage")]
    public TurnCoverage? TurnCoverage { get; set; }

    /// <summary>
    /// If true, enables interim transcript timestamps.
    /// </summary>
    [JsonPropertyName("interimTranscriptTimestampEnabled")]
    public bool? InterimTranscriptTimestampEnabled { get; set; }
}

/// <summary>
/// Configures automatic detection of activity.
/// </summary>
public class AutomaticActivityDetection
{
    /// <summary>
    /// If enabled, detected voice and text input count as activity. If disabled, the client must send activity signals.
    /// </summary>
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    /// Determines how likely speech is to be detected.
    /// </summary>
    [JsonPropertyName("startOfSpeechSensitivity")]
    public StartSensitivity? StartOfSpeechSensitivity { get; set; }

    /// <summary>
    /// Determines how likely detected speech is ended.
    /// </summary>
    [JsonPropertyName("endOfSpeechSensitivity")]
    public EndSensitivity? EndOfSpeechSensitivity { get; set; }

    /// <summary>
    /// The required duration of detected speech (in milliseconds) before start-of-speech is committed.
    /// The lower this value the more sensitive the start-of-speech detection is and the shorter speech can be
    /// recognized. However, this also increases the probability of false positives.
    /// </summary>
    [JsonPropertyName("prefixPaddingMs")]
    public int? PrefixPaddingMs { get; set; }

    /// <summary>
    /// The required duration of detected non-speech (e.g. silence, in milliseconds) before end-of-speech is committed.
    /// The larger this value, the longer speech gaps can be without interrupting the user's activity but this will
    /// increase the model's latency.
    /// </summary>
    [JsonPropertyName("silenceDurationMs")]
    public int? SilenceDurationMs { get; set; }
}

/// <summary>
/// Start of speech sensitivity.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<StartSensitivity>))]
public enum StartSensitivity
{
    /// <summary>
    /// The default is <see cref="START_SENSITIVITY_HIGH"/> for the Gemini API and <see cref="START_SENSITIVITY_LOW"/> for Vertex AI.
    /// </summary>
    START_SENSITIVITY_UNSPECIFIED = 0,

    /// <summary>
    /// Automatic detection will detect the start of speech more often.
    /// </summary>
    START_SENSITIVITY_HIGH = 1,

    /// <summary>
    /// Automatic detection will detect the start of speech less often.
    /// </summary>
    START_SENSITIVITY_LOW = 2
}

/// <summary>
/// End of speech sensitivity.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EndSensitivity>))]
public enum EndSensitivity
{
    /// <summary>
    /// The default is <see cref="END_SENSITIVITY_HIGH"/> for the Gemini API and <see cref="END_SENSITIVITY_LOW"/> for Vertex AI.
    /// </summary>
    END_SENSITIVITY_UNSPECIFIED = 0,

    /// <summary>
    /// Automatic detection ends speech more often.
    /// </summary>
    END_SENSITIVITY_HIGH = 1,

    /// <summary>
    /// Automatic detection ends speech less often.
    /// </summary>
    END_SENSITIVITY_LOW = 2
}

/// <summary>
/// The different ways of handling user activity.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ActivityHandling>))]
public enum ActivityHandling
{
    /// <summary>
    /// If unspecified, the default behavior is <see cref="START_OF_ACTIVITY_INTERRUPTS"/>.
    /// </summary>
    ACTIVITY_HANDLING_UNSPECIFIED = 0,

    /// <summary>
    /// Start of activity will interrupt the model's response (also called "barge in"). The model's current
    /// response will be cut-off in the moment of the interruption. This is the default behavior.
    /// </summary>
    START_OF_ACTIVITY_INTERRUPTS = 1,

    /// <summary>
    /// The model's response will not be interrupted.
    /// </summary>
    NO_INTERRUPTION = 2
}

/// <summary>
/// Options about which input is included in the user's turn.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TurnCoverage>))]
public enum TurnCoverage
{
    /// <summary>
    /// If unspecified, the default behavior is <see cref="TURN_INCLUDES_ONLY_ACTIVITY"/>.
    /// </summary>
    TURN_COVERAGE_UNSPECIFIED = 0,

    /// <summary>
    /// The user's turn only includes activity since the last turn, excluding inactivity (e.g. silence on the audio stream).
    /// This is the default behavior.
    /// </summary>
    TURN_INCLUDES_ONLY_ACTIVITY = 1,

    /// <summary>
    /// The user's turn includes all realtime input since the last turn, including inactivity (e.g. silence on the audio stream).
    /// </summary>
    TURN_INCLUDES_ALL_INPUT = 2,

    /// <summary>
    /// Includes audio activity and all video since the last turn. With automatic activity detection,
    /// audio activity means speech and excludes silence.
    /// </summary>
    TURN_INCLUDES_AUDIO_ACTIVITY_AND_ALL_VIDEO = 3
}
