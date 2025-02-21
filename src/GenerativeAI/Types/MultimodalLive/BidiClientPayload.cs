using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents the payload for a bidirectional client interaction.
/// This class is used to encapsulate data such as the setup, client content, real-time input,
/// and tool responses that are involved in bidirectional communication.
/// </summary>
public class BidiClientPayload
{
    /// <summary>
    /// Gets or sets the setup message for a bidirectional content generation session.
    /// </summary>
    [JsonPropertyName("setup")]
    public BidiGenerateContentSetup? Setup { get; set; }

    /// <summary>
    /// Gets or sets an incremental update of the current conversation delivered from the client.
    /// </summary>
    [JsonPropertyName("clientContent")]
    public BidiGenerateContentClientContent? ClientContent { get; set; }

    /// <summary>
    /// Gets or sets user input that is sent in real time.
    /// </summary>
    [JsonPropertyName("realtimeInput")]
    public BidiGenerateContentRealtimeInput? RealtimeInput { get; set; }

    /// <summary>
    /// Gets or sets a client generated response to a <see cref="FunctionCall"/> received from the server.
    /// </summary>
    [JsonPropertyName("toolResponse")]
    public BidiGenerateContentToolResponse? ToolResponse { get; set; }
}