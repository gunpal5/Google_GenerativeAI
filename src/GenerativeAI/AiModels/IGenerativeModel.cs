using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Defines the contract for a generative AI model. This interface provides methods for generating
/// answers and content, streaming content, and counting tokens within inputs or outputs.
/// It also includes properties for configuring behavior related to grounding, external tools,
/// and advanced options such as JSON mode or function calling.
/// </summary>
public interface IGenerativeModel
{
    /// <summary>
    /// Determines whether grounding is enabled.
    /// </summary>
    bool UseGrounding { get; set; }

    /// <summary>
    /// Specifies whether the Google Search integration is enabled.
    /// </summary>
    bool UseGoogleSearch { get; set; }

    /// <summary>
    /// Indicates whether the code execution tool is enabled.
    /// </summary>
    bool UseCodeExecutionTool { get; set; }

    /// <summary>
    /// Determines whether JSON mode is enabled. Incompatible with grounding, Google Search, and code execution tools.
    /// </summary>
    bool UseJsonMode { get; set; }

    /// <summary>
    /// Defines the behavior for function calling within the generative model.
    /// </summary>
    public FunctionCallingBehaviour FunctionCallingBehaviour { get; set; }

    /// <summary>
    /// Generates an answer asynchronously based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the input details for generating an answer.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Returns a <see cref="GenerateAnswerResponse"/> containing the generated answer and additional context.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    Task<GenerateAnswerResponse> GenerateAnswerAsync(GenerateAnswerRequest request,
        CancellationToken cancellationToken = default);
    

    /// <summary>
    /// Generates an answer asynchronously based on the given prompt and specified parameters.
    /// </summary>
    /// <param name="prompt">The text input that serves as the basis for generating a response.</param>
    /// <param name="answerStyle">An optional parameter indicating the stylistic approach to be used when generating the answer.</param>
    /// <param name="safetySettings">An optional collection of rules or configurations applied to ensure safety during the answer generation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous operation.</param>
    /// <returns>Returns a <see cref="GenerateAnswerResponse"/> object containing the generated response and associated metadata.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    Task<GenerateAnswerResponse> GenerateAnswerAsync(string prompt,
        AnswerStyle answerStyle = AnswerStyle.ABSTRACTIVE,
        ICollection<SafetySetting>? safetySettings = null,
        CancellationToken cancellationToken = default);
    
    

    #region Generate Content

    /// <summary>
    /// Generates content asynchronously based on the given input parameters.
    /// </summary>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> with the generated content.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>
    Task<GenerateContentResponse> GenerateContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates content asynchronously based on the specified text input prompt.
    /// </summary>
    /// <param name="prompt">The text input used to generate the content.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated content response.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>

    Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates content asynchronously based on the provided text prompt and an inline file path.
    /// </summary>
    /// <param name="prompt">The input text prompt used for generating content.</param>
    /// <param name="fileUri">The URI to an file that should be included in the content generation request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="GenerateContentResponse"/> or null if content generation fails.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/vision">See Official API Documentation For Vision Capabilities</seealso>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/audio">See Official API Documentation For Audio Understanding</seealso>

    Task<GenerateContentResponse> GenerateContentAsync(
        string prompt,
        string fileUri,
        string mimeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate content based on a sequence of Part objects.
    /// </summary>
    /// <param name="parts">The sequence of Part objects to be used for generating content.</param>
    /// <param name="cancellationToken">A token for cancelling the operation, if needed.</param>
    /// <returns>Returns an instance of GenerateContentResponse containing the generated content.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>

    Task<GenerateContentResponse> GenerateContentAsync(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default);

    #endregion

    #region Stream Content

    /// <summary>
    /// Streams content generation results asynchronously based on a given request and optional cancellation token.
    /// </summary>
    /// <param name="request">The content generation request containing input configurations and options.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of <see cref="GenerateContentResponse"/> objects containing the streamed generation results.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation?lang=rest#generate-a-text-stream">See Official API Documentation</seealso>

    IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams content generation based on a string message.
    /// </summary>
    /// <param name="prompt">The input prompt to be processed and streamed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of generated content responses.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation?lang=rest#generate-a-text-stream">See Official API Documentation</seealso>
    IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        CancellationToken cancellationToken = default);

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
    IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        string prompt,
        string fileUri,
        string mimeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams content generation based on a GenerateContentRequest asynchronously.
    /// </summary>
    /// <param name="parts">The Parts containing input parameters for the content generation process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous stream of GenerateContentResponse objects representing the generated content.</returns>
    IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams generated content as an asynchronous enumerable based on the provided request.
    /// </summary>
    /// <param name="contents">The contents containing the parameters for content generation.</param>
    /// <param name="cancellationToken">Optional token to cancel the streaming operation.</param>
    /// <returns>An asynchronous enumerable of GenerateContentResponse objects.</returns>
    IAsyncEnumerable<GenerateContentResponse> StreamContentAsync(
        IEnumerable<Content> contents,
        CancellationToken cancellationToken = default);

    #endregion

    #region Count Async

    /// <summary>
    /// Counts tokens asynchronously based on the provided request.
    /// </summary>
    /// <param name="request">An instance of <see cref="CountTokensRequest"/> containing the details of contents or generation parameters for counting tokens.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with the token count details.</returns>
    Task<CountTokensResponse> CountTokensAsync(CountTokensRequest request);

    /// <summary>
    /// Asynchronously counts the tokens in the given contents.
    /// </summary>
    /// <param name="contents">A collection of <see cref="Content"/> objects representing the input for which tokens need to be counted.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the resulting token count.</returns>
    Task<CountTokensResponse> CountTokensAsync(IEnumerable<Content> contents);

    /// <summary>
    /// Asynchronously counts the tokens in the provided collection of parts.
    /// </summary>
    /// <param name="parts">A collection of <see cref="Part"/> objects representing the input data for token counting.</param>
    /// <returns>A task representing the asynchronous operation, containing the <see cref="CountTokensResponse"/> with the token counting results.</returns>
    Task<CountTokensResponse> CountTokensAsync(IEnumerable<Part> parts);

    /// <summary>
    /// Counts the number of tokens in the content based on the provided <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="generateContentRequest">An instance of <see cref="GenerateContentRequest"/> containing the input data for which the token count is calculated.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="CountTokensResponse"/> with token count details.</returns>
    Task<CountTokensResponse> CountTokensAsync(GenerateContentRequest generateContentRequest);


    #endregion

    #region Generate Object

    /// <summary>
    /// Generates content asynchronously using JSON mode based on the given input parameters and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to send as JSON schema input in request.</typeparam>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated content response of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    Task<GenerateContentResponse> GenerateContentAsync<T>(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Generates content asynchronously using JSON mode based on the given input parameters and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to convert the resulting JSON into.</typeparam>
    /// <param name="request">An instance of <see cref="GenerateContentRequest"/> containing the input configuration and settings for content generation.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated object of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    Task<T?> GenerateObjectAsync<T>(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Generates content asynchronously using JSON mode based on the specified text input prompt and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to convert the resulting JSON into.</typeparam>
    /// <param name="prompt">The text input used to generate the content.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the generated object of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    Task<T?> GenerateObjectAsync<T>(
        string prompt,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Generates content asynchronously using JSON mode based on the specified content generation request parts and converts the resulting JSON into a C# object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of C# object to convert the resulting JSON into.</typeparam>
    /// <param name="parts">An enumerable containing the input parts used for generating the content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the generated object of type <typeparamref name="T"/>.</returns>
    /// <remarks>Some of the complex data types are not supported such as Dictionary. So make sure to avoid these.</remarks>
    Task<T?> GenerateObjectAsync<T>(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default) where T : class;

    #endregion
}