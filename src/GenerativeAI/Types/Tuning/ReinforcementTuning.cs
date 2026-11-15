using System.Text.Json.Serialization;
using GenerativeAI.Types.RagEngine;

namespace GenerativeAI.Types;

/// <summary>
/// Tuning spec for reinforcement tuning (Vertex AI only).
/// </summary>
public class ReinforcementTuningSpec
{
    /// <summary>
    /// Composite reward function configuration. Mutually exclusive with <see cref="SingleRewardConfig"/>.
    /// </summary>
    [JsonPropertyName("compositeRewardConfig")]
    public CompositeReinforcementTuningRewardConfig? CompositeRewardConfig { get; set; }

    /// <summary>
    /// Cloud Storage path to the JSONL training dataset.
    /// </summary>
    [JsonPropertyName("trainingDatasetUri")]
    public string? TrainingDatasetUri { get; set; }

    /// <summary>
    /// Cloud Storage path to the JSONL validation dataset.
    /// </summary>
    [JsonPropertyName("validationDatasetUri")]
    public string? ValidationDatasetUri { get; set; }

    /// <summary>
    /// Additional hyperparameters to use during tuning.
    /// </summary>
    [JsonPropertyName("hyperParameters")]
    public ReinforcementTuningHyperParameters? HyperParameters { get; set; }

    /// <summary>
    /// Single reward function configuration. Mutually exclusive with <see cref="CompositeRewardConfig"/>.
    /// </summary>
    [JsonPropertyName("singleRewardConfig")]
    public SingleReinforcementTuningRewardConfig? SingleRewardConfig { get; set; }
}

/// <summary>
/// Hyperparameters for reinforcement tuning.
/// </summary>
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class ReinforcementTuningHyperParameters
{
    /// <summary>
    /// Optional. Number of training epochs for the tuning job.
    /// </summary>
    [JsonPropertyName("epochCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? EpochCount { get; set; }

    /// <summary>
    /// Learning rate multiplier for reinforcement learning.
    /// </summary>
    [JsonPropertyName("learningRateMultiplier")]
    public double? LearningRateMultiplier { get; set; }

    /// <summary>
    /// Optional. Adapter size for reinforcement tuning.
    /// </summary>
    [JsonPropertyName("adapterSize")]
    public AdapterSize? AdapterSize { get; set; }

    /// <summary>
    /// Optional. Number of different responses to generate per prompt during tuning.
    /// </summary>
    [JsonPropertyName("samplesPerPrompt")]
    public int? SamplesPerPrompt { get; set; }

    /// <summary>
    /// Optional. Batch size (number of prompts processed per train step).
    /// </summary>
    [JsonPropertyName("batchSize")]
    public int? BatchSize { get; set; }

    /// <summary>
    /// Optional. How often, in steps, to evaluate the tuning job during training.
    /// </summary>
    [JsonPropertyName("evaluateInterval")]
    public int? EvaluateInterval { get; set; }

    /// <summary>
    /// Optional. How often, in steps, to save checkpoints during training.
    /// </summary>
    [JsonPropertyName("checkpointInterval")]
    public int? CheckpointInterval { get; set; }

    /// <summary>
    /// Optional. The maximum number of tokens to generate per prompt.
    /// </summary>
    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Optional. The maximum thinking depth during tuning.
    /// </summary>
    [JsonPropertyName("thinkingLevel")]
    public ReinforcementTuningThinkingLevel? ThinkingLevel { get; set; }

    /// <summary>
    /// Optional. The thinking budget to optimize for (Gemini 2.5 only; -1 means dynamic thinking).
    /// </summary>
    [JsonPropertyName("thinkingBudget")]
    public int? ThinkingBudget { get; set; }

    /// <summary>
    /// Optional. Number of steps for the tuning job (mutually exclusive with <see cref="EpochCount"/>).
    /// </summary>
    [JsonPropertyName("stepCount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? StepCount { get; set; }
}

/// <summary>
/// A single reward function configuration for reinforcement tuning.
/// </summary>
public class SingleReinforcementTuningRewardConfig
{
    /// <summary>
    /// Scores parsed responses by using an autorater model to compute the reward.
    /// </summary>
    [JsonPropertyName("autoraterScorer")]
    public ReinforcementTuningAutoraterScorer? AutoraterScorer { get; set; }

    /// <summary>
    /// A unique reward name identifying this reward.
    /// </summary>
    [JsonPropertyName("rewardName")]
    public string? RewardName { get; set; }

    /// <summary>
    /// Defines how to parse the sample response before scoring.
    /// </summary>
    [JsonPropertyName("parseResponseConfig")]
    public ReinforcementTuningParseResponseConfig? ParseResponseConfig { get; set; }

    /// <summary>
    /// Scores parsed responses by running a Python code snippet.
    /// </summary>
    [JsonPropertyName("codeExecutionRewardScorer")]
    public ReinforcementTuningCodeExecutionRewardScorer? CodeExecutionRewardScorer { get; set; }

    /// <summary>
    /// Scores parsed responses by simple string matching.
    /// </summary>
    [JsonPropertyName("stringMatchRewardScorer")]
    public ReinforcementTuningStringMatchRewardScorer? StringMatchRewardScorer { get; set; }

    /// <summary>
    /// Scores parsed responses by calling a Cloud Run service.
    /// </summary>
    [JsonPropertyName("cloudRunRewardScorer")]
    public ReinforcementTuningCloudRunRewardScorer? CloudRunRewardScorer { get; set; }
}

/// <summary>
/// A composite (weighted) reward function configuration for reinforcement tuning.
/// </summary>
public class CompositeReinforcementTuningRewardConfig
{
    /// <summary>
    /// List of reward function configurations with weights.
    /// </summary>
    [JsonPropertyName("weightedRewardConfigs")]
    public List<CompositeReinforcementTuningRewardConfigWeightedRewardConfig>? WeightedRewardConfigs { get; set; }
}

/// <summary>
/// A weighted single reward configuration inside a <see cref="CompositeReinforcementTuningRewardConfig"/>.
/// </summary>
public class CompositeReinforcementTuningRewardConfigWeightedRewardConfig
{
    /// <summary>
    /// Single reward configuration.
    /// </summary>
    [JsonPropertyName("rewardConfig")]
    public SingleReinforcementTuningRewardConfig? RewardConfig { get; set; }

    /// <summary>
    /// How much this reward contributes to the total overall reward.
    /// </summary>
    [JsonPropertyName("weight")]
    public double? Weight { get; set; }
}

/// <summary>
/// Scores parsed responses with an autorater model.
/// </summary>
public class ReinforcementTuningAutoraterScorer
{
    /// <summary>
    /// Autorater config.
    /// </summary>
    [JsonPropertyName("autoraterConfig")]
    public AutoraterConfig? AutoraterConfig { get; set; }

    /// <summary>
    /// The prompt for the autorater to score the parsed sample response.
    /// </summary>
    [JsonPropertyName("autoraterPrompt")]
    public string? AutoraterPrompt { get; set; }

    /// <summary>
    /// Parses the autorater response for scoring.
    /// </summary>
    [JsonPropertyName("autoraterResponseParseConfig")]
    public ReinforcementTuningParseResponseConfig? AutoraterResponseParseConfig { get; set; }

    /// <summary>
    /// Scores autorater responses by directly converting the parsed response to a float reward.
    /// </summary>
    [JsonPropertyName("parsedResponseConversionScorer")]
    public ReinforcementTuningAutoraterScorerParsedResponseConversionScorer? ParsedResponseConversionScorer { get; set; }

    /// <summary>
    /// Scores autorater responses by exact string match.
    /// </summary>
    [JsonPropertyName("exactMatchScorer")]
    public ReinforcementTuningAutoraterScorerExactMatchScorer? ExactMatchScorer { get; set; }
}

/// <summary>
/// Scores autorater responses by converting the parsed response to a float reward clipped to <c>[-1, 1]</c>.
/// This message has no fields; set an instance to select this scorer.
/// </summary>
public class ReinforcementTuningAutoraterScorerParsedResponseConversionScorer
{
}

/// <summary>
/// Scores autorater responses by exact string match.
/// </summary>
public class ReinforcementTuningAutoraterScorerExactMatchScorer
{
    /// <summary>
    /// Reward if the parsed response equals the expression.
    /// </summary>
    [JsonPropertyName("correctAnswerReward")]
    public double? CorrectAnswerReward { get; set; }

    /// <summary>
    /// Reward if the parsed response does not equal the expression.
    /// </summary>
    [JsonPropertyName("wrongAnswerReward")]
    public double? WrongAnswerReward { get; set; }

    /// <summary>
    /// The string expression to match (supports <c>{{references.key}}</c> placeholders).
    /// </summary>
    [JsonPropertyName("expression")]
    public string? Expression { get; set; }
}

/// <summary>
/// Defines how a response is parsed before scoring.
/// </summary>
public class ReinforcementTuningParseResponseConfig
{
    /// <summary>
    /// The parse type.
    /// </summary>
    [JsonPropertyName("parseType")]
    public ResponseParseType? ParseType { get; set; }

    /// <summary>
    /// The regular expression used when <see cref="ParseType"/> is <see cref="ResponseParseType.REGEX_EXTRACT"/>.
    /// </summary>
    [JsonPropertyName("regexExtractExpression")]
    public string? RegexExtractExpression { get; set; }
}

/// <summary>
/// Scores parsed responses by running a Python code snippet.
/// </summary>
public class ReinforcementTuningCodeExecutionRewardScorer
{
    /// <summary>
    /// The Python code snippet used to evaluate rewards.
    /// </summary>
    [JsonPropertyName("pythonCodeSnippet")]
    public string? PythonCodeSnippet { get; set; }
}

/// <summary>
/// Scores parsed responses by simple string matching.
/// </summary>
public class ReinforcementTuningStringMatchRewardScorer
{
    /// <summary>
    /// Reward returned when the parsed response evaluates to false.
    /// </summary>
    [JsonPropertyName("wrongAnswerReward")]
    public double? WrongAnswerReward { get; set; }

    /// <summary>
    /// Reward returned when the parsed response evaluates to true.
    /// </summary>
    [JsonPropertyName("correctAnswerReward")]
    public double? CorrectAnswerReward { get; set; }

    /// <summary>
    /// Uses a string match expression to evaluate the parsed response.
    /// </summary>
    [JsonPropertyName("stringMatchExpression")]
    public ReinforcementTuningStringMatchRewardScorerStringMatchExpression? StringMatchExpression { get; set; }

    /// <summary>
    /// Uses a JSON match expression to evaluate the parsed response.
    /// </summary>
    [JsonPropertyName("jsonMatchExpression")]
    public ReinforcementTuningStringMatchRewardScorerJsonMatchExpression? JsonMatchExpression { get; set; }
}

/// <summary>
/// Evaluates a parsed response using a match operation against an expression.
/// </summary>
public class ReinforcementTuningStringMatchRewardScorerStringMatchExpression
{
    /// <summary>
    /// The match operation.
    /// </summary>
    [JsonPropertyName("matchOperation")]
    public MatchOperation? MatchOperation { get; set; }

    /// <summary>
    /// A string or regular expression to match against (supports <c>{{references.key}}</c> placeholders).
    /// </summary>
    [JsonPropertyName("expression")]
    public string? Expression { get; set; }
}

/// <summary>
/// Parses the response as JSON, finds the first-level value for <see cref="KeyName"/> and evaluates it.
/// </summary>
public class ReinforcementTuningStringMatchRewardScorerJsonMatchExpression
{
    /// <summary>
    /// The first-level key name to look up in the JSON response.
    /// </summary>
    [JsonPropertyName("keyName")]
    public string? KeyName { get; set; }

    /// <summary>
    /// The string match expression applied to the matched value.
    /// </summary>
    [JsonPropertyName("valueStringMatchExpression")]
    public ReinforcementTuningStringMatchRewardScorerStringMatchExpression? ValueStringMatchExpression { get; set; }
}

/// <summary>
/// Scores parsed responses by calling a Cloud Run service.
/// </summary>
public class ReinforcementTuningCloudRunRewardScorer
{
    /// <summary>
    /// URI of the Cloud Run service used to compute the reward.
    /// </summary>
    [JsonPropertyName("cloudRunUri")]
    public string? CloudRunUri { get; set; }
}

/// <summary>
/// An example (prompt) used to validate a reinforcement tuning reward configuration.
/// </summary>
public class ReinforcementTuningExample
{
    /// <summary>
    /// Multi-turn contents that represent the prompt.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content>? Contents { get; set; }

    /// <summary>
    /// References for the prompt, keyed by reference name.
    /// </summary>
    [JsonPropertyName("references")]
    public Dictionary<string, string>? References { get; set; }

    /// <summary>
    /// System instruction for the prompt.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// Optional. Tools available for the prompt.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<Tool>? Tools { get; set; }
}

/// <summary>
/// Request body of <c>tuningJobs:validateReinforcementTuningReward</c>.
/// </summary>
public class ValidateRewardRequest
{
    /// <summary>
    /// The sample response to validate the reward configuration against.
    /// </summary>
    [JsonPropertyName("sampleResponse")]
    public Content? SampleResponse { get; set; }

    /// <summary>
    /// The example (prompt and references) to validate the reward configuration against.
    /// </summary>
    [JsonPropertyName("example")]
    public ReinforcementTuningExample? Example { get; set; }

    /// <summary>
    /// Single reward configuration. Mutually exclusive with <see cref="CompositeRewardConfig"/>.
    /// </summary>
    [JsonPropertyName("singleRewardConfig")]
    public SingleReinforcementTuningRewardConfig? SingleRewardConfig { get; set; }

    /// <summary>
    /// Composite reward configuration. Mutually exclusive with <see cref="SingleRewardConfig"/>.
    /// </summary>
    [JsonPropertyName("compositeRewardConfig")]
    public CompositeReinforcementTuningRewardConfig? CompositeRewardConfig { get; set; }
}

/// <summary>
/// Response of <c>tuningJobs:validateReinforcementTuningReward</c>.
/// </summary>
public class ValidateRewardResponse
{
    /// <summary>
    /// Output only. The overall (weighted) reward.
    /// </summary>
    [JsonPropertyName("overallReward")]
    public double? OverallReward { get; set; }

    /// <summary>
    /// Output only. A detailed error message, if the validation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Output only. The reward details per reward name.
    /// </summary>
    [JsonPropertyName("rewardInfoDetails")]
    public Dictionary<string, ReinforcementTuningRewardInfo>? RewardInfoDetails { get; set; }
}

/// <summary>
/// The reward info for a single reward function.
/// </summary>
public class ReinforcementTuningRewardInfo
{
    /// <summary>
    /// Output only. The calculated reward.
    /// </summary>
    [JsonPropertyName("reward")]
    public double? Reward { get; set; }

    /// <summary>
    /// Output only. Auxiliary info returned by a Cloud Run reward function.
    /// </summary>
    [JsonPropertyName("userRequestedAuxInfo")]
    public string? UserRequestedAuxInfo { get; set; }

    /// <summary>
    /// Output only. The error status for this reward, if any.
    /// </summary>
    [JsonPropertyName("errorStatus")]
    public GoogleRpcStatus? ErrorStatus { get; set; }
}
