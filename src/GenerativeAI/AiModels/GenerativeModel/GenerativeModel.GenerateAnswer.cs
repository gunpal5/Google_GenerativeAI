using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GenerativeModel
{
    /// <summary>
    /// Generates an answer asynchronously based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the input details for generating an answer.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Returns a <see cref="GenerateAnswerResponse"/> containing the generated answer and additional context.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    public async Task<GenerateAnswerResponse> GenerateAnswerAsync(GenerateAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        return await GenerateAnswerAsync(Model, request, cancellationToken);
    }

    /// <summary>
    /// Generates an answer asynchronously based on the given prompt and specified parameters.
    /// </summary>
    /// <param name="prompt">The text input that serves as the basis for generating a response.</param>
    /// <param name="answerStyle">An optional parameter indicating the stylistic approach to be used when generating the answer.</param>
    /// <param name="safetySettings">An optional collection of rules or configurations applied to ensure safety during the answer generation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous operation.</param>
    /// <returns>Returns a <see cref="GenerateAnswerResponse"/> object containing the generated response and associated metadata.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    public async Task<GenerateAnswerResponse> GenerateAnswerAsync(string prompt, AnswerStyle? answerStyle = null,
        ICollection<SafetySetting>? safetySettings = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateAnswerRequest();
        request.AddText(prompt);
        request.AnswerStyle = answerStyle ?? AnswerStyle.ANSWER_STYLE_UNSPECIFIED;
        request.SafetySettings = safetySettings == null ? this.SafetySettings : safetySettings.ToList();
        return await GenerateAnswerAsync(Model, request, cancellationToken);
    }
}