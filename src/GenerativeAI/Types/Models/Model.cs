using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Information about a Generative Language Model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/Model">See Official API Documentation</seealso> 
public class Model
{
    /// <summary>
    /// Required. The resource name of the <c>Model</c>.
    /// Refer to <see href="https://ai.google.dev/gemini-api/docs/models/gemini#model-variations">Model variants</see>
    /// for all allowed values.
    /// Format: <c>models/{model}</c> with a <c>{model}</c> naming convention of:
    /// - "{baseModelId}-{version}"
    /// Examples:
    /// - <c>models/gemini-1.5-flash-001</c>
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Required. The name of the base model, pass this to the generation request.
    /// Examples:
    /// - <c>gemini-1.5-flash</c>
    /// </summary>
    [JsonPropertyName("baseModelId")]
    public string BaseModelId { get; set; } = "";

    /// <summary>
    /// Required. The version number of the model.
    /// This represents the major version (<c>1.0</c> or <c>1.5</c>)
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    /// <summary>
    /// The human-readable name of the model. E.g. "Gemini 1.5 Flash".
    /// The name can be up to 128 characters long and can consist of any UTF-8 characters.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// A short description of the model.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Maximum number of input tokens allowed for this model.
    /// </summary>
    [JsonPropertyName("inputTokenLimit")]
    public int InputTokenLimit { get; set; }

    /// <summary>
    /// Maximum number of output tokens available for this model.
    /// </summary>
    [JsonPropertyName("outputTokenLimit")]
    public int OutputTokenLimit { get; set; }

    /// <summary>
    /// The model's supported generation methods.
    /// The corresponding API method names are defined as Pascal case strings, such as
    /// <c>generateMessage</c> and <c>generateContent</c>.
    /// </summary>
    [JsonPropertyName("supportedGenerationMethods")]
    public List<string>? SupportedGenerationMethods { get; set; }

    /// <summary>
    /// Controls the randomness of the output.
    /// Values can range over <c>[0.0,maxTemperature]</c>, inclusive. A higher value will produce responses
    /// that are more varied, while a value closer to <c>0.0</c> will typically result in less surprising
    /// responses from the model. This value specifies default to be used by the backend while making the
    /// call to the model.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    /// <summary>
    /// The maximum temperature this model can use.
    /// </summary>
    [JsonPropertyName("maxTemperature")]
    public double MaxTemperature { get; set; }

    /// <summary>
    /// For <see href="https://ai.google.dev/gemini-api/docs/prompting-strategies#top-p">Nucleus sampling</see>.
    /// Nucleus sampling considers the smallest set of tokens whose probability sum is at least <c>topP</c>.
    /// This value specifies default to be used by the backend while making the call to the model.
    /// </summary>
    [JsonPropertyName("topP")]
    public double TopP { get; set; }

    /// <summary>
    /// For Top-k sampling.
    /// Top-k sampling considers the set of <c>topK</c> most probable tokens. This value specifies default to be
    /// used by the backend while making the call to the model. If empty, indicates the model doesn't use top-k
    /// sampling, and <c>topK</c> isn't allowed as a generation parameter.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}