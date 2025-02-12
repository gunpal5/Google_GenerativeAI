using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata related to retrieval in the grounding flow.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#RetrievalMetadata">See Official API Documentation</seealso> 
public class RetrievalMetadata
{
    /// <summary>
    /// Optional. Score indicating how likely information from google search could help answer the prompt.
    /// The score is in the range, where 0 is the least likely and 1 is the most likely.
    /// This score is only populated when google search grounding and dynamic retrieval is enabled.
    /// It will be compared to the threshold to determine whether to trigger google search.
    /// </summary>
    [JsonPropertyName("googleSearchDynamicRetrievalScore")]
    public double? GoogleSearchDynamicRetrievalScore { get; set; }
}