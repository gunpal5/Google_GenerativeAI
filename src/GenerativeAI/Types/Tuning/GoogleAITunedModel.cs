using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A tuned model resource of the Gemini Developer API (<c>tunedModels/{id}</c>).
/// </summary>
/// <remarks>
/// <see cref="GenerativeAI.Clients.TuningClient"/> converts it to a <see cref="TuningJob"/> with
/// <see cref="ToTuningJob"/>, as the google-genai SDK does.
/// </remarks>
/// <seealso href="https://ai.google.dev/api/tuning#TunedModel">See Official API Documentation</seealso>
public class GoogleAITunedModel
{
    /// <summary>
    /// Output only. The tuned model name, e.g. <c>tunedModels/my-model-id</c>.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. Tuned model as a source for training a new model.
    /// </summary>
    [JsonPropertyName("tunedModelSource")]
    public TunedModelSource? TunedModelSource { get; set; }

    /// <summary>
    /// Immutable. The name of the model to tune, e.g. <c>models/gemini-1.5-flash-001</c>.
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string? BaseModel { get; set; }

    /// <summary>
    /// Optional. The display name of the tuned model.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional. A short description of the tuned model.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Optional. Default sampling temperature of the tuned model.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Optional. Default nucleus sampling value of the tuned model.
    /// </summary>
    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    /// <summary>
    /// Optional. Default top-k sampling value of the tuned model.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    /// <summary>
    /// Output only. The state of the tuned model.
    /// </summary>
    [JsonPropertyName("state")]
    public TuningState? State { get; set; }

    /// <summary>
    /// Output only. The timestamp when this model was created.
    /// </summary>
    [JsonPropertyName("createTime")]
    public Timestamp? CreateTime { get; set; }

    /// <summary>
    /// Output only. The timestamp when this model was updated.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public Timestamp? UpdateTime { get; set; }

    /// <summary>
    /// Required. The tuning task that creates the tuned model.
    /// </summary>
    [JsonPropertyName("tuningTask")]
    public TuningTask? TuningTask { get; set; }

    /// <summary>
    /// Optional. List of project numbers that have read access to the tuned model.
    /// </summary>
    [JsonPropertyName("readerProjectNumbers")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public List<long>? ReaderProjectNumbers { get; set; }

    /// <summary>
    /// Converts this Gemini API tuned model into a <see cref="TuningJob"/> the same way the google-genai SDK does
    /// (<c>CREATING</c> maps to <see cref="JobState.JOB_STATE_RUNNING"/>, <c>ACTIVE</c> to
    /// <see cref="JobState.JOB_STATE_SUCCEEDED"/> and <c>FAILED</c> to <see cref="JobState.JOB_STATE_FAILED"/>).
    /// </summary>
    /// <returns>The equivalent <see cref="TuningJob"/>.</returns>
    public TuningJob ToTuningJob()
    {
        return new TuningJob
        {
            Name = Name,
            State = State switch
            {
                TuningState.CREATING => JobState.JOB_STATE_RUNNING,
                TuningState.ACTIVE => JobState.JOB_STATE_SUCCEEDED,
                TuningState.FAILED => JobState.JOB_STATE_FAILED,
                TuningState.STATE_UNSPECIFIED => JobState.JOB_STATE_UNSPECIFIED,
                _ => null
            },
            CreateTime = CreateTime,
            StartTime = TuningTask?.StartTime,
            EndTime = TuningTask?.CompleteTime,
            UpdateTime = UpdateTime,
            Description = Description,
            BaseModel = BaseModel,
            TunedModelDisplayName = DisplayName,
            TunedModel = Name == null ? null : new TunedModel { Model = Name, Endpoint = Name }
        };
    }
}
