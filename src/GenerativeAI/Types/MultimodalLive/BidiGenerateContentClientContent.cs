using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Incremental update of the current conversation delivered from the client. All of the content here is unconditionally appended to the conversation history and used as part of the prompt to the model to generate content.
/// A message here will interrupt any current model generation.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontentclientcontent">See Official API Documentation</seealso>
public class BidiGenerateContentClientContent
{
    /// <summary>
    /// The content appended to the current conversation with the model.
    /// For single-turn queries, this is a single instance. For multi-turn queries, this is a repeated field that contains conversation history and the latest request.
    /// </summary>
    [JsonPropertyName("turns")]
    public Content[]? Turns { get; set; }

    /// <summary>
    /// If true, indicates that the server content generation should start with the currently accumulated prompt. Otherwise, the server awaits additional messages before starting generation.
    /// </summary>
    [JsonPropertyName("turnComplete")]
    public bool? TurnComplete { get; set; }
}