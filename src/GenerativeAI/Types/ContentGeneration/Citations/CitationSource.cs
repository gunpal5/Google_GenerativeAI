using System.Text.Json.Serialization;

namespace GenerativeAI.Types
{
    /// <summary>
    /// A citation to a source for a portion of a specific response.
    /// </summary>
    /// <seealso href="https://ai.google.dev/api/generate-content#CitationSource">Official CitationSource Documentation</seealso>
    public class CitationSource
    {
        /// <summary>
        /// Optional. Start of segment of the response that is attributed to this source.
        /// Index indicates the start of the segment, measured in bytes.
        /// </summary>
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// Optional. End of the attributed segment, exclusive.
        /// </summary>
        [JsonPropertyName("endIndex")]
        public int? EndIndex { get; set; }

        /// <summary>
        /// Optional. URI that is attributed as a source for a portion of the text.
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// Optional. License for the GitHub project that is attributed as a source for segment.
        /// License info is required for code citations.
        /// </summary>
        [JsonPropertyName("license")]
        public string? License { get; set; }
    }
}