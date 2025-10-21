using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Input configuration for a batch job (Google AI / MLDev format).
/// </summary>
public class BatchInputConfig
{
    /// <summary>
    /// The inlined requests for batch processing.
    /// </summary>
    [JsonPropertyName("requests")]
    public RequestsWrapper? Requests { get; set; }

    /// <summary>
    /// The file name for batch processing.
    /// </summary>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
}

/// <summary>
/// Wrapper for requests array (Google AI nested structure).
/// </summary>
public class RequestsWrapper
{
    /// <summary>
    /// The array of wrapped requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public List<InlinedRequestWrapper>? Requests { get; set; }
}
