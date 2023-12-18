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

        public async Task<EnhancedGenerateContentResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            var content = RequestExtensions.FormatGenerateContentInput(message);

            var contents = new List<Content>();
            contents.AddRange(this.History);
            contents.Add(content);

            var request = new GenerateContentRequest()
            {
                Contents = contents.ToArray()
            };

            var response = await Model.GenerateContentAsync(request,cancellationToken);

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
                    throw new Exception(blockErrorMessage);
                }
            }
           
            return response;
        }

        public async Task<EnhancedGenerateContentResponse> SendMessage(GenerateContentRequest request)
        {
            var contents = new List<Content>();
            contents.AddRange(this.History);
            if (request.Contents != null)
            {
                contents.AddRange(request.Contents);
                
            }
            var request2 = new GenerateContentRequest()
            {
                Contents = contents.ToArray()
            };
            var response = await Model.GenerateContentAsync(request2);

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
                    throw new Exception(blockErrorMessage);
                }
            }

            return response;
        }
        #endregion
    }
}
