namespace GenerativeAI.Types;

/// <summary>
/// Represents the request body for the visual question answering API.
/// </summary>
/// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/visual-question-answering">See Official API Documentation</seealso>
public class VqaRequest
{
    /// <summary>
    /// The list of instances for the request.  Only 1 image object allowed.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("instances")]
    public List<VqaInstance>? Instances { get; set; }

    /// <summary>
    /// The parameters for the request.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("parameters")]
    public VqaParameters? Parameters { get; set; }
}