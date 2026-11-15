using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request for the client to execute the <see cref="FunctionCall"/> and return the responses with the matching <c>id</c>s.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontenttoolcall">See Official API Documentation</seealso>
public class LiveServerToolCall
{
    /// <summary>
    /// Output only. The function call to be executed.
    /// </summary>
    [JsonPropertyName("functionCalls")]
    public FunctionCall[]? FunctionCalls { get; set; }
}