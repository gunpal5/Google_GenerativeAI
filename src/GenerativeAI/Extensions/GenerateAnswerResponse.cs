using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for <see cref="GenerateAnswerResponse"/> objects.
/// </summary>
public static class GenerateAnswerResponseExtension
{
    /// <summary>
    /// Extracts the answer text from a GenerateAnswerResponse.
    /// </summary>
    /// <param name="response">The response to extract the answer from.</param>
    /// <returns>The answer text joined by newlines.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the response or answer is null.</exception>
    public static string GetAnswer(this GenerateAnswerResponse? response)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        if(response==null)
            throw new ArgumentNullException(nameof(response));
#endif
        if(response.Answer == null)
            throw new InvalidOperationException("Response answer cannot be null.");
        if(response.Answer.Content == null)
            return string.Empty;
        return string.Join("\r\n", response.Answer.Content.Parts.Select(s => s.Text));
    }
}