using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Response to count tokens.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tokens#response-body">See Official API Documentation</seealso>
public class CountTokensResponse
{
    /// <summary>
    /// The number of tokens that the <c>Model</c> tokenizes the <c>prompt</c> into. Always non-negative.
    /// </summary>
    [JsonPropertyName("totalTokens")]
    public int TotalTokens { get; set; }

    /// <summary>
    /// Number of tokens in the cached part of the prompt (the cached content).
    /// </summary>
    [JsonPropertyName("cachedContentTokenCount")]
    public int CachedContentTokenCount { get; set; }

    /// <summary>
    /// Output only. List of modalities that were processed in the request input.
    /// </summary>
    [JsonPropertyName("promptTokensDetails")]
    public List<ModalityTokenCount>? PromptTokensDetails { get; set; }
}