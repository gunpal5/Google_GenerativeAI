namespace GenerativeAI.Types;

/// <summary>
/// Represents the parameters for the visual question answering request.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
public class VqaParameters
{
    /// <summary>
    /// The number of generated text strings to return.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("sampleCount")]
    public int? SampleCount { get; set; }

    /// <summary>
    /// The seed for the random number generator.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("seed")]
    public int? Seed { get; set; }
}