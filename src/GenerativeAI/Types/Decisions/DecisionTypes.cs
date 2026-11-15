using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Request body of <c>POST /decisions</c>: evaluates one or more structured decision questions (NOUL, choice or
/// score) against a shared <see cref="Input"/>.
/// </summary>
public sealed class CreateDecisionRequest
{
    /// <summary>
    /// Required. The model used to evaluate the decisions (e.g. <see cref="DecisionModels.GeminiDecision1"/>).
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Required. Shared input prefix evaluated once across all questions: a string, a content block (text, image,
    /// audio, video or document) or a list of content blocks.
    /// </summary>
    [JsonPropertyName("input")]
    public InteractionInput? Input { get; set; }

    /// <summary>
    /// Required. The questions to evaluate, keyed by caller-defined unique question identifiers (at least one).
    /// </summary>
    [JsonPropertyName("questions")]
    public Dictionary<string, DecisionQuestion>? Questions { get; set; }

    /// <summary>Additional body fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>
/// A decision question, discriminated by <c>type</c> (see <see cref="DecisionQuestionTypes"/>).
/// Unknown types are read as <see cref="UnknownDecisionQuestion"/>.
/// </summary>
[JsonConverter(typeof(DecisionQuestionConverter))]
public abstract class DecisionQuestion
{
    /// <summary>
    /// Initializes a new question with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected DecisionQuestion(string type)
    {
        Type = type;
    }

    /// <summary>The question type (see <see cref="DecisionQuestionTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>
    /// Required. Question-specific instructions: a string, a text content block or a list of text content blocks.
    /// </summary>
    [JsonPropertyName("instructions")]
    public InteractionInput? Instructions { get; set; }

    /// <summary>Fields this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>A yes/no ("NOUL") question (<c>noul</c>), answered with a probability.</summary>
public sealed class NoulDecisionQuestion : DecisionQuestion
{
    /// <summary>Initializes a new instance of the <see cref="NoulDecisionQuestion"/> class.</summary>
    public NoulDecisionQuestion() : base(DecisionQuestionTypes.Noul)
    {
    }

    /// <summary>Optional descriptions of the true and false outcomes.</summary>
    [JsonPropertyName("criteria")]
    public NoulCriteria? Criteria { get; set; }
}

/// <summary>Descriptions of the outcomes of a <see cref="NoulDecisionQuestion"/>.</summary>
public sealed class NoulCriteria
{
    /// <summary>Description of the true outcome.</summary>
    [JsonPropertyName("true_description")]
    public string? TrueDescription { get; set; }

    /// <summary>Description of the false outcome.</summary>
    [JsonPropertyName("false_description")]
    public string? FalseDescription { get; set; }
}

/// <summary>A multiple-choice question (<c>choice</c>).</summary>
public sealed class ChoiceDecisionQuestion : DecisionQuestion
{
    /// <summary>Initializes a new instance of the <see cref="ChoiceDecisionQuestion"/> class.</summary>
    public ChoiceDecisionQuestion() : base(DecisionQuestionTypes.Choice)
    {
    }

    /// <summary>Required. The choices, keyed by choice identifier, with their descriptions.</summary>
    [JsonPropertyName("criteria")]
    public Dictionary<string, string>? Criteria { get; set; }
}

/// <summary>A scoring question (<c>score</c>).</summary>
public sealed class ScoreDecisionQuestion : DecisionQuestion
{
    /// <summary>Initializes a new instance of the <see cref="ScoreDecisionQuestion"/> class.</summary>
    public ScoreDecisionQuestion() : base(DecisionQuestionTypes.Score)
    {
    }

    /// <summary>Required. The ordered scoring criteria (one per score level).</summary>
    [JsonPropertyName("criteria")]
    public List<string>? Criteria { get; set; }
}

/// <summary>A question whose <c>type</c> this SDK does not know; the raw JSON is kept.</summary>
public sealed class UnknownDecisionQuestion : DecisionQuestion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownDecisionQuestion"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownDecisionQuestion(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the question.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>Response of <c>POST /decisions</c>.</summary>
public sealed class CreateDecisionResponse
{
    /// <summary>The answers, keyed by the question identifiers of the request.</summary>
    [JsonPropertyName("answers")]
    public Dictionary<string, DecisionAnswer>? Answers { get; set; }

    /// <summary>Feedback on the input, e.g. when it was blocked.</summary>
    [JsonPropertyName("prompt_feedback")]
    public DecisionPromptFeedback? PromptFeedback { get; set; }

    /// <summary>Token usage.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Feedback on the input of a decision request.</summary>
public sealed class DecisionPromptFeedback
{
    /// <summary>Why the input was blocked (see <see cref="DecisionBlockReasons"/>).</summary>
    [JsonPropertyName("block_reason")]
    public string? BlockReason { get; set; }

    /// <summary>A human-readable description of the block reason.</summary>
    [JsonPropertyName("block_reason_message")]
    public string? BlockReasonMessage { get; set; }
}

/// <summary>
/// The answer to a decision question, discriminated by <c>type</c>. Unknown types are read as
/// <see cref="UnknownDecisionAnswer"/>.
/// </summary>
[JsonConverter(typeof(DecisionAnswerConverter))]
public abstract class DecisionAnswer
{
    /// <summary>
    /// Initializes a new answer with the given discriminator.
    /// </summary>
    /// <param name="type">The <c>type</c> discriminator.</param>
    protected DecisionAnswer(string type)
    {
        Type = type;
    }

    /// <summary>The answer type (see <see cref="DecisionQuestionTypes"/>).</summary>
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>The answer to a <see cref="NoulDecisionQuestion"/> (<c>noul</c>).</summary>
public sealed class NoulDecisionAnswer : DecisionAnswer
{
    /// <summary>Initializes a new instance of the <see cref="NoulDecisionAnswer"/> class.</summary>
    public NoulDecisionAnswer() : base(DecisionQuestionTypes.Noul)
    {
    }

    /// <summary>The probability that the answer is true.</summary>
    [JsonPropertyName("noul")]
    public double? Noul { get; set; }

    /// <summary>The confidence of the answer.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }
}

/// <summary>The answer to a <see cref="ChoiceDecisionQuestion"/> (<c>choice</c>).</summary>
public sealed class ChoiceDecisionAnswer : DecisionAnswer
{
    /// <summary>Initializes a new instance of the <see cref="ChoiceDecisionAnswer"/> class.</summary>
    public ChoiceDecisionAnswer() : base(DecisionQuestionTypes.Choice)
    {
    }

    /// <summary>The selected choice identifier.</summary>
    [JsonPropertyName("choice")]
    public string? Choice { get; set; }

    /// <summary>The confidence of the answer.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>The probability of each choice.</summary>
    [JsonPropertyName("probabilities")]
    public Dictionary<string, double>? Probabilities { get; set; }
}

/// <summary>The answer to a <see cref="ScoreDecisionQuestion"/> (<c>score</c>).</summary>
public sealed class ScoreDecisionAnswer : DecisionAnswer
{
    /// <summary>Initializes a new instance of the <see cref="ScoreDecisionAnswer"/> class.</summary>
    public ScoreDecisionAnswer() : base(DecisionQuestionTypes.Score)
    {
    }

    /// <summary>The score.</summary>
    [JsonPropertyName("score")]
    public double? Score { get; set; }

    /// <summary>The confidence of the answer.</summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>The meaning of each score level.</summary>
    [JsonPropertyName("legend")]
    public Dictionary<string, string>? Legend { get; set; }

    /// <summary>The probability of each score level.</summary>
    [JsonPropertyName("probabilities")]
    public Dictionary<string, double>? Probabilities { get; set; }
}

/// <summary>An answer whose <c>type</c> this SDK does not know; the raw JSON is kept.</summary>
public sealed class UnknownDecisionAnswer : DecisionAnswer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownDecisionAnswer"/> class.
    /// </summary>
    /// <param name="type">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownDecisionAnswer(string? type, JsonNode raw) : base(type ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the answer.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>Known decision question and answer types.</summary>
public static class DecisionQuestionTypes
{
    /// <summary>A yes/no question answered with a probability.</summary>
    public const string Noul = "noul";

    /// <summary>A multiple-choice question.</summary>
    public const string Choice = "choice";

    /// <summary>A scoring question.</summary>
    public const string Score = "score";
}

/// <summary>Known values of <see cref="DecisionPromptFeedback.BlockReason"/>.</summary>
public static class DecisionBlockReasons
{
    /// <summary>Blocked for safety reasons.</summary>
    public const string Safety = "safety";

    /// <summary>Blocked for other reasons.</summary>
    public const string Other = "other";

    /// <summary>Blocked because of terms on the blocklist.</summary>
    public const string Blocklist = "blocklist";

    /// <summary>Blocked because of prohibited content.</summary>
    public const string ProhibitedContent = "prohibited_content";

    /// <summary>Blocked because of unsafe image content.</summary>
    public const string ImageSafety = "image_safety";
}

/// <summary>Models of the Decisions API.</summary>
public static class DecisionModels
{
    /// <summary>The Gemini decision model.</summary>
    public const string GeminiDecision1 = "gemini-decision-1";
}
