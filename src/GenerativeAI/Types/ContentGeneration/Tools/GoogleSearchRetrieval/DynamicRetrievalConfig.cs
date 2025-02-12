using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Describes the options to customize dynamic retrieval.
/// </summary>
/// <seealso href="https://ai.google.dev/api/rest/v1beta/DynamicRetrievalConfig">See Official API Documentation</seealso> 
public class DynamicRetrievalConfig
{
    /// <summary>
    /// The mode of the predictor to be used in dynamic retrieval.
    /// </summary>
    [JsonPropertyName("mode")]
    public DynamicRetrievalMode Mode { get; set; }

    /// <summary>
    /// The threshold to be used in dynamic retrieval.
    /// If not set, a system default value is used.
    /// </summary>
    [JsonPropertyName("dynamicThreshold")]
    public double? DynamicThreshold { get; set; }
}