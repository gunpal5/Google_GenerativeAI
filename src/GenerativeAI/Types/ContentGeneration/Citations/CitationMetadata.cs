using System.Text.Json.Serialization;

namespace GenerativeAI.Types
{
    /// <summary>
    /// A collection of source attributions for a piece of content.
    /// </summary>
    /// <seealso href="https://ai.google.dev/api/generate-content#citationmetadata">See Official CitationMetadata Documentation</seealso>
    public class CitationMetadata
    {
        /// <summary>
        /// Citations to sources for a specific response.
        /// </summary>
        [JsonPropertyName("citationSources")]
        public List<CitationSource>? CitationSources { get; set; }
    }
}
