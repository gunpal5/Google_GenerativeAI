using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration options for model generation and outputs. Not all parameters are configurable
/// for every model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#generationconfig">See Official API Documentation</seealso>
public class GenerationConfig
{
    /// <summary>
    /// Optional. The set of character sequences (up to 5) that will stop output generation.
    /// If specified, the API will stop at the first appearance of a <c>stop_sequence</c>.
    /// The stop sequence will not be included as part of the response.
    /// </summary>
    [JsonPropertyName("stopSequences")]
    public List<string>? StopSequences { get; set; }

    /// <summary>
    /// Optional. MIME type of the generated candidate text. Supported MIME types are:
    /// <c>text/plain</c>: (default) Text output.
    /// <c>application/json</c>: JSON response in the response candidates.
    /// <c>text/x.enum</c>: ENUM as a string response in the response candidates.
    /// Refer to the <see href="https://ai.google.dev/gemini-api/docs/prompting_with_media#plain_text_formats">docs</see>
    /// for a list of all supported text MIME types.
    /// </summary>
    [JsonPropertyName("responseMimeType")]
    public string? ResponseMimeType { get; set; }

    /// <summary>
    /// Optional. Output schema of the generated candidate text. Schemas must be a subset of the
    /// <see href="https://spec.openapis.org/oas/v3.0.3#schema">OpenAPI schema</see> and can be
    /// objects, primitives or arrays.
    /// If set, a compatible <see cref="ResponseMimeType">ResponseMimeType</see> must also be set. Compatible MIME types:
    /// <c>application/json</c>: Schema for JSON response. Refer to the
    /// <see href="https://ai.google.dev/gemini-api/docs/json-mode">JSON text generation guide</see>
    /// for more details.
    /// </summary>
    [JsonPropertyName("responseSchema")]
    //[JsonConverter(typeof(ObjectToJsonSchemaConverter))]
    public Schema? ResponseSchema { get; set; }

    /// <summary>
    /// Optional. The requested modalities of the response. Represents the set of modalities
    /// that the model can return, and should be expected in the response. This is an exact
    /// match to the modalities of the response.
    /// A model may have multiple combinations of supported modalities. If the requested modalities
    /// do not match any of the supported combinations, an error will be returned.
    /// An empty list is equivalent to requesting only text.
    /// </summary>
    [JsonPropertyName("responseModalities")]
    public List<Modality>? ResponseModalities { get; set; }

    /// <summary>
    /// Optional. Number of generated responses to return.
    /// Currently, this value can only be set to 1. If unset, this will default to 1.
    /// </summary>
    [JsonPropertyName("candidateCount")]
    public int? CandidateCount { get; set; }

    /// <summary>
    /// Optional. The maximum number of tokens to include in a response candidate.
    /// Note: The default value varies by model, see the <c>Model.output_token_limit</c>
    /// attribute of the <see cref="Model">Model</see> returned from the <c>getModel</c> function.
    /// </summary>
    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Optional. Controls the randomness of the output.
    /// Note: The default value varies by model, see the <see cref="Model.Temperature">Model.Temperature</see> attribute
    /// of the <see cref="Model">Model</see> returned from the <c>getModel</c> function.
    /// Values can range from.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Optional. The maximum cumulative probability of tokens to consider when sampling.
    /// The model uses combined Top-k and Top-p (nucleus) sampling.
    /// Tokens are sorted based on their assigned probabilities so that only the most
    /// likely tokens are considered. Top-k sampling directly limits the maximum number of
    /// tokens to consider, while Nucleus sampling limits the number of tokens based on
    /// the cumulative probability.
    /// Note: The default value varies by <see cref="Model"/> and is specified by the
    /// <see cref="Model.TopP"/> attribute returned from the model information.
    /// An empty <see cref="TopK"/> attribute indicates that the model doesn't apply top-k sampling
    /// and doesn't allow setting <see cref="TopK"/> on requests.
    /// </summary>
    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    /// <summary>
    /// Optional. The maximum number of tokens to consider when sampling.
    /// Gemini models use Top-p (nucleus) sampling or a combination of Top-k and nucleus
    /// sampling. Top-k sampling considers the set of <see cref="TopK"/> most probable tokens.
    /// Models running with nucleus sampling don't allow TopK setting.
    /// Note: The default value varies by <see cref="Model"/> and is specified by the
    /// <see cref="Model.TopP"/> attribute returned from the model information.
    /// An empty <see cref="TopK"/> attribute indicates that the model doesn't apply top-k sampling
    /// and doesn't allow setting <see cref="TopK"/> on requests.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    /// <summary>
    /// Optional. Seed used in decoding. If not set, the request uses a randomly generated seed.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Optional. Presence penalty applied to the next token's logprobs if the token has
    /// already been seen in the response.
    /// This penalty is binary on/off and not dependant on the number of times the token
    /// is used (after the first). Use <see cref="FrequencyPenalty"/> for a penalty
    /// that increases with each use.
    /// A positive penalty will discourage the use of tokens that have already been used
    /// in the response, increasing the vocabulary.
    /// A negative penalty will encourage the use of tokens that have already been used
    /// in the response, decreasing the vocabulary.
    /// </summary>
    [JsonPropertyName("presencePenalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Optional. Frequency penalty applied to the next token's logprobs, multiplied by the
    /// number of times each token has been seen in the response so far.
    /// A positive penalty will discourage the use of tokens that have already been used,
    /// proportional to the number of times the token has been used: The more a token is used,
    /// the more difficult it is for the model to use that token again, increasing the
    /// vocabulary of responses.
    /// Caution: A *negative* penalty will encourage the model to reuse tokens proportional
    /// to the number of times the token has been used. Small negative values will reduce the
    /// vocabulary of a response. Larger negative values will cause the model to start repeating
    /// a common token until it hits the <see cref="MaxOutputTokens"/> limit.
    /// </summary>
    [JsonPropertyName("frequencyPenalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Optional. If true, export the logprobs results in response.
    /// </summary>
    [JsonPropertyName("responseLogprobs")]
    public bool? ResponseLogprobs { get; set; }

    /// <summary>
    /// Optional. Only valid if <see cref="ResponseLogprobs"/> is True. This sets the
    /// number of top logprobs to return at each decoding step in the
    /// <see cref="LogprobsResult"/>.
    /// </summary>
    [JsonPropertyName("logprobs")]
    public int? Logprobs { get; set; }

    /// <summary>
    /// Optional. Enables enhanced civic answers. It may not be available for all models.
    /// </summary>
    [JsonPropertyName("enableEnhancedCivicAnswers")]
    public bool? EnableEnhancedCivicAnswers { get; set; }

    /// <summary>
    /// Optional. The speech generation config.
    /// </summary>
    [JsonPropertyName("speechConfig")]
    public SpeechConfig? SpeechConfig { get; set; }
    
    /// <summary>
    /// Optional. Config for thinking features.
    /// An error will be returned if this field is set for models that don't support thinking.
    /// </summary>
    [JsonPropertyName("thinkingConfig")]
    public ThinkingConfig? ThinkingConfig { get; set; }

    /// <summary>
    /// Optional. If specified, the media resolution specified will be used.
    /// </summary>
    [JsonPropertyName("mediaResolution")]
    public MediaResolution? MediaResolution { get; set; }
    
    /// <summary>
    /// Optional. If enabled, audio timestamp will be included in the request to the model.
    /// </summary>
    [JsonPropertyName("audioTimestamp")]
    public bool? AudioTimestamp { get; set; }
    
    /// <summary>
    /// Optional. Routing configuration.
    /// </summary>
    [JsonPropertyName("routingConfig")]
    public RoutingConfig? RoutingConfig { get; set; }

    /// <summary>
    /// Optional. Config for model selection.
    /// </summary>
    [JsonPropertyName("modelSelectionConfig")]
    public ModelSelectionConfig? ModelSelectionConfig { get; set; }

    /// <summary>
    /// Optional. If enabled, the model will detect emotions and adapt its responses accordingly.
    /// </summary>
    [JsonPropertyName("enableAffectiveDialog")]
    public bool? EnableAffectiveDialog { get; set; }

    /// <summary>
    /// Optional. Output schema of the generated response. This is an alternative to responseSchema
    /// that accepts JSON Schema. If set, responseSchema must be omitted, but responseMimeType is required.
    /// </summary>
    [JsonPropertyName("responseJsonSchema")]
    public JsonNode? ResponseJsonSchema { get; set; }
}


/// <summary>
/// Config for thinking features.
/// </summary>
public class ThinkingConfig
{
    /// <summary>
    /// Indicates whether to include thoughts in the response. If true, thoughts are returned only if the model supports thought and thoughts are available.
    /// </summary>
    [JsonPropertyName("includeThoughts")]
    public bool? IncludeThoughts { get; set; }
    
    /// <summary>
    /// Indicates the thinking budget in tokens
    /// </summary>
    [JsonPropertyName("thinkingBudget")]
    public int? ThinkingBudget { get; set; }
}

/// <summary>
/// Media resolution for the input media.
/// </summary>
public enum MediaResolution
{
    /// <summary>
    /// The media resolution is unspecified.
    /// </summary>
    MEDIA_RESOLUTION_UNSPECIFIED,
    /// <summary>
    /// Low media resolution.
    /// </summary>
    MEDIA_RESOLUTION_LOW,
    /// <summary>
    /// Medium media resolution.
    /// </summary>
    MEDIA_RESOLUTION_MEDIUM,
    /// <summary>
    /// High media resolution.
    /// </summary>
    MEDIA_RESOLUTION_HIGH
}


/// <summary>
/// The configuration for routing the request to a specific model.
/// </summary>
public class RoutingConfig
{
    /// <summary>
    /// Automated routing.
    /// </summary>
    [JsonPropertyName("autoMode")]
    public AutoRoutingMode? AutoMode { get; set; }

    /// <summary>
    /// Manual routing.
    /// </summary>
    [JsonPropertyName("manualMode")]
    public ManualRoutingMode? ManualMode { get; set; }
}

/// <summary>
/// When automated routing is specified, the routing will be determined by the pretrained routing model 
/// and customer provided model routing preference.
/// </summary>
public class AutoRoutingMode
{
    /// <summary>
    /// The model routing preference.
    /// </summary>
    [JsonPropertyName("modelRoutingPreference")]
    public ModelRoutingPreference? ModelRoutingPreference { get; set; }
}

/// <summary>
/// Model routing preference options.
/// </summary>
public enum ModelRoutingPreference
{
    /// <summary>
    /// Unknown preference.
    /// </summary>
    UNKNOWN,

    /// <summary>
    /// Prioritize quality over cost.
    /// </summary>
    PRIORITIZE_QUALITY,

    /// <summary>
    /// Balanced between quality and cost.
    /// </summary>
    BALANCED,

    /// <summary>
    /// Prioritize cost over quality.
    /// </summary>
    PRIORITIZE_COST
}

/// <summary>
/// When manual routing is set, the specified model will be used directly.
/// </summary>
public class ManualRoutingMode
{
    /// <summary>
    /// The model name to use. Only the public LLM models are accepted. e.g. 'gemini-1.5-pro-001'.
    /// </summary>
    [JsonPropertyName("modelName")]
    public string? ModelName { get; set; }
}