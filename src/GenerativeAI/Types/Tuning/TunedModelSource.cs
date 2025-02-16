using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tuned model as a source for training a new model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tuning#TunedModelSource">See Official API Documentation</seealso>
public class TunedModelSource
{
    /// <summary>
    /// Immutable. The name of the <see cref="TunedModel"/> to use as the starting point for training the new model.
    /// Example: <c>tunedModels/my-tuned-model</c>
    /// </summary>
    [JsonPropertyName("tunedModel")]
    public string? TunedModel { get; set; }

    /// <summary>
    /// Output only. The name of the base <see cref="TunedModel"/> this <see cref="Model"/> was tuned from.
    /// Example: <c>models/gemini-1.5-flash-001</c>
    /// </summary>
    [JsonPropertyName("baseModel")]
    public string? BaseModel { get; set; }
}