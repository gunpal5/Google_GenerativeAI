using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

public static class GenerateAnswerResponseExtension
{
    public static string GetAnswer(this GenerateAnswerResponse? response)
    {
        if(response==null)
            throw new ArgumentNullException(nameof(response));
        if(response.Answer == null)
            throw new ArgumentNullException(nameof(response.Answer));
        return string.Join("\r\n", response.Answer.Content.Parts.Select(s => s.Text));
    }
}