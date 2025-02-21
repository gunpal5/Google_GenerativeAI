using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// User input that is sent in real time.
/// This is different from <see cref="BidiGenerateContentClientContent"/> in a few ways:
/// - Can be sent continuously without interruption to model generation.
/// - If there is a need to mix data interleaved across the <see cref="BidiGenerateContentClientContent"/> and the <see cref="BidiGenerateContentRealtimeInput"/>, the server attempts to optimize for best response, but there are no guarantees.
/// - End of turn is not explicitly specified, but is rather derived from user activity (for example, end of speech).
/// - Even before the end of turn, the data is processed incrementally to optimize for a fast start of the response from the model.
/// - Is always direct user input that is sent in real time. Can be sent continuously without interruptions. The model automatically detects the beginning and the end of user speech and starts or terminates streaming the response accordingly. Data is processed incrementally as it arrives, minimizing latency.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontentrealtimeinput">See Official API Documentation</seealso>
public class BidiGenerateContentRealtimeInput
{
    /// <summary>
    /// Inlined bytes data for media input.
    /// </summary>
    [JsonPropertyName("mediaChunks")]
    public Blob[]? MediaChunks { get; set; }
}