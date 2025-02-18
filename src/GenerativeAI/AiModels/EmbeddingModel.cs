using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// Represents a machine learning model for generating embeddings from input text or content.
/// This model allows handling of single or batch embedding operations using various input types or formats.
/// Inherits from the <see cref="BaseModel"/> class.
/// </summary>
public class EmbeddingModel : BaseModel
{
    /// <summary>
    /// Gets or sets the name or identifier of the embedding model to be used for generating embeddings.
    /// The model name typically represents a specific configuration or version of the machine learning model.
    /// This property is utilized in embedding operations to define which model to apply for processing the input.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// A class that represents an embedding model used for generating vector embeddings
    /// from input content such as text or files. This model supports both single and batch
    /// operations for processing multiple inputs and generates high-dimensional embeddings
    /// compatible with various downstream applications.
    /// </summary>
    /// <remarks>
    /// This class provides multiple overloads of the `EmbedContentAsync` method to handle different
    /// input types, including strings, content objects, and batch requests. It is designed to be flexible
    /// for integration with machine learning workflows.
    /// </remarks>
    public EmbeddingModel(IPlatformAdapter platform, string model, HttpClient? httpClient = null , ILogger? logger = null) : base(platform, httpClient, logger)
    {
        this.Model = model;
    }

    /// <summary>
    /// A class that represents an embedding model designed to generate vector representations (embeddings)
    /// from input text or content. Supports operations for both single inputs and batch processing using
    /// multiple input types.
    /// </summary>
    /// <remarks>
    /// The class provides several overloads of the `EmbedContentAsync` method to allow flexibility in embedding
    /// generation based on the input structure, such as individual strings, content objects, or collections.
    /// It also supports batch operations through `BatchEmbedContentAsync` methods for efficiency in processing
    /// multiple inputs simultaneously. This model integrates functionality inherited from the `BaseModel` class.
    /// </remarks>
    public EmbeddingModel(string apiKey, string model, HttpClient? httpClient = null , ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey), model, httpClient, logger)
    {
    }

    /// <summary>
    /// Embeds content based on a text input message.
    /// </summary>
    /// <param name="content">The content object to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse> EmbedContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        
        return await EmbedContentAsync(new EmbedContentRequest(){Content = content},cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on the provided text input message.
    /// </summary>
    /// <param name="request">The request object to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse> EmbedContentAsync(
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
    public async Task<EmbedContentResponse> EmbedContentAsync(
        string message,
        CancellationToken cancellationToken = default)
    {

        return await EmbedContentAsync(RequestExtensions.FormatGenerateContentInput(message) ,cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on a sequence of Part objects.
    /// </summary>
    /// <param name="parts">The sequence of Part objects to be used for embedding content.</param>
    /// <param name="cancellationToken">A token for cancelling the operation, if needed.</param>
    /// <returns>Returns an instance of EmbedContentResponse containing the embeddings.</returns>
    public async Task<EmbedContentResponse> EmbedContentAsync(
        IEnumerable<Part> parts,
        CancellationToken cancellationToken = default)
    {
        return await EmbedContentAsync(RequestExtensions.FormatGenerateContentInput(parts),cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Embeds content based on a collection of strings.
    /// </summary>
    /// <param name="messages">The collection of text strings to generate embeddings from.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the embedding response.</returns>
    public async Task<EmbedContentResponse> EmbedContentAsync(
        IEnumerable<string> messages,
        CancellationToken cancellationToken = default)
    {

        return await EmbedContentAsync(RequestExtensions.FormatGenerateContentInput(messages),cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Embeds a batch of content based on a collection of <see cref="Content"/> objects.
    /// </summary>
    /// <param name="requests">The collection of <see cref="EmbedContentRequest"/> requests to embed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the batch embedding response.</returns>
    public async Task<BatchEmbedContentsResponse> BatchEmbedContentAsync(
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
    public async Task<BatchEmbedContentsResponse> BatchEmbedContentAsync(
        IEnumerable<Content> contents,
        CancellationToken cancellationToken = default)
    {
        var requests = contents.Select(c => new EmbedContentRequest()
        {
            Content = c,
            Model = Model.ToModelId()
        });
        
        return await BatchEmbedContentAsync(requests,cancellationToken).ConfigureAwait(false);
    }
    
   
}