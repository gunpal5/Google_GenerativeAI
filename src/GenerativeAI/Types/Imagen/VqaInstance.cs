namespace GenerativeAI.Types;

/// <summary>
/// Represents a single instance in the visual question answering request.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
public class VqaInstance
{
    /// <summary>
    /// The question you want to get answered about your image.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    /// <summary>
    /// The image to get information about.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("image")]
    public VqaImage? Image { get; set; }
}