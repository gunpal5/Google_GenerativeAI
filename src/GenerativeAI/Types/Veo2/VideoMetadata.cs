using System.Text.Json.Serialization;

namespace GenerativeAI.Types
{
    /// <summary>
    /// Metadata describes the input video content.
    /// </summary>
    /// <remarks>
    /// This class represents the metadata associated with a video segment, potentially indicating start and end times.
    /// </remarks>
    public class VideoMetadataVideoGeneration
    {
        /// <summary>
        /// Optional. The end offset of the video segment. Represented as a string duration (e.g., "10.5s").
        /// </summary>
        [JsonPropertyName("endOffset")]
        public string? EndOffset { get; set; }

        /// <summary>
        /// Optional. The start offset of the video segment. Represented as a string duration (e.g., "5s").
        /// </summary>
        [JsonPropertyName("startOffset")]
        public string? StartOffset { get; set; }
    }
}