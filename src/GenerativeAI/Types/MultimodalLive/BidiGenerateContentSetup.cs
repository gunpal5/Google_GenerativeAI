using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Message to be sent in the first and only first client message. Contains configuration that will apply for the duration of the streaming session.
/// Clients should wait for a <see cref="BidiGenerateContentSetupComplete"/> message before sending any additional messages.
/// </summary>
/// <seealso href="https://ai.google.dev/gemini-api/docs/multimodal-live#bidigeneratecontentsetup">See Official API Documentation</seealso>
public class BidiGenerateContentSetup
{
    /// <summary>
    /// Required. The model's resource name. This serves as an ID for the Model to use.
    /// Format: <c>models/{model}</c>
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Generation config.
    /// The following fields are not supported:
    /// - <c>responseLogprobs</c>
    /// - <c>responseMimeType</c>
    /// - <c>logprobs</c>
    /// - <c>responseSchema</c>
    /// - <c>stopSequence</c>
    /// - <c>routingConfig</c>
    /// - <c>audioTimestamp</c>
    /// </summary>
    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; set; }

    /// <summary>
    /// The user provided system instructions for the model.
    /// Note: Only text should be used in parts. Content in each part will be in a separate paragraph.
    /// </summary>
    [JsonPropertyName("systemInstruction")]
    public Content? SystemInstruction { get; set; }

    /// <summary>
    /// A list of <see cref="Tool"/> the model may use to generate the next response.
    /// A <see cref="Tool"/> is a piece of code that enables the system to interact with external systems to perform an action, or set of actions, outside of knowledge and scope of the model.
    /// </summary>
    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }
}