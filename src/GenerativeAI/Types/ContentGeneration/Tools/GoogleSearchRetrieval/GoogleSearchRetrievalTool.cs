using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tool to retrieve public web data for grounding, powered by Google.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#GoogleSearchRetrieval">See Official API Documentation</seealso> 
public class GoogleSearchRetrievalTool
{
    /// <summary>
    /// Specifies the dynamic retrieval configuration for the given source.
    /// </summary>
    [JsonPropertyName("dynamicRetrievalConfig")]
    public DynamicRetrievalConfig? DynamicRetrievalConfig { get; set; }
}