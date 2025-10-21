using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents an inlined response from batch processing.
/// </summary>
public class InlinedResponse
{
    /// <summary>
    /// The response to the request.
    /// </summary>
    [JsonPropertyName("response")]
    public GenerateContentResponse? Response { get; set; }

    /// <summary>
    /// The error encountered while processing the request.
    /// </summary>
    [JsonPropertyName("error")]
    public JobError? Error { get; set; }
}
