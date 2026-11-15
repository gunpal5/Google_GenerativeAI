using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The Interaction resource returned by the Interactions API (<c>/interactions</c>). The output is exposed as
/// <see cref="Steps"/>; <see cref="OutputText"/>, <see cref="OutputImage"/>, <see cref="OutputAudio"/> and
/// <see cref="OutputVideo"/> are convenience accessors computed from the steps, like python-genai's helpers.
/// </summary>
public sealed class Interaction
{
    /// <summary>Output only. A unique identifier for the interaction.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Output only. The status of the interaction (see <see cref="InteractionStatuses"/>).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Output only. The resource type. Only present on interactions embedded in streaming events.</summary>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    /// <summary>The model that served the interaction.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>The agent that served the interaction.</summary>
    [JsonPropertyName("agent")]
    public string? Agent { get; set; }

    /// <summary>The agent configuration.</summary>
    [JsonPropertyName("agent_config")]
    public InteractionAgentConfig? AgentConfig { get; set; }

    /// <summary>Output only. Creation time in ISO 8601 format.</summary>
    [JsonPropertyName("created")]
    public string? Created { get; set; }

    /// <summary>Output only. Last update time in ISO 8601 format.</summary>
    [JsonPropertyName("updated")]
    public string? Updated { get; set; }

    /// <summary>Output only. The steps that make up the interaction, when included in the response.</summary>
    [JsonPropertyName("steps")]
    public List<InteractionStep>? Steps { get; set; }

    /// <summary>The input of the interaction (only returned when requested with <c>include_input</c>).</summary>
    [JsonPropertyName("input")]
    public InteractionInput? Input { get; set; }

    /// <summary>Statistics on the interaction's token usage.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }

    /// <summary>
    /// Opaque token to resume a long decode. Set when <see cref="Status"/> is
    /// <see cref="InteractionStatuses.RequiresContinuation"/>; pass it back unchanged to continue decoding.
    /// </summary>
    [JsonPropertyName("continuation_token")]
    public string? ContinuationToken { get; set; }

    /// <summary>The ID of the previous interaction, if any.</summary>
    [JsonPropertyName("previous_interaction_id")]
    public string? PreviousInteractionId { get; set; }

    /// <summary>Output only. Diagnostic faults or platform errors recorded on the interaction.</summary>
    [JsonPropertyName("errors")]
    public List<InteractionError>? Errors { get; set; }

    /// <summary>The environment configuration of the interaction.</summary>
    [JsonPropertyName("environment")]
    public InteractionEnvironmentReference? Environment { get; set; }

    /// <summary>Output only. The environment ID, when an environment was configured.</summary>
    [JsonPropertyName("environment_id")]
    public string? EnvironmentId { get; set; }

    /// <summary>The generation configuration.</summary>
    [JsonPropertyName("generation_config")]
    public InteractionGenerationConfig? GenerationConfig { get; set; }

    /// <summary>The system instruction.</summary>
    [JsonPropertyName("system_instruction")]
    public string? SystemInstruction { get; set; }

    /// <summary>The tools available to the model.</summary>
    [JsonPropertyName("tools")]
    public List<InteractionTool>? Tools { get; set; }

    /// <summary>The requested response format(s).</summary>
    [JsonPropertyName("response_format")]
    [JsonConverter(typeof(Converters.InteractionResponseFormatListConverter))]
    public List<InteractionResponseFormat>? ResponseFormat { get; set; }

    /// <summary>Deprecated in the API. The MIME type of the response.</summary>
    [JsonPropertyName("response_mime_type")]
    public string? ResponseMimeType { get; set; }

    /// <summary>Deprecated in the API. The requested response modalities (see <see cref="InteractionModalities"/>).</summary>
    [JsonPropertyName("response_modalities")]
    public List<string>? ResponseModalities { get; set; }

    /// <summary>Safety settings.</summary>
    [JsonPropertyName("safety_settings")]
    public List<InteractionSafetySetting>? SafetySettings { get; set; }

    /// <summary>The service tier (see <see cref="InteractionServiceTiers"/>).</summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    /// <summary>User-defined labels.</summary>
    [JsonPropertyName("labels")]
    public Dictionary<string, string>? Labels { get; set; }

    /// <summary>Webhook configuration.</summary>
    [JsonPropertyName("webhook_config")]
    public InteractionWebhookConfig? WebhookConfig { get; set; }

    /// <summary>Deprecated in the API. The cached content used as context.</summary>
    [JsonPropertyName("cached_content")]
    public string? CachedContent { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// Concatenated text of the trailing model output, after the last user input (thought steps are skipped);
    /// an empty string when there is none. Mirrors python-genai's <c>Interaction.output_text</c>.
    /// </summary>
    [JsonIgnore]
    public string OutputText
    {
        get
        {
            var parts = new List<string>();
            var collecting = false;
            var steps = Steps ?? new List<InteractionStep>();
            for (var s = steps.Count - 1; s >= 0; s--)
            {
                var step = steps[s];
                if (step.Type == InteractionStepTypes.UserInput)
                    break;
                if (step.Type == InteractionStepTypes.Thought)
                    continue;
                if (step is not InteractionModelOutputStep output)
                {
                    if (collecting)
                        break;
                    continue;
                }

                if (output.Content == null)
                    continue;

                var stop = false;
                for (var c = output.Content.Count - 1; c >= 0; c--)
                {
                    if (output.Content[c] is InteractionTextContent text)
                    {
                        collecting = true;
                        parts.Add(text.Text ?? string.Empty);
                    }
                    else if (collecting)
                    {
                        stop = true;
                        break;
                    }
                }

                if (stop)
                    break;
            }

            parts.Reverse();
            var builder = new StringBuilder();
            foreach (var part in parts)
                builder.Append(part);
            return builder.ToString();
        }
    }

    /// <summary>The last generated image of the model output steps, or <c>null</c>.</summary>
    [JsonIgnore]
    public InteractionImageContent? OutputImage => FindLastOutput<InteractionImageContent>();

    /// <summary>The last generated audio of the model output steps, or <c>null</c>.</summary>
    [JsonIgnore]
    public InteractionAudioContent? OutputAudio => FindLastOutput<InteractionAudioContent>();

    /// <summary>
    /// The last generated video of the model output steps, or <c>null</c>. The video is either inline
    /// (<see cref="InteractionMediaContent.Data"/>, see <see cref="InteractionMediaContent.GetDataBytes"/>) or
    /// referenced by <see cref="InteractionMediaContent.Uri"/>.
    /// </summary>
    [JsonIgnore]
    public InteractionVideoContent? OutputVideo => FindLastOutput<InteractionVideoContent>();

    /// <summary>
    /// The function calls of the trailing model turn (after the last user input), which the client must answer
    /// with <see cref="InteractionFunctionResultStep"/>s when <see cref="Status"/> is
    /// <see cref="InteractionStatuses.RequiresAction"/>.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<InteractionFunctionCallStep> FunctionCalls
    {
        get
        {
            var calls = new List<InteractionFunctionCallStep>();
            var steps = Steps ?? new List<InteractionStep>();
            for (var s = steps.Count - 1; s >= 0; s--)
            {
                if (steps[s].Type == InteractionStepTypes.UserInput)
                    break;
                if (steps[s] is InteractionFunctionCallStep call)
                    calls.Insert(0, call);
            }

            return calls;
        }
    }

    private T? FindLastOutput<T>() where T : InteractionContent
    {
        var steps = Steps ?? new List<InteractionStep>();
        for (var s = steps.Count - 1; s >= 0; s--)
        {
            if (steps[s] is not InteractionModelOutputStep output || output.Content == null)
                continue;
            for (var c = output.Content.Count - 1; c >= 0; c--)
            {
                if (output.Content[c] is T match)
                    return match;
            }
        }

        return null;
    }
}

/// <summary>Token usage of an interaction.</summary>
public sealed class InteractionUsage
{
    /// <summary>Number of tokens in the prompt (context).</summary>
    [JsonPropertyName("total_input_tokens")]
    public int? TotalInputTokens { get; set; }

    /// <summary>Total number of tokens across all generated responses.</summary>
    [JsonPropertyName("total_output_tokens")]
    public int? TotalOutputTokens { get; set; }

    /// <summary>Number of thought tokens for thinking models.</summary>
    [JsonPropertyName("total_thought_tokens")]
    public int? TotalThoughtTokens { get; set; }

    /// <summary>Number of tokens in the cached part of the prompt.</summary>
    [JsonPropertyName("total_cached_tokens")]
    public int? TotalCachedTokens { get; set; }

    /// <summary>Number of tokens present in tool-use prompt(s).</summary>
    [JsonPropertyName("total_tool_use_tokens")]
    public int? TotalToolUseTokens { get; set; }

    /// <summary>Total token count (prompt + responses + other internal tokens).</summary>
    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; set; }

    /// <summary>Number of input tokens used in the first turn of this interaction.</summary>
    [JsonPropertyName("initial_input_tokens")]
    public int? InitialInputTokens { get; set; }

    /// <summary>Input token usage by modality.</summary>
    [JsonPropertyName("input_tokens_by_modality")]
    public List<InteractionModalityTokens>? InputTokensByModality { get; set; }

    /// <summary>Output token usage by modality.</summary>
    [JsonPropertyName("output_tokens_by_modality")]
    public List<InteractionModalityTokens>? OutputTokensByModality { get; set; }

    /// <summary>Cached token usage by modality.</summary>
    [JsonPropertyName("cached_tokens_by_modality")]
    public List<InteractionModalityTokens>? CachedTokensByModality { get; set; }

    /// <summary>Tool-use token usage by modality.</summary>
    [JsonPropertyName("tool_use_tokens_by_modality")]
    public List<InteractionModalityTokens>? ToolUseTokensByModality { get; set; }

    /// <summary>Grounding tool usage counts.</summary>
    [JsonPropertyName("grounding_tool_count")]
    public List<InteractionGroundingToolCount>? GroundingToolCount { get; set; }
}

/// <summary>Token count for one modality.</summary>
public sealed class InteractionModalityTokens
{
    /// <summary>The modality (see <see cref="InteractionModalities"/>).</summary>
    [JsonPropertyName("modality")]
    public string? Modality { get; set; }

    /// <summary>Number of tokens for the modality.</summary>
    [JsonPropertyName("tokens")]
    public int? Tokens { get; set; }
}

/// <summary>Usage count of a grounding tool.</summary>
public sealed class InteractionGroundingToolCount
{
    /// <summary>The grounding tool type (<c>google_search</c>, <c>google_maps</c>, <c>retrieval</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The number of uses.</summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

/// <summary>An error recorded on an interaction or sent in an error event.</summary>
public sealed class InteractionError
{
    /// <summary>A URI that identifies the error type.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>A human-readable error message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>A <c>google.rpc.Status</c> error.</summary>
public sealed class InteractionRpcStatus
{
    /// <summary>The status code (a <c>google.rpc.Code</c> value).</summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>A developer-facing error message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Error details.</summary>
    [JsonPropertyName("details")]
    public List<JsonObject>? Details { get; set; }
}
