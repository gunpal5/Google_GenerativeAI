namespace GenerativeAI.Types;

/// <summary>
/// Represents the result of a vision generative model.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/imagen-api">See Official API Documentation</seealso>
public class VisionGenerativeModelResult
{
    /// <summary>
    /// Gets or sets the base64 encoded generated image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("bytesBase64Encoded")]
    public string? BytesBase64Encoded { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the generated image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the reason why the image was filtered by responsible AI, if applicable.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("raiFilteredReason")]
    public string? RaiFilteredReason { get; set; }

    /// <summary>
    /// Gets or sets the safety attributes of the generated image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("safetyAttributes")]
    public SafetyAttributes? SafetyAttributes { get; set; }

    /// <summary>
    /// Enhanced prompt used for generation.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
}