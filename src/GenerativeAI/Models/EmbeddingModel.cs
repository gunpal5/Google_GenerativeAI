using GenerativeAI.Constants;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Models;

public class EmbeddingModel : BaseModel
{
    string Model { get; set; }
    public EmbeddingModel(IPlatformAdapter platform, string model, HttpClient? httpClient = null , ILogger? logger = null) : base(platform, httpClient, logger)
    {
        this.Model = model;
    }
    public EmbeddingModel(string apiKey, string model, HttpClient? httpClient = null , ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey), model, httpClient, logger)
    {
    }

    /// <summary>
    /// Embeds content based on a text input message.
    /// </summary>
    /// <param name="content">The content object to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse?> EmbedContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        
        return await EmbedContentAsync(new EmbedContentRequest(){Content = content}).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on the provided text input message.
    /// </summary>
    /// <param name="request">The request object to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse?> EmbedContentAsync(
        EmbedContentRequest request,
        CancellationToken cancellationToken = default)
    {
        return await EmbedContentAsync(Model, request).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on a text input message.
    /// </summary>
    /// <param name="message">The text input to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse?> EmbedContentAsync(
        string message,
        CancellationToken cancellationToken = default)
    {

        return await EmbedContentAsync(RequestExtensions.FormatGenerateContentInput(message) ).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on a sequence of Part objects.
    /// </summary>
    /// <param name="parts">The sequence of Part objects to be used for embedding content.</param>
    /// <param name="cancellationToken">A token for cancelling the operation, if needed.</param>
    /// <returns>Returns an instance of EmbedContentResponse containing the embeddings.</returns>
    public async Task<EmbedContentResponse?> EmbedContentAsync(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default)
    {
        return await EmbedContentAsync(RequestExtensions.FormatGenerateContentInput(parts)).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on a collection of strings.
    /// </summary>
    /// <param name="messages">The collection of text strings to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse?> EmbedContentAsync(
        IEnumerable<string> messages,
        CancellationToken cancellationToken = default)
    {

        return await EmbedContentAsync(RequestExtensions.FormatGenerateContentInput(messages)).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Embeds a batch of content based on a collection of <see cref="Content"/> objects.
    /// </summary>
    /// <param name="requests">The collection of <see cref="EmbedContentRequest"/> requests to embed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the batch embedding response.</returns>
    public async Task<BatchEmbedContentsResponse?> BatchEmbedContentAsync(
        IEnumerable<EmbedContentRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var request = new BatchEmbedContentRequest { Requests = requests.ToList()};
        return await BatchEmbedContentAsync(Model, request).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Embeds a batch of content based on a collection of <see cref="Content"/> objects.
    /// </summary>
    /// <param name="contents">The collection of <see cref="Content"/> objects to embed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the batch embedding response.</returns>
    public async Task<BatchEmbedContentsResponse?> BatchEmbedContentAsync(
        IEnumerable<Content> contents,
        CancellationToken cancellationToken = default)
    {
        var requests = contents.Select(c => new EmbedContentRequest()
        {
            Content = c,
            Model = Model.ToModelId()
        });
        
        return await BatchEmbedContentAsync(requests).ConfigureAwait(false);
    }
    
   
}