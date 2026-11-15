using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The tuning method used by a tuning job.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TuningMethod>))]
public enum TuningMethod
{
    /// <summary>Supervised fine tuning (the default).</summary>
    SUPERVISED_FINE_TUNING,

    /// <summary>Preference optimization tuning.</summary>
    PREFERENCE_TUNING,

    /// <summary>Distillation tuning.</summary>
    DISTILLATION,

    /// <summary>Reinforcement tuning.</summary>
    REINFORCEMENT_TUNING
}

/// <summary>
/// The detailed state of a tuning job while the overall <see cref="JobState"/> is running.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TuningJobState>))]
public enum TuningJobState
{
    /// <summary>Default value.</summary>
    TUNING_JOB_STATE_UNSPECIFIED,

    /// <summary>The tuning job is waiting for job quota.</summary>
    TUNING_JOB_STATE_WAITING_FOR_QUOTA,

    /// <summary>The tuning job is validating the dataset.</summary>
    TUNING_JOB_STATE_PROCESSING_DATASET,

    /// <summary>The tuning job is waiting for hardware capacity.</summary>
    TUNING_JOB_STATE_WAITING_FOR_CAPACITY,

    /// <summary>The tuning job is running.</summary>
    TUNING_JOB_STATE_TUNING,

    /// <summary>The tuning job is doing some post processing steps.</summary>
    TUNING_JOB_STATE_POST_PROCESSING
}

/// <summary>
/// The maximum thinking depth used during reinforcement tuning.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ReinforcementTuningThinkingLevel>))]
public enum ReinforcementTuningThinkingLevel
{
    /// <summary>Unspecified thinking level.</summary>
    REINFORCEMENT_TUNING_THINKING_LEVEL_UNSPECIFIED,

    /// <summary>Minimal thinking.</summary>
    MINIMAL,

    /// <summary>Low thinking.</summary>
    LOW,

    /// <summary>Medium thinking.</summary>
    MEDIUM,

    /// <summary>High thinking.</summary>
    HIGH
}

/// <summary>
/// Defines how a sample response is parsed before scoring in reinforcement tuning.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ResponseParseType>))]
public enum ResponseParseType
{
    /// <summary>Unspecified parse type.</summary>
    RESPONSE_PARSE_TYPE_UNSPECIFIED,

    /// <summary>Use the response as is.</summary>
    IDENTITY,

    /// <summary>Extract part of the response with a regular expression.</summary>
    REGEX_EXTRACT
}

/// <summary>
/// The match operation used by a reinforcement tuning string match expression.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MatchOperation>))]
public enum MatchOperation
{
    /// <summary>Unspecified match operation.</summary>
    MATCH_OPERATION_UNSPECIFIED,

    /// <summary>The target matches the regular expression.</summary>
    REGEX_CONTAINS,

    /// <summary>The target contains the expression.</summary>
    PARTIAL_MATCH,

    /// <summary>The target equals the expression.</summary>
    EXACT_MATCH
}

/// <summary>
/// The tuning task of a Veo tuning job.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VeoTuningTask>))]
public enum VeoTuningTask
{
    /// <summary>Default value.</summary>
    TUNING_TASK_UNSPECIFIED,

    /// <summary>Image to video.</summary>
    TUNING_TASK_I2V,

    /// <summary>Text to video.</summary>
    TUNING_TASK_T2V,

    /// <summary>Reference to video.</summary>
    TUNING_TASK_R2V
}

/// <summary>
/// The speed of a Veo tuning job.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TuningSpeed>))]
public enum TuningSpeed
{
    /// <summary>Default value.</summary>
    TUNING_SPEED_UNSPECIFIED,

    /// <summary>Regular speed.</summary>
    REGULAR,

    /// <summary>Fast speed.</summary>
    FAST
}

/// <summary>
/// The orientation of the videos produced by a Veo LoRA tuned model.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VideoOrientation>))]
public enum VideoOrientation
{
    /// <summary>Default value.</summary>
    VIDEO_ORIENTATION_UNSPECIFIED,

    /// <summary>Landscape orientation.</summary>
    LANDSCAPE,

    /// <summary>Portrait orientation.</summary>
    PORTRAIT
}
