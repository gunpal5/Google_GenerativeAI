using GenerativeAI.Tools;
using System.Net.Http;

namespace GenerativeAI.Models
{
    /// <summary>
    /// Gemini Pro Model
    /// </summary>
    public class GeminiProModel : GenerativeModel
    { 
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Google Generative AI API Key</param>
        /// <param name="client">HTTP Client</param>
        /// <param name="functions">Available Extension Functions</param>
        /// <param name="calls">Function Calls</param>
        public GeminiProModel(string apiKey, HttpClient? client = null, ICollection<ChatCompletionFunction>? functions = null, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls = null) : base(apiKey, GoogleAIModels.GeminiPro, client, functions, calls)
        {
        }
    }
}
