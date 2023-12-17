using GenerativeAI.Types;

namespace GenerativeAI.Requests
{
    public class ResponseHelper
    {
        public static string FormatBlockErrorMessage(GenerateContentResponse response)
        {
            var message = "";
            if (response.Candidates == null || response.Candidates.Length == 0 && response.PromptFeedback!=null)
            {
                message += "Response was blocked";
                if (response.PromptFeedback?.BlockReason >0)
                {
                    message += $" due to {response.PromptFeedback.BlockReason}";
                }

                if (!string.IsNullOrEmpty(response.PromptFeedback?.BlockReasonMessage))
                {
                    message = $" :{response.PromptFeedback?.BlockReasonMessage}";
                }
            }
            else if (response.Candidates?[0] != null)
            {
                var firstCandidate = response.Candidates[0];
                if (hadBadFinishReason(firstCandidate))
                {
                    message += $": {firstCandidate.FinishMessage}";
                }
            }
            return message;
        }

        public static bool hadBadFinishReason(GenerateContentCandidate candidate)
        {
            if (candidate.FinishReason == FinishReason.RECITATION || candidate.FinishReason == FinishReason.SAFETY)
            {
                return false;
            }
            else return true;
        }
    }
}
