using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Clients;

/// <summary>
/// The CorpusManager class provides a client interface for managing corpora, documents, chunks, and permissions
/// within a generative AI platform. It supports creating, retrieving, listing, querying, and deleting entities
/// related to the corpus resources.
/// </summary>
public class CorporaManager : BaseClient
{
    /// <summary>
    /// Gets the client for performing operations on documents.
    /// </summary>
    public DocumentsClient DocumentsClient { get; private set; }

    /// <summary>
    /// Gets the client for managing corpora.
    /// </summary>
    public CorporaClient CorporaClient { get; private set; }

    /// <summary>
    /// Gets the client for handling chunks.
    /// </summary>
    public ChunkClient ChunkClient { get; private set; }

    /// <summary>
    /// Gets the client for managing permissions on corpora.
    /// </summary>
    public CorpusPermissionClient CorpusPermissionClient { get; private set; }

    public CorporaManager(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger = null) : base(platform,
        httpClient, logger)
    {
        if (platform.Authenticator == null)
            throw new GenerativeAIException("Google Authenticator is required for Corpus Manager to work. .",
                "Please provide an instance of GoogleAuthenticator to the constructor of Corpus Manager to work.");
        InitilizeClients();
    }

    /// <summary>
    /// Initializes the individual clients used by the semantic retriever model.
    /// </summary>
    private void InitilizeClients()
    {
        this.DocumentsClient = new DocumentsClient(this.Platform, this.HttpClient, this.Logger);
        this.CorporaClient = new CorporaClient(this.Platform, this.HttpClient, this.Logger);
        this.ChunkClient = new ChunkClient(this.Platform, this.HttpClient, this.Logger);
        this.CorpusPermissionClient = new CorpusPermissionClient(this.Platform, this.HttpClient, this.Logger);
    }

    #region Corpus Operations

    /// <summary>
    /// Creates a new corpus with the given display name.
    /// </summary>
    /// <param name="displayName">The display name of the corpus.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created corpus.</returns>
    public async Task<Corpus?> CreateCorpusAsync(string displayName, CancellationToken cancellationToken = default)
    {
        var corpus = new Corpus { DisplayName = displayName };
        return await CorporaClient.CreateCorpusAsync(corpus, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a corpus by its name.
    /// </summary>
    /// <param name="corpusName">The name of the corpus.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The corpus.</returns>
    public async Task<Corpus?> GetCorpusAsync(string corpusName, CancellationToken cancellationToken = default)
    {
        return await CorporaClient.GetCorpusAsync(corpusName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all corpora.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all corpora.</returns>
    public async Task<List<Corpus>?> ListCorporaAsync(CancellationToken cancellationToken = default)
    {
        var response = await CorporaClient.ListCorporaAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return response?.Corpora;
    }

    /// <summary>
    /// Deletes a corpus by its name.
    /// </summary>
    /// <param name="corpusName">The name of the corpus.</param>
    /// <param name="force">Whether to force delete the corpus.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteCorpusAsync(string corpusName, bool force = false,
        CancellationToken cancellationToken = default)
    {
        await CorporaClient.DeleteCorpusAsync(corpusName, force, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Document Operations

    /// <summary>
    /// Adds a document to a corpus with the given display name and optional metadata.
    /// </summary>
    /// <param name="corpusName">The name of the corpus to add the document to.</param>
    /// <param name="displayName">The display name of the document.</param>
    /// <param name="metadata">Optional metadata for the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created document.</returns>
    public async Task<Document?> AddDocumentAsync(string corpusName, string displayName,
        List<CustomMetadata>? metadata = null, CancellationToken cancellationToken = default)
    {
        var document = new Document { DisplayName = displayName, CustomMetadata = metadata };
        return await DocumentsClient.CreateDocumentAsync(corpusName, document, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a document by its name.
    /// </summary>
    /// <param name="documentName">The name of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The document.</returns>
    public async Task<Document?> GetDocumentAsync(string documentName, CancellationToken cancellationToken = default)
    {
        return await DocumentsClient.GetDocumentAsync(documentName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all documents in a corpus.
    /// </summary>
    /// <param name="corpusName">The name of the corpus.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all documents in the corpus.</returns>
    public async Task<List<Document>?> ListDocumentsAsync(string corpusName,
        CancellationToken cancellationToken = default)
    {
        var response = await DocumentsClient.ListDocumentsAsync(corpusName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response?.Documents;
    }

    /// <summary>
    /// Deletes a document by its name.
    /// </summary>
    /// <param name="documentName">The name of the document.</param>
    /// <param name="force">Whether to force delete the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteDocumentAsync(string documentName, bool force = false,
        CancellationToken cancellationToken = default)
    {
        await DocumentsClient.DeleteDocumentAsync(documentName, force, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Chunk Operations

    /// <summary>
    /// Adds a chunk to a document.
    /// </summary>
    /// <param name="documentName">The name of the document to add the chunk to.</param>
    /// <param name="chunk">The chunk to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created chunk.</returns>
    public async Task<Chunk?> AddChunkAsync(string documentName, Chunk chunk,
        CancellationToken cancellationToken = default)
    {
        return await ChunkClient.CreateChunkAsync(documentName, chunk, cancellationToken).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Adds a chunk to a document.
    /// </summary>
    /// <param name="documentName">The name of the document to add the chunk to.</param>
    /// <param name="chunks">The list of chunks to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created chunk.</returns>
    public async Task<List<Chunk>?> AddChunksAsync(string documentName, List<Chunk> chunks,
        CancellationToken cancellationToken = default)
    {
        var chunksResponsRequests = chunks.Select(chunk => new CreateChunkRequest() { Chunk = chunk, Parent = documentName })
            .ToList();
        var response = await ChunkClient.BatchCreateChunksAsync(documentName, chunksResponsRequests, cancellationToken).ConfigureAwait(false);
        if(response==null || response.Chunks==null || response.Chunks.Count!=chunks.Count)
            throw new GenerativeAIException("Failed to add chunks to document", "Failed to add chunks to document");
        return response.Chunks;
    }

    /// <summary>
    /// Gets a chunk by its name.
    /// </summary>
    /// <param name="chunkName">The name of the chunk.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The chunk.</returns>
    public async Task<Chunk?> GetChunkAsync(string chunkName, CancellationToken cancellationToken = default)
    {
        return await ChunkClient.GetChunkAsync(chunkName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all chunks in a document.
    /// </summary>
    /// <param name="documentName">The name of the document.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all chunks in the document.</returns>
    public async Task<List<Chunk>?> ListChunksAsync(string documentName, CancellationToken cancellationToken = default)
    {
        var response = await ChunkClient.ListChunksAsync(documentName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response?.Chunks;
    }

    /// <summary>
    /// Deletes a chunk by its name.
    /// </summary>
    /// <param name="chunkName">The name of the chunk.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteChunkAsync(string chunkName, CancellationToken cancellationToken = default)
    {
        await ChunkClient.DeleteChunkAsync(chunkName, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Permission Operations

    /// <summary>
    /// Creates a new permission for a corpus.
    /// </summary>
    /// <param name="corpusName">The name of the corpus.</param>
    /// <param name="permission">The permission to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created permission.</returns>
    public async Task<Permission?> CreateCorpusPermissionAsync(string corpusName, Permission permission,
        CancellationToken cancellationToken = default)
    {
        return await CorpusPermissionClient.CreatePermissionAsync(corpusName, permission, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a permission by its name.
    /// </summary>
    /// <param name="permissionName">The name of the permission.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The permission.</returns>
    public async Task<Permission?> GetPermissionAsync(string permissionName,
        CancellationToken cancellationToken = default)
    {
        return await CorpusPermissionClient.GetPermissionAsync(permissionName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists all permissions for a corpus.
    /// </summary>
    /// <param name="corpusName">The name of the corpus.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all permissions for the corpus.</returns>
    public async Task<List<Permission>?> ListCorpusPermissionsAsync(string corpusName,
        CancellationToken cancellationToken = default)
    {
        var response = await CorpusPermissionClient
            .ListPermissionsAsync(corpusName, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response?.Permissions;
    }

    /// <summary>
    /// Deletes a permission by its name.
    /// </summary>
    /// <param name="permissionName">The name of the permission.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeletePermissionAsync(string permissionName, CancellationToken cancellationToken = default)
    {
        await CorpusPermissionClient.DeletePermissionAsync(permissionName, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Querying Operations

    /// <summary>
    /// Performs a semantic search over a corpus.
    /// </summary>
    /// <param name="corpusName">The name of the corpus to query.</param>
    /// <param name="query">The query string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    public async Task<QueryCorpusResponse?> QueryCorpusAsync(string corpusName, string query,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryCorpusRequest { Query = query };
        return await CorporaClient.QueryCorpusAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs a semantic search over a document.
    /// </summary>
    /// <param name="documentName">The name of the document to query.</param>
    /// <param name="query">The query string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The query response.</returns>
    public async Task<QueryDocumentResponse?> QueryDocumentAsync(string documentName, string query,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryDocumentRequest { Query = query };
        return await DocumentsClient.QueryDocumentAsync(documentName, request, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}