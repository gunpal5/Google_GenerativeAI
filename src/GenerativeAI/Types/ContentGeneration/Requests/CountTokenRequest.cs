using System.Text.Json.Serialization;
using GenerativeAI.Types.ContentGeneration.Inputs;

namespace GenerativeAI.Types.ContentGeneration.Requests;

/// <summary>
/// Request to count tokens.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/models/countTokens">See Official API Documentation</seealso>
public class CountTokensRequest
{
    /// <summary>
    /// Optional. The input given to the model as a prompt. This field is ignored when
    /// <see cref="GenerateContentRequest"/> is set.
    /// </summary>
    [JsonPropertyName("contents")]
    public List<Content>? Contents { get; set; }

    /// <summary>
    /// Optional. The overall input given to the <c>Model</c>. This includes the prompt as well as other model
    /// steering information like <see href="https://ai.google.dev/gemini-api/docs/system-instructions">system instructions</see>,
    /// and/or function declarations for <see href="https://ai.google.dev/gemini-api/docs/function-calling">function calling</see>.
    /// <c>Model</c>/<c>Content</c>s and <c>generateContentRequest</c>s are mutually exclusive. You can either send
    /// <c>Model</c> + <c>Content</c>s or a <c>generateContentRequest</c>, but never both.
    /// </summary>
    [JsonPropertyName("generateContentRequest")]
    public GenerateContentRequest? GenerateContentRequest { get; set; }
}