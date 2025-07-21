﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>
    public virtual async Task<GenerateContentResponse> GenerateContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        PrepareRequest(request);
        // Use the base class method to get the raw response
        var baseResponse = await base.GenerateContentAsync(Model, request).ConfigureAwait(false);

        // Attempt to call functions if instructed
        return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates content asynchronously based on the specified text input prompt.
    /// </summary>
    /// <param name="prompt">The text input used to generate the content.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated content response.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>

    public async Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();
        request.AddText(prompt, false);

        return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);

      //  return await CallFunctionAsync(request, baseResponse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates content asynchronously based on the provided text prompt and a remote file URI.
    /// </summary>
    /// <param name="prompt">The input text prompt used for content generation.</param>
    /// <param name="fileUri">The URI of the file to be included in the content generation process.</param>
    /// <param name="mimeType">The MIME type of the file referenced by the URI.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> object with the generated content.</returns>
    public async Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        string fileUri,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();

        request.AddContent(new Content() { Role = Roles.User });
        //await AppendFile(filePath, request,cancellationToken);
        request.AddRemoteFile(fileUri, mimeType);
        request.AddText(prompt);


        return await GenerateContentAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generate content based on a sequence of Part objects.
    /// </summary>
    /// <param name="parts">The sequence of Part objects to be used for generating content.</param>
    /// <param name="cancellationToken">A token for cancelling the operation, if needed.</param>
    /// <returns>Returns an instance of GenerateContentResponse containing the generated content.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>

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
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation?lang=rest#generate-a-text-stream">See Official API Documentation</seealso>

    public virtual async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        GenerateContentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        PrepareRequest(request);

        GenerateContentResponse? lastResponse = null;
        await foreach (var streamedItem in base.GenerateContentStreamAsync(Model, request, cancellationToken).ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            lastResponse = streamedItem;
            yield return streamedItem;
        }
        if (lastResponse != null)
        {
            if (lastResponse.GetFunctions() != null)
            {
                await foreach (var relastResponse in CallFunctionStreamingAsync(request,lastResponse,cancellationToken).ConfigureAwait(false))
                {
                    yield return relastResponse;
                }
            }
        }
    }

    /// <summary>
    /// Streams content generation based on a string message.
    /// </summary>
    /// <param name="prompt">The input prompt to be processed and streamed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of generated content responses.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation?lang=rest#generate-a-text-stream">See Official API Documentation</seealso>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(RequestExtensions.FormatGenerateContentInput(prompt));

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (streamedItem?.Candidates == null) continue;

            yield return streamedItem;
        }
    }

    /// <summary>
    /// Streams content generation asynchronously based on the given input prompt, file URI, and MIME type.
    /// </summary>
    /// <param name="prompt">The input string used as a prompt for generating and streaming content.</param>
    /// <param name="fileUri">The URI of the file to be used for content generation.</param>
    /// <param name="mimeType">The MIME type associated with the provided file URI.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of <see cref="GenerateContentResponse"/> instances representing the streamed content responses.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/vision">See Official Vision API Documentation</seealso>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/audio">See Official Audio API Documentation</seealso>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        string fileUri,
        string mimeType,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest();

        request.AddContent(new Content() { Role = Roles.User });
        //await AppendFile(filePath, request, cancellationToken);
        request.AddRemoteFile(fileUri, mimeType);
        request.AddText(prompt);

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
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
    /// <param name="parts">The Parts containing input parameters for the content generation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of GenerateContentResponse objects representing the generated content.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        IEnumerable<Part> parts,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(RequestExtensions.FormatGenerateContentInput(parts));

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
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
    /// <param name="contents">The Contents containing the parameters for content generation.</param>
    /// <param name="cancellationToken">Optional token to cancel the streaming operation.</param>
    /// <returns>An asynchronous enumerable of GenerateContentResponse objects.</returns>
    public async IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        IEnumerable<Content> contents,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new GenerateContentRequest(contents.ToList());

        await foreach (var streamedItem in StreamContentAsync(request, cancellationToken).ConfigureAwait(false))
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
    /// <param name="cancellationToken">Token to monitor for cancellation requests during the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with the token count details.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(CountTokensRequest request,
        CancellationToken cancellationToken = default)
    {
        return await base.CountTokensAsync(Model, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously counts the tokens in the provided contents.
    /// </summary>
    /// <param name="contents">A collection of <see cref="Content"/> objects for which the token count needs to be determined.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the token count for the provided contents.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(IEnumerable<Content> contents,
        CancellationToken cancellationToken = default)
    {
        var request = new CountTokensRequest { Contents = contents.ToList() };

        return await base.CountTokensAsync(Model, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously counts the tokens in the provided collection of parts.
    /// </summary>
    /// <param name="parts">A collection of <see cref="Part"/> objects representing the input data for token counting.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the token counting results.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(IEnumerable<Part> parts,
        CancellationToken cancellationToken = default)
    {
        var request = new CountTokensRequest
            { Contents = new List<Content> { RequestExtensions.FormatGenerateContentInput(parts) } };

        return await base.CountTokensAsync(Model, request,cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Counts the number of tokens in the content based on the provided <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="generateContentRequest">An instance of <see cref="GenerateContentRequest"/> containing the input data for which the token count is calculated.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with token count details.</returns>
    public async Task<CountTokensResponse> CountTokensAsync(GenerateContentRequest generateContentRequest, CancellationToken cancellationToken = default)
    {
       
        var request = new CountTokensRequest { GenerateContentRequest = new GenerateContentRequestForCountToken(Model.ToModelId(),generateContentRequest) };

        return await base.CountTokensAsync(Model, request, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #endregion
}