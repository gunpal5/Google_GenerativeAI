using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The sources in which to ground the answer.
/// </summary>
public class GroundingSource
{
    /// <summary>
    /// Passages provided inline with the request.
    /// </summary>
    [JsonPropertyName("inlinePassages")]
    public GroundingPassages? InlinePassages { get; set; }

    /// <summary>
    /// Content retrieved from resources created via the Semantic Retriever API.
    /// </summary>
    [JsonPropertyName("semanticRetriever")]
    public SemanticRetrieverConfig? SemanticRetriever { get; set; }
}