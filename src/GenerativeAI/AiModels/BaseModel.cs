using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// The BaseModel class is an abstract representation of the model interactions in the GenerativeAI library.
/// It serves as a foundational implementation that enables derived classes to interact with the GenerativeAI platform.
/// This class inherits from BaseClient. It provides protected methods for use by derived classes,
/// allowing them to handle content generation, token management, and content embedding functionalities.
/// BaseModel is designed for flexible extension, enabling the development of specific AI model implementations.
/// </summary>
public abstract class BaseModel : BaseClient
{
    /// <summary>
    /// Abstract base class for models that interact with generative AI functionalities, providing common implementation details
    /// for generating content, embedding content, and token counting operations.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="BaseClient"/> and serves as a foundational class for specific models like
    /// embedding or generative models. It provides core functionality for handling requests to the generative AI
    /// services, including streaming and non-streaming content generation, token counting, and content embedding.
    /// </remarks>
    protected BaseModel(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger = null) : base(platform,
        httpClient, logger)
    {
    }

    /// <summary>
    /// Checks the response from the generative AI model to determine if the response is blocked or invalid and throws an exception if so.
    /// </summary>
    /// <param name="response">The <see cref="GenerateContentResponse"/> received from the generative AI model, which may contain content candidates.</param>
    /// <param name="url">The URL of the request made to the generative AI model, used for error reporting.</param>
    /// <exception cref="GenerativeAIException">Thrown if the response is blocked or invalid with details of the error.</exception>
    protected void CheckBlockedResponse(GenerateContentResponse? response, string url)
    {
        if (!(response.Candidates is { Length: > 0 }))
        {
            var blockErrorMessage = ResponseHelper.FormatBlockErrorMessage(response);
            if (!string.IsNullOrEmpty(blockErrorMessage))
            {
                throw new GenerativeAIException(
                    $"Error while requesting {url.MaskApiKey()}:\r\n\r\n{blockErrorMessage}",
                    blockErrorMessage);
            }
        }
    }
    
    /// <summary>
    /// Generates a model response given an input <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="model">The name of the <see cref="Model">Generative AI Model</see> to use for generating the completion.  Format: `models/{model}`.</param>
    /// <param name="request">The <see cref="GenerateContentRequest"/> containing the content of the current conversation with the model.</param>
    /// <returns>The <see cref="GenerateContentResponse"/> containing the model's response.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>
    protected virtual async Task<GenerateContentResponse> GenerateContentAsync(string model, GenerateContentRequest request)
    {
        var url = $"{_platform.GetBaseUrl()}/{model.ToModelId()}:{GenerativeModelTasks.GenerateContent}";

        var response = await SendAsync<GenerateContentRequest, GenerateContentResponse>(url, request, HttpMethod.Post).ConfigureAwait(false);
        CheckBlockedResponse(response, url);
        return response;
    }

    /// <summary>
    /// Streams model responses chunk by chunk given an input <see cref="GenerateContentRequest"/>.
    /// </summary>
    /// <param name="model">
    /// The name of the <see cref="Model">Generative AI Model</see> to use for generating
    /// the completion. Format: "models/{model}".
    /// </param>
    /// <param name="request">
    /// The <see cref="GenerateContentRequest"/> containing content of the current conversation with the model.
    /// </param>
    /// <param name="cancellationToken">Token for cancelling the streaming process.</param>
    /// <returns>An async stream of <see cref="GenerateContentResponse"/> items.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/text-generation">See Official API Documentation</seealso>
    protected virtual async IAsyncEnumerable<GenerateContentResponse> GenerateContentStreamAsync(
        string model,
        GenerateContentRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var url = $"{_platform.GetBaseUrl()}/{model.ToModelId()}:{GenerativeModelTasks.StreamGenerateContent}";
       
        await foreach (var response in StreamAsync<GenerateContentRequest, GenerateContentResponse>(url, request, cancellationToken).ConfigureAwait(false))
            yield return response;
    }

    /// <summary>
    /// Counts the number of tokens in a given input using the provided model and request.
    /// </summary>
    /// <param name="model">The name of the <see cref="Model">Generative AI Model</see> to use for counting tokens. Format: `models/{model}`.</param>
    /// <param name="request">The <see cref="CountTokensRequest"/> containing the input data for token counting.</param>
    /// <returns>The <see cref="CountTokensResponse"/> containing details about the token count.</returns>
    /// <seealso href="https://ai.google.dev/api/tokens">See Official API Documentation</seealso>
    protected virtual async Task<CountTokensResponse> CountTokensAsync(string model, CountTokensRequest request)
    {
        var url = $"{_platform.GetBaseUrl()}/{model.ToModelId()}:{GenerativeModelTasks.CountTokens}";
        return await SendAsync<CountTokensRequest, CountTokensResponse>(url, request, HttpMethod.Post).ConfigureAwait(false);
    }
    
 
     /// <summary>
    /// Embeds a batch of content using the specified <see cref="Model">Generative AI model</see>.
    /// </summary>
    /// <param name="model">
    /// The name of the <see cref="Model">Generative AI Model</see> to use for embedding the content.
    /// Format: "models/{model}".
    /// </param>
    /// <param name="request">
    /// The batch <see cref="BatchEmbedContentRequest"/> containing the content and optional settings for embedding.
    /// </param>
    /// <returns>The <see cref="BatchEmbedContentsResponse"/> containing the content embeddings.</returns>
    /// <seealso href="https://ai.google.dev/api/embeddings">See Official API Documentation</seealso>
    protected virtual async Task<BatchEmbedContentsResponse> BatchEmbedContentAsync(string model, BatchEmbedContentRequest request)
    {
        var url = $"{_platform.GetBaseUrl()}/{model.ToModelId()}:{GenerativeModelTasks.BatchEmbedContents}";
        foreach (var req in request.Requests)
        {
            ValidateEmbeddingRequest(model,req);
        }
        return await SendAsync<BatchEmbedContentRequest, BatchEmbedContentsResponse>(url, request, HttpMethod.Post).ConfigureAwait(false);
    }

    private void ValidateEmbeddingRequest(string model, EmbedContentRequest req)
    {
        req.Model = req.Model?? model;
        
        if(!SupportedEmbedingModels.All.Contains(req.Model)) throw new NotSupportedException($"Model {req.Model} is not supported for embedding.");
        
        if (!string.IsNullOrEmpty(req.Title) && req.TaskType != TaskType.RETRIEVAL_DOCUMENT) throw new NotSupportedException("A title can only be specified for tasks of type 'RETRIEVAL_DOCUMENT'.");

    }


    /// <summary>
    /// Embeds content using the specified <see cref="Model">Generative AI Model</see>.
    /// </summary>
    /// <param name="model">
    /// The name of the <see cref="Model">Generative AI Model</see> to use for embedding the content.
    /// Format: "models/{model}".
    /// </param>
    /// <param name="request">
    /// The <see cref="EmbedContentRequest"/> containing the content and optional settings for embedding.
    /// </param>
    /// <returns>The <see cref="EmbedContentResponse"/> containing the generated text embedding vector.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/embeddings">See Official API Documentation</seealso>
    protected virtual async Task<EmbedContentResponse> EmbedContentAsync(string model, EmbedContentRequest request)
    {
        var url = $"{_platform.GetBaseUrl()}/{model.ToModelId()}:{GenerativeModelTasks.EmbedContent}";
        ValidateEmbeddingRequest(model,request); 
        return await SendAsync<EmbedContentRequest, EmbedContentResponse>(url, request, HttpMethod.Post).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates a grounded answer from the model given an input <see cref="GenerateAnswerRequest"/>.
    /// </summary>
    /// <param name="model">The name of the <see cref="Model"/> to use for generating the grounded response. Format: <c>models/{model}</c>.</param>
    /// <param name="request">The <see cref="GenerateAnswerRequest"/> containing the input data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The <see cref="GenerateAnswerResponse"/> containing the model's answer.</returns>
    /// <seealso href="https://ai.google.dev/gemini-api/docs/question_answering#method:-models.generateanswer">See Official API Documentation</seealso>
    protected async Task<GenerateAnswerResponse> GenerateAnswerAsync(string model, GenerateAnswerRequest request,CancellationToken cancellationToken=default)
    {
        var url = $"{_platform.GetBaseUrl()}/{model.ToModelId()}:{GenerativeModelTasks.GenerateAnswer}";
     
        return await SendAsync<GenerateAnswerRequest, GenerateAnswerResponse>(url, request, HttpMethod.Post, cancellationToken).ConfigureAwait(false);
    }
}