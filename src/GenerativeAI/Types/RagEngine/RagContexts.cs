using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Relevant contexts for one query.
/// </summary>
public class RagContexts
{
    /// <summary>
    /// All its contexts.
    /// </summary>
    [JsonPropertyName("contexts")]
    public System.Collections.Generic.ICollection<RagContextsContext>? Contexts { get; set; } 
}