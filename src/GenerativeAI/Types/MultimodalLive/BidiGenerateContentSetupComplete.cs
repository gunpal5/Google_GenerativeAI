using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Sent in response to a <see cref="BidiGenerateContentSetup"/> message from the client.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#BidiGenerateContentSetupComplete">See Official API Documentation</seealso>
public class BidiGenerateContentSetupComplete
{
    /// <summary>
    /// The session id of the live session.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
}