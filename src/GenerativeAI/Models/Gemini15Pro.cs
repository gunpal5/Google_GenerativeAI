using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI.Classes;
using GenerativeAI.Methods;
using GenerativeAI.Tools;
using GenerativeAI.Types;

namespace GenerativeAI.Models
{
    public class Gemini15Pro:GenerativeModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Google Generative AI API Key</param>
        /// <param name="client">HTTP Client</param>
        /// <param name="functions">Available Extension Functions</param>
        /// <param name="calls">Function Calls</param>
        public Gemini15Pro(string apiKey, System.Net.Http.HttpClient? client = null, ICollection<ChatCompletionFunction>? functions = null, IReadOnlyDictionary<string, Func<string, CancellationToken, Task<string>>>? calls = null, string? systemInstruction = null) : base(apiKey, GoogleAIModels.Gemini15Pro, client, functions, calls, systemInstruction)
        {
        }

        /// <summary>
        /// Generate Content with Gemini Vision
        /// </summary>
        /// <param name="prompt">Prompt for processing Image</param>
        /// <param name="imageObject">Image File Object</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<EnhancedGenerateContentResponse> GenerateContentAsync(string prompt, FileObject imageObject,
            CancellationToken cancellationToken = default)
        {
            var imagePart = new Part()
            {
                InlineData = new GenerativeContentBlob()
                {
                    MimeType = MimeTypeHelper.GetMimeType(imageObject.FileName),
                    Data = Convert.ToBase64String(imageObject.FileContent)
                }
            };

            var textPart = new Part()
            {
                Text = prompt
            };

            var parts = new[] { textPart, imagePart };

            return await GenerateContentAsync(parts, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Stream Content with Gemini Vision
        /// </summary>
        /// <param name="prompt">Prompt for processing Image</param>
        /// <param name="imageObject">Image File Object</param>
        /// <param name="handler">Stream Content Handler</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public async Task<string> StreamContentAsync(string prompt, FileObject imageObject, Action<string> handler,
            CancellationToken cancellationToken = default)
        {
            var imagePart = new Part()
            {
                InlineData = new GenerativeContentBlob()
                {
                    MimeType = MimeTypeHelper.GetMimeType(imageObject.FileName),
                    Data = Convert.ToBase64String(imageObject.FileContent)
                }
            };

            var textPart = new Part()
            {
                Text = prompt
            };

            var parts = new[] { textPart, imagePart };

            return await StreamContentAsync(parts, handler, cancellationToken).ConfigureAwait(false);
        }

        public override ChatSession StartChat(StartChatParams startSessionParams)
        {
            startSessionParams.IsVision = true;
            return base.StartChat(startSessionParams);
        }
    }
}
