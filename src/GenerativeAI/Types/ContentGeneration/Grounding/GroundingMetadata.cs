using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Metadata returned to client when grounding is enabled.
/// </summary>
/// <seealso href="https://ai.google.dev/api/generate-content#GroundingMetadata">See Official API Documentation</seealso> 
public class GroundingMetadata
{
    /// <summary>
    /// List of supporting references retrieved from the specified grounding source.
    /// </summary>
    [JsonPropertyName("groundingChunks")]
    public List<GroundingChunk>? GroundingChunks { get; set; }

    /// <summary>
    /// List of grounding support.
    /// </summary>
    [JsonPropertyName("groundingSupports")]
    public List<GroundingSupport>? GroundingSupports { get; set; }

    /// <summary>
    /// Web search queries for the following-up web search.
    /// </summary>
    [JsonPropertyName("webSearchQueries")]
    public List<string>? WebSearchQueries { get; set; }

    /// <summary>
    /// Optional. Google search entry for the following-up web searches.
    /// </summary>
    [JsonPropertyName("searchEntryPoint")]
    public SearchEntryPoint? SearchEntryPoint { get; set; }

    /// <summary>
    /// Metadata related to retrieval in the grounding flow.
    /// </summary>
    [JsonPropertyName("retrievalMetadata")]
    public RetrievalMetadata? RetrievalMetadata { get; set; }
}