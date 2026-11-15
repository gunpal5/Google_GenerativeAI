using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Lifecycle status of the model that served a request, including deprecation and retirement information.
/// </summary>
public class ModelStatus
{
    /// <summary>
    /// A human-readable message about the model's status.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// The model's lifecycle stage.
    /// </summary>
    [JsonPropertyName("modelStage")]
    public ModelStage? ModelStage { get; set; }

    /// <summary>
    /// When the model will be (or was) retired.
    /// </summary>
    [JsonPropertyName("retirementTime")]
    public Timestamp? RetirementTime { get; set; }
}

/// <summary>
/// Lifecycle stage of a model.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ModelStage>))]
public enum ModelStage
{
    /// <summary>
    /// The stage is unspecified.
    /// </summary>
    MODEL_STAGE_UNSPECIFIED,

    /// <summary>
    /// Unstable experimental model.
    /// </summary>
    UNSTABLE_EXPERIMENTAL,

    /// <summary>
    /// Experimental model.
    /// </summary>
    EXPERIMENTAL,

    /// <summary>
    /// Preview model.
    /// </summary>
    PREVIEW,

    /// <summary>
    /// Stable (generally available) model.
    /// </summary>
    STABLE,

    /// <summary>
    /// Legacy model that is still available but superseded.
    /// </summary>
    LEGACY,

    /// <summary>
    /// Deprecated model that is scheduled for retirement.
    /// </summary>
    DEPRECATED,

    /// <summary>
    /// Retired model.
    /// </summary>
    RETIRED
}
