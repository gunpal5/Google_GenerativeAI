namespace GenerativeAI.Types;

/// <summary>
/// Represents the response body for the visual question answering API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
public class VqaResponse
{
    /// <summary>
    /// The list of predicted answers.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("predictions")]
    public List<string>? Predictions { get; set; }
}