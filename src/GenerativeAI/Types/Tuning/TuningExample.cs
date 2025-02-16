using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A single example for tuning.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tuning#TuningExample">See Official API Documentation</seealso>
public class TuningExample
{
    /// <summary>
    /// Required. The expected model output.
    /// </summary>
    [JsonPropertyName("output")]
    public string Output { get; set; } = null!;

    /// <summary>
    /// Optional. Text model input.
    /// </summary>
    [JsonPropertyName("textInput")]
    public string? TextInput { get; set; }
}