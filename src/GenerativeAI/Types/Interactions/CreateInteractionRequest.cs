using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

/// <summary>
/// Request body of <c>POST /interactions</c>. Set either <see cref="Model"/> (model interaction) or
/// <see cref="Agent"/> (agent interaction, e.g. Deep Research or Antigravity), not both.
/// </summary>
public sealed class CreateInteractionRequest
{
    /// <summary>The model that will complete the prompt (see <see cref="InteractionModels"/>).</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>The agent to interact with (see <see cref="InteractionAgents"/>).</summary>
    [JsonPropertyName("agent")]
    public string? Agent { get; set; }

    /// <summary>Configuration of the agent. Only valid with <see cref="Agent"/>.</summary>
    [JsonPropertyName("agent_config")]
    public InteractionAgentConfig? AgentConfig { get; set; }

    /// <summary>The input of the interaction: a string, content blocks, or steps.</summary>
    [JsonPropertyName("input")]
    public InteractionInput? Input { get; set; }

    /// <summary>System instruction for the interaction.</summary>
    [JsonPropertyName("system_instruction")]
    public string? SystemInstruction { get; set; }

    /// <summary>Tools the model may call during the interaction.</summary>
    [JsonPropertyName("tools")]
    public List<InteractionTool>? Tools { get; set; }

    /// <summary>Generation parameters. Only valid with <see cref="Model"/>.</summary>
    [JsonPropertyName("generation_config")]
    public InteractionGenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// The requested output format(s): structured text, image, audio and/or video. A single entry is sent as an
    /// object, several as an array.
    /// </summary>
    [JsonPropertyName("response_format")]
    [JsonConverter(typeof(InteractionResponseFormatListConverter))]
    public List<InteractionResponseFormat>? ResponseFormat { get; set; }

    /// <summary>Deprecated in the API in favour of <see cref="ResponseFormat"/>. The MIME type of the response.</summary>
    [JsonPropertyName("response_mime_type")]
    public string? ResponseMimeType { get; set; }

    /// <summary>
    /// Deprecated in the API in favour of <see cref="ResponseFormat"/>. The requested response modalities
    /// (see <see cref="InteractionModalities"/>).
    /// </summary>
    [JsonPropertyName("response_modalities")]
    public List<string>? ResponseModalities { get; set; }

    /// <summary>Input only. Run the interaction in the background; poll it with <c>GetAsync</c>.</summary>
    [JsonPropertyName("background")]
    public bool? Background { get; set; }

    /// <summary>Input only. Whether to store the request and response for later retrieval (default true).</summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    /// <summary>Input only. Whether the response is streamed as Server-Sent Events. Set by <c>StreamAsync</c>.</summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>The ID of the previous interaction, to continue a conversation with server-side state.</summary>
    [JsonPropertyName("previous_interaction_id")]
    public string? PreviousInteractionId { get; set; }

    /// <summary>
    /// Opaque token to resume a long decode: pass back the <see cref="Interaction.ContinuationToken"/> of an
    /// interaction whose status is <see cref="InteractionStatuses.RequiresContinuation"/>.
    /// </summary>
    [JsonPropertyName("continuation_token")]
    public string? ContinuationToken { get; set; }

    /// <summary>The environment: an existing environment ID or a remote environment configuration.</summary>
    [JsonPropertyName("environment")]
    public InteractionEnvironmentReference? Environment { get; set; }

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

    /// <summary>
    /// Deprecated in the API. The cached content used as context (<c>cachedContents/{id}</c>). Only valid with
    /// <see cref="Model"/>.
    /// </summary>
    [JsonPropertyName("cached_content")]
    public string? CachedContent { get; set; }

    /// <summary>Additional body fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// Creates a shallow copy of this request.
    /// </summary>
    /// <returns>The copy.</returns>
    public CreateInteractionRequest Clone() => (CreateInteractionRequest)MemberwiseClone();
}
