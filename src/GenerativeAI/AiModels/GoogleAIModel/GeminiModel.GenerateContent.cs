using System.Runtime.CompilerServices;
using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GeminiModel
{
    #region GenerateContent

    /// <summary>
    /// Generates content asynchronously based on the given text prompt and the specified file path.
    /// </summary>
    /// <param name="prompt">The textual input used as the basis for content generation.</param>
    /// <param name="filePath">The path to the file that should be included in the content generation request.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="GenerateContentResponse"/> or null if the operation fails.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/vision">See Official Vision API Documentation</seealso>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/audio">See Official Audio API Documentation</seealso>

    public async Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();

        request.AddContent(new Content() { Role = Roles.User });
        await AppendFile(filePath, request, cancellationToken).ConfigureAwait(false);

        request.AddText(prompt);


        return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region StreamContent

    /// <summary>
    /// Streams content generation asynchronously based on the provided text prompt, file path, and cancellation token.
    /// </summary>
    /// <param name="prompt">The input text prompt used to guide the content generation process.</param>
    /// <param name="filePath">The local file path</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the streaming process.</param>
    /// <returns>An asynchronous enumerable of <see cref="GenerateContentResponse"/> instances representing the streamed content generation results.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/vision">See Official Vision API Documentation</seealso>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/audio">See Official Audio API Documentation</seealso>

    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        string filePath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();

        request.AddContent(new Content() { Role = Roles.User });
        
        await AppendFile(filePath, request, cancellationToken).ConfigureAwait(false);
        

        request.AddText(prompt);

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    #endregion

   
}