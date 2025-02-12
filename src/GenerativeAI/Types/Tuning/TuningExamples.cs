using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// A set of tuning examples. Can be training or validation data.
/// </summary>
/// <seealso href="https://developers.generativeai.google/api/tuning#TuningExamples">See Official API Documentation</seealso>
public class TuningExamples
{
    /// <summary>
    /// The examples. Example input can be for text or discuss, but all examples in a set must be of the same type.
    /// </summary>
    [JsonPropertyName("examples")]
    public List<TuningExample>? Examples { get; set; }
}