using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Notification for the client that a previously issued <see cref="BidiGenerateContentToolCall"/> with the specified <c>id</c>s should have been not executed and should be cancelled. If there were side-effects to those tool calls, clients may attempt to undo the tool calls. This message occurs only in cases where the clients interrupt server turns.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontenttoolcallcancellation">See Official API Documentation</seealso>
public class BidiGenerateContentToolCallCancellation
{
    /// <summary>
    /// Output only. The ids of the tool calls to be cancelled.
    /// </summary>
    [JsonPropertyName("ids")]
    public string[]? Ids { get; set; }
}