using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata on the generation request's token usage.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#UsageMetadata">See Official API Documentation</seealso>
public class UsageMetadata
{
    /// <summary>
    /// Number of tokens in the prompt. When <see cref="GenerateContentRequest.CachedContent"/> is set, this is still the
    /// total effective prompt size meaning this includes the number of tokens in the cached content.
    /// </summary>
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; set; }

    /// <summary>
    /// Number of tokens in the cached part of the prompt (the cached content).
    /// </summary>
    [JsonPropertyName("cachedContentTokenCount")]
    public int CachedContentTokenCount { get; set; }

    /// <summary>
    /// Total number of tokens across all the generated response candidates.
    /// </summary>
    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; set; }

    /// <summary>
    /// Output only. Total number of tokens across all the generated response candidates.
    /// This is an alias for <see cref="CandidatesTokenCount"/> used in newer API versions.
    /// </summary>
    [JsonPropertyName("responseTokenCount")]
    public int? ResponseTokenCount { get; set; }

    /// <summary>
    /// Output only. Number of tokens present in tool-use prompt(s).
    /// </summary>
    [JsonPropertyName("toolUsePromptTokenCount")]
    public int ToolUsePromptTokenCount { get; set; }

    /// <summary>
    /// Output only. Number of tokens of thoughts for thinking models.
    /// </summary>
    [JsonPropertyName("thoughtsTokenCount")]
    public int ThoughtsTokenCount { get; set; }

    /// <summary>
    /// Total token count for the generation request (prompt + response candidates).
    /// </summary>
    [JsonPropertyName("totalTokenCount")]
    public int? TotalTokenCount { get; set; }

    /// <summary>
    /// Output only. List of modalities that were processed in the request input.
    /// </summary>
    [JsonPropertyName("promptTokensDetails")]
    public List<ModalityTokenCount>? PromptTokensDetails { get; set; }

    /// <summary>
    /// Output only. List of modalities of the cached content in the request input.
    /// </summary>
    [JsonPropertyName("cacheTokensDetails")]
    public List<ModalityTokenCount>? CacheTokensDetails { get; set; }

    /// <summary>
    /// Output only. List of modalities that were returned in the response.
    /// </summary>
    [JsonPropertyName("candidatesTokensDetails")]
    public List<ModalityTokenCount>? CandidatesTokensDetails { get; set; }

    /// <summary>
    /// Output only. List of modalities that were returned in the response.
    /// This is an alias for <see cref="CandidatesTokensDetails"/> used in newer API versions.
    /// </summary>
    [JsonPropertyName("responseTokensDetails")]
    public List<ModalityTokenCount>? ResponseTokensDetails { get; set; }

    /// <summary>
    /// Output only. List of modalities that were processed for tool-use request inputs.
    /// </summary>
    [JsonPropertyName("toolUsePromptTokensDetails")]
    public List<ModalityTokenCount>? ToolUsePromptTokensDetails { get; set; }

    /// <summary>
    /// Output only. Traffic type. This shows whether a request consumes Pay-As-You-Go or Provisioned Throughput quota.
    /// </summary>
    [JsonPropertyName("trafficType")]
    public TrafficType? TrafficType { get; set; }
}