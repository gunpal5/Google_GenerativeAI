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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null) throw new ArgumentNullException(nameof(request));
#endif
        if (request.AnswerStyle == AnswerStyle.ANSWER_STYLE_UNSPECIFIED)
            request.AnswerStyle = AnswerStyle.ABSTRACTIVE;

        if (request.InlinePassages == null && request.SemanticRetriever == null)
        {
            throw new ArgumentNullException(nameof(request.InlinePassages), "Grounding source is required. either InlinePassages or SemanticRetriever set.");
        }

        return await GenerateAnswerAsync(Model, request, cancellationToken).ConfigureAwait(false);
    }
}