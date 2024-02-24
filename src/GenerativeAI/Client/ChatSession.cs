using GenerativeAI.Classes;
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
        public bool IsVision { get; private set; }
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

            if(model.Model.Contains("vision"))
                this.IsVision = true;
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

        /// <summary>
        /// Generate Content with Gemini Vision
        /// </summary>
        /// <param name="prompt">Prompt for processing Image</param>
        /// <param name="imageObject">Image File</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns cref="EnhancedGenerateContentResponse">Content Response</returns>
        public async Task<EnhancedGenerateContentResponse> GenerateContentVisionAsync(string prompt, FileObject imageObject,
            CancellationToken cancellationToken = default)
        {
            if (!IsVision)
                throw new GenerativeAIException("Invalid Model", "This method is only available for Gemini Vision.");
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
            return await SendMessageAsync(parts, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Stream Content with Gemini Vision
        /// </summary>
        /// <param name="prompt">Prompt for processing Image</param>
        /// <param name="imageObject">Image File</param>
        /// <param name="handler">Stream Handler</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns cref="EnhancedGenerateContentResponse">Content Response</returns>
        public async Task<string> StreamContentVisionAsync(string prompt, FileObject imageObject, Action<string> handler,
            CancellationToken cancellationToken = default)
        {
            if (!IsVision)
                throw new GenerativeAIException("Invalid Model", "This method is only available for Gemini Vision.");
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
            return await StreamContentAsync(parts,handler,cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Send Message to Model
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="handler">Stream Handler</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Content Response</returns>
        /// <exception cref="GenerativeAIException">throws Generative AI Exception</exception>
        public async Task<string> StreamContentAsync(string message,Action<string> handler, CancellationToken cancellationToken = default)
        {
            var content = RequestExtensions.FormatGenerateContentInput(message);

            var contents = new List<Content>();
            contents.AddRange(this.History);
            contents.Add(content);

            var request = new GenerateContentRequest()
            {
                Contents = contents.ToArray()
            };

            var response = await Model.StreamContentAsync(request, handler,cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response))
            {
                this.History.Add(content);
                this.History.Add(RequestExtensions.FormatGenerateContentInput(response,Roles.Model));
            }

            return response;
        }

        /// <summary>
        /// Send Message with Content Parts.
        /// </summary>
        /// <param name="parts">Parts</param>
        /// <param name="handler">Stream Handler</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns cref="EnhancedGenerateContentResponse">Content Response</returns>
        public async Task<string> StreamContentAsync(IEnumerable<Part> parts, Action<string> handler,
            CancellationToken cancellationToken = default)
        {
            var request = new GenerateContentRequest()
            { Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) } };
            return await StreamContentAsync(request, handler, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Send Message with Generate Request
        /// </summary>
        /// <param name="request">Generative AI Request</param>
        /// <param name="handler">Stream Handler</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns cref="EnhancedGenerateContentResponse">Content Response</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> StreamContentAsync(GenerateContentRequest request,Action<string> handler, CancellationToken cancellationToken = default)
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
            var response = await Model.StreamContentAsync(request2,handler,cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response))
            {
                if (request.Contents != null)
                    this.History.AddRange(request.Contents);
                this.History.Add(RequestExtensions.FormatGenerateContentInput(response,Roles.Model));
            }

            return response;
        }


        #endregion
    }
}
