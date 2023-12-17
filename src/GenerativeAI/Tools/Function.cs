using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenerativeAI.Tools
{
    public class ChatCompletionFunctionParameters
    {

        private IDictionary<string, object>? _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ??= new System.Collections.Generic.Dictionary<string, object>(); }
            set
            {
                _additionalProperties = value;
            }
        }

    }

    public class ChatCompletionFunction
    {
        /// <summary>
        /// The name of the function to be called. Must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length of 64.
        /// </summary>

        [JsonPropertyName("name")]

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [Required(AllowEmptyStrings = true)]
        public string Name { get; set; } = default!;

        /// <summary>
        /// A description of what the function does, used by the model to choose when and how to call the function.
        /// </summary>

        [JsonPropertyName("description")]

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Description { get; set; } = default!;

        [JsonPropertyName("parameters")]

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        [Required]
        public ChatCompletionFunctionParameters Parameters { get; set; } = new ChatCompletionFunctionParameters();

        private IDictionary<string, object>? _additionalProperties;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties ??= new Dictionary<string, object>(); }
            set { _additionalProperties = value; }
        }
    }
}
