using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for the GenerateContentResponse type in the GenerativeAI.Types namespace.
/// </summary>
public static class GenerateContentResponseExtensions
{
    /// <summary>
    /// Returns the text of the first candidate’s first content part.
    /// </summary>
    /// <param name="response">The GenerateContentResponse to process.</param>
    /// <returns>The text if found; otherwise null.</returns>
    public static string? Text(this GenerateContentResponse response)
    {
        return response?.Candidates?[0].Content?.Parts?[0].Text;
    }

    /// <summary>
    /// Returns the first candidate’s first function call.
    /// </summary>
    /// <param name="response">The GenerateContentResponse to process.</param>
    /// <returns>The function call if found; otherwise null.</returns>
    public static FunctionCall? GetFunction(this GenerateContentResponse response)
    {
        return response?.Candidates?[0].Content?.Parts?[0].FunctionCall;
    }
}
