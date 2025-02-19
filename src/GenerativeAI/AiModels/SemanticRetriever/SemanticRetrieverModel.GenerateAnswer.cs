using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;

namespace GenerativeAI;

public partial class SemanticRetrieverModel
{
    /// <summary>
    /// Generates an answer asynchronously based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the input details for generating an answer.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Returns a <see cref="GenerateAnswerResponse"/> containing the generated answer and additional context.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    public  async Task<GenerateAnswerResponse> GenerateAnswerAsync(GenerateAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.AnswerStyle == AnswerStyle.ANSWER_STYLE_UNSPECIFIED)
            request.AnswerStyle = AnswerStyle.ABSTRACTIVE;

        if (request.InlinePassages == null && request.SemanticRetriever == null)
        {
            throw new ArgumentNullException(nameof(request.InlinePassages), "Grounding source is required. either InlinePassages or SemanticRetriever set.");
        }

        var answer = await GenerateAnswerAsync(this.ModelName, request, cancellationToken).ConfigureAwait(false);
        if (answer.Answer == null)
        {
            var message = ResponseHelper.FormatErrorMessage(answer.Answer.FinishReason ?? FinishReason.SAFETY);
            throw new GenerativeAIException(message, message);
            return answer;
        }

        return answer;
    }

    /// <summary>
    /// Generates an answer asynchronously based on the given prompt and specified parameters.
    /// </summary>
    /// <param name="prompt">The text input that serves as the basis for generating a response.</param>
    /// <param name="answerStyle">An optional parameter indicating the stylistic approach to be used when generating the answer.</param>
    /// <param name="groundingSource">An optional parameter specifying additional grounding content that can provide context or relevance to the generated answer.</param>
    /// <param name="safetySettings">An optional collection of rules or configurations applied to ensure safety during the answer generation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous operation.</param>
    /// <returns>Returns a <see cref="GenerateAnswerResponse"/> object containing the generated response and associated metadata.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    public async Task<GenerateAnswerResponse> GenerateAnswerAsync(string prompt,
        string corpusId,
        AnswerStyle answerStyle = AnswerStyle.ABSTRACTIVE,
        ICollection<SafetySetting>? safetySettings = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateAnswerRequest();
        var content = new Content(prompt, Roles.User);
        request.AddContent(content);

        request.SemanticRetriever = new SemanticRetrieverConfig()
        {
            Source = corpusId,
            Query = content
        };

        request.AnswerStyle = answerStyle;
        request.SafetySettings = safetySettings == null ? this.SafetySettings : safetySettings.ToList();
        return await GenerateAnswerAsync(request, cancellationToken).ConfigureAwait(false);
    }
    
    
}