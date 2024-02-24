using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenerativeAI.Services.Classes
{
    public class GetModelsListResponse
    {
        [JsonPropertyName("models")]
        public List<ModelInfo> Models { get; set; }
    }
}
