using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenerativeAI.Services.Classes
{
    public class ModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        public string ModelId { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("inputTokenLimit")]
        public int InputTokenLimit { get; set; }

        [JsonPropertyName("outputTokenLimit")]
        public int OutputTokenLimit { get; set; }

        [JsonPropertyName("supportedGenerationMethods")]
        public List<string> SupportedGenerationMethods { get; set; }

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("topP")]
        public double? TopP { get; set; }

        [JsonPropertyName("topK")]
        public double? TopK { get; set; }
    }
}
