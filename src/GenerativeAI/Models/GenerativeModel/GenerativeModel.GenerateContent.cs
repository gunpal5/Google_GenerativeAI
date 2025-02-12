using GenerativeAI.Types;

namespace GenerativeAI.Models;

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
    public async Task<GenerateContentResponse> GenerateContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Ensure config and safety settings are not null
        request.GenerationConfig ??= Config;
        request.SafetySettings ??= SafetySettings;

        // Include function definitions if enabled
        AddTools(request);

        // Use the base class method to get the raw response
        var baseResponse = await base.GenerateContentAsync(Model, request);


        // Attempt to call functions if instructed
        return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates content based on a text input message.
    /// </summary>
    /// <param name="message">The text input to generate content from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated content response.</returns>
    public async Task<GenerateContentResponse?> GenerateContentAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest
        {
            Contents = new[] { RequestExtensions.FormatGenerateContentInput(message) }.ToList(),
        };

        var baseResponse = await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);

        return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
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

    #region Stream Content

    /// <summary>
    /// Streams content generation results asynchronously based on a given request and optional cancellation token.
    /// </summary>
    /// <param name="request">The content generation request containing input configurations and options.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of <see cref="GenerateContentResponse"/> objects containing the streamed generation results.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        request.GenerationConfig ??= Config;
        request.SafetySettings ??= SafetySettings;
        AddTools(request);

        await foreach (var streamedItem in base.GenerateContentStreamAsync(Model, request, cancellationToken))
        {
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    /// <summary>
    /// Streams content generation based on a string message.
    /// </summary>
    /// <param name="message">The input string message to be processed and streamed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of generated content responses.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(RequestExtensions.FormatGenerateContentInput(message));

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken))
        {
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
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    #endregion

    // /// <summary>
    // /// Start a Chat Session
    // /// </summary>
    // public virtual ChatSession StartChat(StartChatParams startSessionParams)
    // {
    //     return new ChatSession(this, startSessionParams);
    // }

    #region Count Async

    /// <summary>
    /// Counts tokens asynchronously based on the provided request.
    /// </summary>
    /// <param name="request">An instance of <see cref="CountTokensRequest"/> containing the details of contents or generation parameters for counting tokens.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with the token count details.</returns>
    public async Task<CountTokensResponse?> CountTokensAsync(CountTokensRequest request)
    {
        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously counts the tokens in the given contents.
    /// </summary>
    /// <param name="contents">A collection of <see cref="Content"/> objects representing the input for which tokens need to be counted.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the resulting token count.</returns>
    public async Task<CountTokensResponse?> CountTokensAsync(IEnumerable<Content> contents)
    {
        var request = new CountTokensRequest { Contents = contents.ToList() };

        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously counts the tokens in the provided collection of parts.
    /// </summary>
    /// <param name="parts">A collection of <see cref="Part"/> objects representing the input data for token counting.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the token counting results.</returns>
    public async Task<CountTokensResponse?> CountTokensAsync(IEnumerable<Part> parts)
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
    public async Task<CountTokensResponse?> CountTokensAsync(GenerateContentRequest generateContentRequest)
    {
       
        var request = new CountTokensRequest { GenerateContentRequest = new GenerateContentRequestForCountToken(Model,generateContentRequest) };

        return await base.CountTokensAsync(Model, request).ConfigureAwait(false);
    }

    #endregion

    #endregion
}