using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Represents an inlined embedding response from batch processing.
/// </summary>
public class InlinedEmbedContentResponse
{
    /// <summary>
    /// The embedding response.
    /// </summary>
    [JsonPropertyName("response")]
    public SingleEmbedContentResponse? Response { get; set; }

    /// <summary>
    /// The error encountered while processing the request.
    /// </summary>
    [JsonPropertyName("error")]
    public JobError? Error { get; set; }
}
