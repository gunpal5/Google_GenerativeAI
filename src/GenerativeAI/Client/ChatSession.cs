using GenerativeAI.Exceptions;
using GenerativeAI.Helpers;
using GenerativeAI.Models;
using GenerativeAI.Requests;
using GenerativeAI.Types;

namespace GenerativeAI.Methods
{
    public class ChatSession
    {
        #region Properties
        public List<Content> History { get; private set; }
        public GenerativeModel Model { get; private set; }
        #endregion

        #region Constructor

        public ChatSession(GenerativeModel model, StartChatParams @params)
        {
            this.Model = model;
            if (@params.History != null)
            {
                this.History = @params.History.Select(s => RequestExtensions.FormatGenerateContentInput(s.Parts)).ToList();
            }
            else this.History = new List<Content>();
        }
        #endregion

        #region public methods
        /// <summary>
        /// Send Message to Model
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Content Response</returns>
        /// <exception cref="GenerativeAIException">throws Generative AI Exception</exception>
        public async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            var content = RequestExtensions.FormatGenerateContentInput(message);

            var contents = new List<Content>();
            contents.AddRange(this.History);
            contents.Add(content);

            var request = new GenerateContentRequest()
            {
                Contents = contents.ToArray()
            };

            var response = await Model.GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.Candidates is { Length: > 0 } && response.Candidates[0].Content!=null)
            {
                this.History.Add(content);
                var responseContent = response.Candidates[0].Content;
               
                responseContent.Role = Roles.Model;

                this.History.Add(responseContent);
            }
            else
            {
                var  blockErrorMessage = ResponseHelper.FormatBlockErrorMessage(response);
                if (!string.IsNullOrEmpty(blockErrorMessage))
                {
                    throw new GenerativeAIException(blockErrorMessage,blockErrorMessage);
                }
            }
           
            return response.Text();
        }
        /// <summary>
        /// Send Message with Content Parts.
        /// </summary>
        /// <param name="parts">Parts</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns cref="EnhancedGenerateContentResponse">Content Response</returns>
        public async Task<EnhancedGenerateContentResponse> SendMessageAsync(IEnumerable<Part> parts,
            CancellationToken cancellationToken = default)
        {
            var request = new GenerateContentRequest()
                { Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) } };
            return await SendMessageAsync(request, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Send Message with Generate Request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns cref="EnhancedGenerateContentResponse">Content Response</returns>
        /// <exception cref="Exception"></exception>
        public async Task<EnhancedGenerateContentResponse> SendMessageAsync(GenerateContentRequest request, CancellationToken cancellationToken = default)
        {
            var contents = new List<Content>();
            contents.AddRange(this.History);
            if (request.Contents != null)
            {
                contents.AddRange(request.Contents);
            }
            var request2 = new GenerateContentRequest()
            {
                Contents = contents.ToArray(),
                GenerationConfig = request.GenerationConfig,
                SafetySettings = request.SafetySettings
            };
            var response = await Model.GenerateContentAsync(request2,cancellationToken).ConfigureAwait(false);

            if (response.Candidates is { Length: > 0 })
            {
                if (request.Contents != null)
                    this.History.AddRange(request.Contents);
                var responseContent = response.Candidates[0].Content;
                if (responseContent != null)
                {
                    responseContent.Role = Roles.Model;

                    this.History.Add(responseContent);
                }
            }
            else
            {
                var blockErrorMessage = ResponseHelper.FormatBlockErrorMessage(response);
                if (!string.IsNullOrEmpty(blockErrorMessage))
                {
                    throw new GenerativeAIException(blockErrorMessage,blockErrorMessage);
                }
            }

            return response;
        }
        #endregion
    }
}
