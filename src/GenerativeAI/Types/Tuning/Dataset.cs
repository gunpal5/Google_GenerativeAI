using System.Text.Json.Serialization;

namespace GenerativeAI.Types
{
    /// <summary>
    /// Dataset for training or validation.
    /// </summary>
    /// <seealso href="https://ai.google.dev/api/tuning#Dataset">See Official API Documentation</seealso>
    public class Dataset
    {
        /// <summary>
        /// Inline examples with simple input/output text.
        /// </summary>
        [JsonPropertyName("examples")]
        public TuningExamples? Examples { get; set; }
    }
}