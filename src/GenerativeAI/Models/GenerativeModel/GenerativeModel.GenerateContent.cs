using System.Diagnostics;
using GenerativeAI.Types;

namespace GenerativeAI;

public partial class GenerativeModel
{
    #region Public Methods

    #region Generate Content

    /// <summary>
    /// Generates content asynchronously based on the given input parameters.
    /// </summary>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> with the generated content.</returns>
    public virtual async Task<GenerateContentResponse> GenerateContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        PrepareRequest(request);
        // Use the base class method to get the raw response
        var baseResponse = await base.GenerateContentAsync(Model, request);


        // Attempt to call functions if instructed
        return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates content asynchronously based on the specified text input prompt.
    /// </summary>
    /// <param name="prompt">The text input used to generate the content.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated content response.</returns>
    public async Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();
        request.AddText(prompt, false);

        var baseResponse = await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);

        return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates content asynchronously based on the provided text prompt and an inline file path.
    /// </summary>
    /// <param name="prompt">The input text prompt used for generating content.</param>
    /// <param name="filePath">The path to an file that should be included in the content generation request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> or null if content generation fails.</returns>
    public async Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();

        request.AddContent(new Content(){Role = Roles.User});
        await AppendFile(filePath, request,cancellationToken);
        
        request.AddText(prompt);

        return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generate content based on a sequence of Part objects.
    /// </summary>
    /// <param name="parts">The sequence of Part objects to be used for generating content.</param>
    /// <param name="cancellationToken">A token for cancelling the operation, if needed.</param>
    /// <returns>Returns an instance of GenerateContentResponse containing the generated content.</returns>
    public async Task<GenerateContentResponse> GenerateContentAsync(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest
        {
            Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) }.ToList()
        };

        return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    }

    #endregion
    
    #region Generate Object
    // /// <summary>
    // /// Generates content asynchronously based on the given input parameters.
    // /// </summary>
    // /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    // /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    // /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> with the generated content.</returns>
    // public virtual async Task<T> GenerateObjectAsync<T>(
    //     GenerateContentRequest request,
    //     CancellationToken cancellationToken = default) where T : class
    // {
    //     request.UseJsonMode<T>();
    // }

    // /// <summary>
    // /// Generates content asynchronously based on the specified text input prompt.
    // /// </summary>
    // /// <param name="prompt">The text input used to generate the content.</param>
    // /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    // /// <returns>A task representing the asynchronous operation, containing the generated content response.</returns>
    // public async Task<GenerateContentResponse> GenerateContentAsync(
    //     string prompt,
    //     CancellationToken cancellationToken = default)
    // {
    //     var request = new GenerateContentRequest();
    //     request.AddText(prompt, false);
    //
    //     var baseResponse = await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    //
    //     return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
    // }
    //
    // /// <summary>
    // /// Generates content asynchronously based on the provided text prompt and an inline file path.
    // /// </summary>
    // /// <param name="prompt">The input text prompt used for generating content.</param>
    // /// <param name="filePath">The path to an file that should be included in the content generation request.</param>
    // /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    // /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> or null if content generation fails.</returns>
    // public async Task<GenerateContentResponse> GenerateContentAsync(
    //     string prompt,
    //     string filePath,
    //     CancellationToken cancellationToken = default)
    // {
    //     var request = new GenerateContentRequest();
    //
    //     request.AddContent(new Content(){Role = Roles.User});
    //     await AppendFile(filePath, request,cancellationToken);
    //     
    //     request.AddText(prompt);
    //
    //     return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    // }
    //
    // /// <summary>
    // /// Generate content based on a sequence of Part objects.
    // /// </summary>
    // /// <param name="parts">The sequence of Part objects to be used for generating content.</param>
    // /// <param name="cancellationToken">A token for cancelling the operation, if needed.</param>
    // /// <returns>Returns an instance of GenerateContentResponse containing the generated content.</returns>
    // public async Task<GenerateContentResponse> GenerateContentAsync(
    //     IEnumerable<Part> parts,
    //     CancellationToken cancellationToken = default)
    // {
    //     var request = new GenerateContentRequest
    //     {
    //         Contents = new[] { RequestExtensions.FormatGenerateContentInput(parts) }.ToList()
    //     };
    //
    //     return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    // }
    #endregion

    #region Stream Content

    /// <summary>
    /// Streams content generation results asynchronously based on a given request and optional cancellation token.
    /// </summary>
    /// <param name="request">The content generation request containing input configurations and options.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of <see cref="GenerateContentResponse"/> objects containing the streamed generation results.</returns>
    public virtual async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        PrepareRequest(request);

        await foreach (var streamedItem in base.GenerateContentStreamAsync(Model, request, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    /// <summary>
    /// Streams content generation based on a string message.
    /// </summary>
    /// <param name="prompt">The input prompt to be processed and streamed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of generated content responses.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(RequestExtensions.FormatGenerateContentInput(prompt));

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    /// <summary>
    /// Streams content generation asynchronously based on the provided prompt and file path.
    /// </summary>
    /// <param name="prompt">The input string prompt used to generate and stream content.</param>
    /// <param name="filePath">The file path associated with the generation operation.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An asynchronous enumerable containing instances of <see cref="GenerateContentResponse"/> representing the streamed generated content.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();

        request.AddContent(new Content(){Role = Roles.User});
        await AppendFile(filePath, request, cancellationToken);
        request.AddText(prompt);

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    /// <summary>
    /// Streams content generation based on a GenerateContentRequest asynchronously.
    /// </summary>
    /// <param name="request">The GenerateContentRequest containing input parameters for the content generation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of GenerateContentResponse objects representing the generated content.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(RequestExtensions.FormatGenerateContentInput(parts));

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    /// <summary>
    /// Streams generated content as an asynchronous enumerable based on the provided request.
    /// </summary>
    /// <param name="request">The GenerateContentRequest containing the parameters for content generation.</param>
    /// <param name="cancellationToken">Optional token to cancel the streaming operation.</param>
    /// <returns>An asynchronous enumerable of GenerateContentResponse objects.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        IEnumerable<Content> contents,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(contents.ToList());

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    #endregion

    #region Count Async

    /// <summary>
    /// Counts tokens asynchronously based on the provided request.
    /// </summary>
    /// <param name="request">An instance of <see cref="CountTokensRequest"/> containing the details of contents or generation parameters for counting tokens.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with the token count details.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(CountTokensRequest request)
    {
        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously counts the tokens in the given contents.
    /// </summary>
    /// <param name="contents">A collection of <see cref="Content"/> objects representing the input for which tokens need to be counted.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the resulting token count.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(IEnumerable<Content> contents)
    {
        var request = new CountTokensRequest { Contents = contents.ToList() };

        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously counts the tokens in the provided collection of parts.
    /// </summary>
    /// <param name="parts">A collection of <see cref="Part"/> objects representing the input data for token counting.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the token counting results.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(IEnumerable<Part> parts)
    {
        var request = new CountTokensRequest
            { Contents = new List<Content> { RequestExtensions.FormatGenerateContentInput(parts) } };

        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Counts the number of tokens in the content based on the provided <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="generateContentRequest">An instance of <see cref="GenerateContentRequest"/> containing the input data for which the token count is calculated.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with token count details.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(GenerateContentRequest generateContentRequest)
    {
       
        var request = new CountTokensRequest { GenerateContentRequest = new GenerateContentRequestForCountToken(Model,generateContentRequest) };

        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    #endregion

    #endregion
}