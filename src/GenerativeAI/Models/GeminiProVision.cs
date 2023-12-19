using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI.Tools;
using GenerativeAI.Types;

namespace GenerativeAI.Models
{
    /// <summary>
    /// Gemini Pro Vision
    /// </summary>
    public class GeminiProVision:GenerativeModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Google Generative AI API Key</param>
        /// <param name="client">HTTP Client</param>
        /// <param name="functions">Available Extension Functions</param>
        /// <param name="calls">Function Calls</param>
        public GeminiProVision(string apiKey, System.Net.Http.HttpClient? client = null, ICollection<ChatCompletionFunction>? functions = null, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls = null) : base(apiKey, GoogleAIModels.GeminiProVision, client, functions, calls)
        {
        }
    }
}
