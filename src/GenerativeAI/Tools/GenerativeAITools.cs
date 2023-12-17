using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GenerativeAI.Tools
{
    /// <summary>
    /// <see href="https://ai.google.dev/docs/function_calling"/>
    /// </summary>
    public class GenerativeAITool
    {
        [JsonPropertyName("function_declarations")]
        public List<ChatCompletionFunction>? FunctionDeclaration { get; set; }
    }

    public class GenerativeAITools : List<GenerativeAITool>
    {

    }
}
