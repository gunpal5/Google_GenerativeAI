using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A fine-tuned model created using <see cref="ModelService"/>.CreateTunedModel.
/// </summary>
/// <seealso href="https://developers.generativeai.google/api/tuning#TunedModel">See Official API Documentation</seealso>
public class TunedModel
{
    /// <summary>
    /// Output only. The tuned model name. A unique name will be generated on create.
    /// Example: <c>tunedModels/az2mb0bpw6i</c>
    /// If <see cref="DisplayName"/> is set on create, the id portion of the name will be set by concatenating the words of the <see cref="DisplayName"/> with hyphens and adding a random portion for uniqueness.
    /// 
    /// Example:
    /// - displayName = <c>Sentence Translator</c>
    /// - name = <c>tunedModels/sentence-translator-u3b7m</c>
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Optional. The name to display for this model in user interfaces.
    /// The display name must be up to 40 characters including spaces.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional. A short description of this model.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Output only. The state of the tuned model.
    /// </summary>
    [JsonPropertyName("state")]
    public TuningState? State { get; set; }

    /// <summary>
    /// Output only. The timestamp when this model was created.
    /// 
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits.
    /// Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    /// <summary>
    /// Output only. The timestamp when this model was updated.
    /// 
    /// Uses RFC 3339, where generated output will always be Z-normalized and uses 0, 3, 6 or 9 fractional digits.
    /// Offsets other than "Z" are also accepted.
    /// Examples: <c>"2014-10-02T15:01:23Z"</c>, <c>"2014-10-02T15:01:23.045123456Z"</c> or <c>"2014-10-02T15:01:23+05:30"</c>.
    /// </summary>
    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    /// <summary>
    /// Required. The tuning task that creates the tuned model.
    /// </summary>
    [JsonPropertyName("tuningTask")]
    public TuningTask? TuningTask { get; set; }

    /// <summary>
    /// Optional. List of project numbers that have read access to the tuned model.
    /// </summary>
    [JsonPropertyName("readerProjectNumbers")]
    public string[]? ReaderProjectNumbers { get; set; }

    /// <summary>
    /// Optional. <see cref="TunedModelSource"/> to use as the starting point for training the new model.
    /// </summary>
    [JsonPropertyName("tunedModelSource")]
    public TunedModelSource? TunedModelSource { get; set; }

    /// <summary>
    /// Immutable. The name of the <see cref="Model"/> to tune.
    /// Example: <c>models/gemini-1.5-flash-001</c>
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string? BaseModel { get; set; }

    /// <summary>
    /// Optional. Controls the randomness of the output.
    /// 
    /// Values can range over <c>[0.0,1.0]</c>, inclusive.
    /// A value closer to <c>1.0</c> will produce responses that are more varied, while a value closer to <c>0.0</c> will typically result in less surprising responses from the model.
    /// 
    /// This value specifies default to be the one used by the base model while creating the model.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Optional. For Nucleus sampling.
    /// 
    /// Nucleus sampling considers the smallest set of tokens whose probability sum is at least <see cref="TopP"/>.
    /// 
    /// This value specifies default to be the one used by the base model while creating the model.
    /// </summary>
    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    /// <summary>
    /// Optional. For Top-k sampling.
    /// 
    /// Top-k sampling considers the set of <see cref="TopK"/> most probable tokens.
    /// This value specifies default to be used by the backend while making the call to the model.
    /// 
    /// This value specifies default to be the one used by the base model while creating the model.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}