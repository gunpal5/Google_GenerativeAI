using System.Diagnostics;
using System.Text.Json;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// VertexRagManager provides management and interaction capabilities for RAG (Retrieval-Augmented Generation)
/// resources. It is designed to facilitate operations involving file management and RAG corpus resources.
/// This class serves as a specialized client built upon the BaseClient, leveraging shared functionality
/// like authorization handling and HTTP communication.
/// The manager provides direct access to:
/// - File management operations via FileManagementClient.
/// - RAG corpus operations via RagCorpusClient.
/// </summary>
public class VertexRagManager : BaseClient
{
    /// <summary>
    /// Provides access to file management operations within the VertexRagManager context.
    /// This property is an instance of the <see cref="FileManagementClient"/> class and enables
    /// methods for uploading, importing, listing, retrieving, and deleting RAG (Retrieval-Augmented Generation)
    /// files. These operations are essential for managing file resources in RAG-based workflows.
    /// </summary>
    public FileManagementClient FileManager { get; private set; }

    /// <summary>
    /// Provides access to RAG (Retrieval-Augmented Generation) corpus management operations within the VertexRagManager context.
    /// This property is an instance of the <see cref="RagCorpusClient"/> class. It enables creating, listing, retrieving,
    /// updating, and deleting RAG corpus resources. These capabilities are crucial for managing data corpora used in
    /// generation and retrieval workflows.
    /// </summary>
    public RagCorpusClient RagCorpusClient { get; private set; }

    /// <summary>
    /// Provides access to manage long-running operations within the VertexRagManager context.
    /// This property is an instance of the <see cref="OperationsClient"/> class and facilitates
    /// tracking, retrieving, and controlling asynchronous operations performed by the manager.
    /// </summary>
    public OperationsClient OperationsClient { get; private set; }

    /// <summary>
    /// Specifies the timeout duration, in milliseconds, for long-running operations initiated within the
    /// VertexRagManager. This property determines the maximum amount of time that the system will wait
    /// for such operations to complete before considering them as timed out.
    /// </summary>
    public int LongRunningOperationTimeout { get; set; } = 5 * 60 * 1000; // 5 minutes in milliseconds;

    /// <summary>
    /// VertexRagManager provides management and interaction capabilities for RAG (Retrieval-Augmented Generation)
    /// resources. It is designed to facilitate operations involving file management and RAG corpus resources.
    /// </summary>
    /// <remarks>
    /// Inherits functionality from BaseClient, leveraging its shared features such as
    /// authorization handling and HTTP communication.
    /// The VertexRagManager provides:
    /// - File management operations via the FileManager property.
    /// - RAG corpus operations via the RagCorpus property.
    /// </remarks>
    public VertexRagManager(IPlatformAdapter platform, HttpClient? httpClient, ILogger? logger = null) : base(platform,
        httpClient, logger)
    {
        InitializeClients();
    }

    /// <summary>
    /// Initializes the clients used by the VertexRagManager, including the FileManager and RagCorpus properties.
    /// </summary>
    /// <remarks>
    /// This method is responsible for instantiating and assigning the specialized client objects:
    /// - FileManager: Handles file management operations.
    /// - RagCorpus: Manages operations related to the RAG corpus.
    /// This ensures the VertexRagManager has access to the necessary client subsystems for RAG resource management.
    /// </remarks>
    private void InitializeClients()
    {
        this.FileManager = new FileManagementClient(_platform, HttpClient, Logger);
        this.RagCorpusClient = new RagCorpusClient(_platform, HttpClient, Logger);
        this.OperationsClient = new OperationsClient(_platform, HttpClient, Logger);
    }

    #region Create Corpus

    /// <summary>
    /// Asynchronously creates a new RAG corpus and returns the resulting <see cref="RagCorpus"/> object upon completion.
    /// </summary>
    /// <param name="corpus">The <see cref="RagCorpus"/> instance representing the corpus configuration to be created.</param>
    /// <param name="cancellationToken">Optional token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the created <see cref="RagCorpus"/>,
    /// or null if the operation fails or cannot determine the result.
    /// </returns>
    public async Task<RagCorpus?> CreateCorpusAsync(RagCorpus corpus, CancellationToken cancellationToken = default)
    {
        var longRunningOperation =
            await RagCorpusClient.CreateRagCorpusAsync(corpus, cancellationToken).ConfigureAwait(false);


        longRunningOperation =
            await AwaitForLongRunningOperation(longRunningOperation.Name, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        if (longRunningOperation.Done == true && longRunningOperation.Response.ContainsKey("name"))
        {
            var nameJson = (JsonElement)longRunningOperation.Response["name"];
            var name = nameJson.GetString();

            return await RagCorpusClient.GetRagCorpusAsync(name, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    /// <summary>
    /// Awaits the completion of a long-running operation specified by its operation ID.
    /// This method repeatedly checks the operation's progress until it's complete or until the specified timeout is reached.
    /// </summary>
    /// <param name="operationId">The unique identifier of the long-running operation to monitor.</param>
    /// <param name="timeOut">Optional timeout (in milliseconds) for awaiting the operation. If not provided, the default timeout specified by LongRunningOperationTimeout is used.</param>
    /// <param name="cancellationToken">An optional token to observe and respond to cancellation requests.</param>
    /// <returns>
    /// Returns a <see cref="GoogleLongRunningOperation"/> instance representing the final state of the long-running operation upon completion.
    /// Throws an exception if the operation encounters an error.
    /// </returns>
    public async Task<GoogleLongRunningOperation?> AwaitForLongRunningOperation(string operationId, int? timeOut = null,
        CancellationToken cancellationToken = default)
    {
        GoogleLongRunningOperation? longRunningOperation = null;
        timeOut ??= LongRunningOperationTimeout;
        var sw = new Stopwatch();
        sw.Start();
        do
        {
            longRunningOperation =
                await OperationsClient.GetOperationAsync(operationId, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            
            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        } while (longRunningOperation.Done != true && sw.ElapsedMilliseconds < LongRunningOperationTimeout);

        if (longRunningOperation.Done == true && longRunningOperation.Error != null)
        {
            throw new VertexAIException(longRunningOperation.Error.Message, longRunningOperation.Error);
        }

        return longRunningOperation;
    }

    /// <summary>
    /// creates a new RAG (Retrieval-Augmented Generation) corpus with the specified parameters.
    /// </summary>
    /// <param name="displayName">The display name of the corpus to be created.</param>
    /// <param name="description">A brief description of the corpus being created.</param>
    /// <param name="pineconeConfig">The configuration for the Pinecone vector database to be associated with the corpus.</param>
    /// <param name="apiKeyResourceName">This the full resource name of the secret that is stored in Secret Manager, which contains your Weaviate or Pinecone API key that depends on your choice of vector database. <br/>Format: projects/{PROJECT_NUMBER}/secrets/{SECRET_ID}/versions/{VERSION_ID}</param>
    /// <param name="embeddingModelName">The name of the embedding model to be used for the corpus. Defaults to text-embedding-004.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if needed. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the created <see cref="RagCorpus"/> instance.</returns>
    public async Task<RagCorpus?> CreateCorpusAsync(string displayName, string? description,
        RagVectorDbConfigPinecone pineconeConfig, string apiKeyResourceName, string? embeddingModelName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RagCorpus();
        request.DisplayName = displayName;
        request.Description = description;
        request.AddPinecone(pineconeConfig, apiKeyResourceName);

        if (embeddingModelName != null)
            request.AddEmbeddingModel(embeddingModelName);

        return await CreateCorpusAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new RAG (Retrieval-Augmented Generation) corpus with the specified parameters.
    /// </summary>
    /// <param name="displayName">The display name of the corpus to be created.</param>
    /// <param name="description">A brief description of the corpus being created.</param>
    /// <param name="weaviateConfig">The configuration for the Weaviate vector database to be associated with the corpus.</param>
    /// <param name="apiKeyResourceName">The full resource name of the secret stored in Secret Manager, which contains the API key for the vector database.
    /// Format: projects/{PROJECT_NUMBER}/secrets/{SECRET_ID}/versions/{VERSION_ID}</param>
    /// <param name="embeddingModelName">The name of the embedding model to be used for the corpus. If not provided, defaults to text-embedding-004.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if needed. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the created <see cref="RagCorpus"/> instance.</returns>
    public async Task<RagCorpus?> CreateCorpusAsync(string displayName, string? description,
        RagVectorDbConfigWeaviate weaviateConfig, string apiKeyResourceName, string? embeddingModelName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RagCorpus();
        request.DisplayName = displayName;
        request.Description = description;
        request.AddWeaviate(weaviateConfig, apiKeyResourceName);

        if (embeddingModelName != null)
            request.AddEmbeddingModel(embeddingModelName);

        return await CreateCorpusAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new RAG (Retrieval-Augmented Generation) corpus with the specified parameters.
    /// </summary>
    /// <param name="displayName">The display name of the corpus to be created.</param>
    /// <param name="description">A brief description of the corpus being created.</param>
    /// <param name="embeddingModelName">The name of the embedding model to be used for the corpus. If not provided, defaults to text-embedding-004.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if needed. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the created <see cref="RagCorpus"/> instance.</returns>
    public async Task<RagCorpus?> CreateCorpusAsync(string displayName, string? description,
        string? embeddingModelName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RagCorpus();
        request.DisplayName = displayName;
        request.Description = description;

        if (embeddingModelName != null)
            request.AddEmbeddingModel(embeddingModelName);

        return await CreateCorpusAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously creates a new RAG (Retrieval-Augmented Generation) corpus with the provided parameters and configuration.
    /// </summary>
    /// <param name="displayName">The display name for the new corpus.</param>
    /// <param name="description">An optional description of the corpus to provide additional context.</param>
    /// <param name="vertexFeatureStoreConfig">The configuration details for the Vertex feature store to be associated with the corpus.</param>
    /// <param name="embeddingModelName">The embedding model's name to be used for the corpus, defaults to "text-embedding-004" if not specified.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation before completion. Defaults to <see cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation and returns the created <see cref="RagCorpus"/> instance, or null if the operation fails.</returns>
    public async Task<RagCorpus?> CreateCorpusAsync(string displayName, string? description,
        RagVectorDbConfigVertexFeatureStore vertexFeatureStoreConfig, string? embeddingModelName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RagCorpus();
        request.DisplayName = displayName;
        request.Description = description;
        request.AddVertexFeatureStore(vertexFeatureStoreConfig);

        if (embeddingModelName != null)
            request.AddEmbeddingModel(embeddingModelName);

        return await CreateCorpusAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously creates a new RAG (Retrieval-Augmented Generation) corpus with the specified parameters and configurations.
    /// </summary>
    /// <param name="displayName">The display name of the RAG corpus to be created.</param>
    /// <param name="description">An optional description of the RAG corpus to provide additional context or details.</param>
    /// <param name="vertexSearchStoreConfig">The configuration parameters for the Vertex feature store to associate with the new corpus.</param>
    /// <param name="embeddingModelName">The name of the embedding model to use with the corpus. If not provided, the default is "text-embedding-004".</param>
    /// <param name="cancellationToken">An optional token to cancel the operation. Defaults to <see cref="CancellationToken.None"/> if not specified.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the created <see cref="RagCorpus"/> instance, or null if the operation fails.</returns>
    public async Task<RagCorpus?> CreateCorpusAsync(string displayName, string? description,
        RagVectorDbConfigVertexVectorSearch vertexSearchStoreConfig, string? embeddingModelName = null,
        CancellationToken cancellationToken = default)
    {
        var request = new RagCorpus();
        request.DisplayName = displayName;
        request.Description = description;
        request.AddVertexSearch(vertexSearchStoreConfig);

        if (embeddingModelName != null)
            request.AddEmbeddingModel(embeddingModelName);

        return await CreateCorpusAsync(request, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Update Rag Corpus

    /// <summary>
    /// Updates an existing RAG corpus asynchronously.
    /// </summary>
    /// <remarks>
    /// This method updates an existing RAG corpus by sending the desired modifications,
    /// waits for the long-running operation to complete, and retrieves the updated corpus upon completion.
    /// </remarks>
    /// <param name="corpus">The RagCorpus object containing the updated information for the RAG corpus.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The updated RagCorpus object if the operation is successful, or null if the operation fails.</returns>
    public async Task<RagCorpus> UpdateCorpusAsync(RagCorpus corpus, CancellationToken cancellationToken = default)
    {
        var longRunning =
            await RagCorpusClient.UpdateRagCorpusAsync(corpus.Name, corpus, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        longRunning = await AwaitForLongRunningOperation(longRunning.Name, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (longRunning.Done == true)
        {
            var name = corpus.Name;
            return await RagCorpusClient.GetRagCorpusAsync(name, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    /// <summary>
    /// Lists available <see cref="RagCorpus"/> resources.
    /// </summary>
    /// <param name="pageSize">The maximum number of <see cref="RagCorpus"/> resources to return.</param>
    /// <param name="pageToken">A page token, received from a previous <see cref="ListRagCorporaAsync"/> call.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A list of <see cref="RagCorpus"/> resources.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<ListRagCorporaResponse?> ListCorporaAsync(int? pageSize = null, string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        return await RagCorpusClient.ListRagCorporaAsync(pageSize, pageToken, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a specific <see cref="RagCorpus"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="RagCorpus"/>.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The <see cref="RagCorpus"/> resource.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task<RagCorpus?> GetCorpusAsync(string name, CancellationToken cancellationToken = default)
    {
        return await RagCorpusClient.GetRagCorpusAsync(name, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a <see cref="RagCorpus"/> resource.
    /// </summary>
    /// <param name="name">The resource name of the <see cref="RagCorpus"/> to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <seealso href="https://cloud.google.com/vertex-ai/generative-ai/docs/model-reference/rag-api">See Official API Documentation</seealso>
    public async Task DeleteRagCorpusAsync(string name, CancellationToken cancellationToken = default)
    {
        await RagCorpusClient.DeleteRagCorpusAsync(name, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Add Files

    /// <summary>
    /// Uploads a local file to the specified RAG corpus with optional progress tracking and metadata.
    /// </summary>
    /// <param name="corpusName">The name of the RAG corpus to upload the file to.</param>
    /// <param name="localFilePath">The full path to the local file to be uploaded.</param>
    /// <param name="displayName">A user-friendly name for the uploaded file.</param>
    /// <param name="description">An optional description of the file being uploaded.</param>
    /// <param name="progressCallback">An optional callback to monitor the upload progress, represented as a percentage value.</param>
    /// <param name="cancellationToken">A token to monitor and handle request cancellation.</param>
    /// <returns>An <see cref="UploadRagFileResponse"/> containing details about the uploaded file, or null if the operation fails.</returns>
    public async Task<RagFile?> UploadLocalFileAsync(string corpusName, string localFilePath, string displayName = null,
        string? description = null, UploadRagFileConfig uploadRagFileConfig = null,
        Action<double>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        return await FileManager.UploadRagFileAsync(corpusName, localFilePath, displayName, description,
            uploadRagFileConfig, progressCallback,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously imports files to a specified RAG corpus using the provided request details.
    /// </summary>
    /// <param name="corpusName">The name of the corpus to which the files will be imported.</param>
    /// <param name="request">An instance of <see cref="ImportRagFilesRequest"/> containing the details of files to import.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation, if necessary.</param>
    /// <returns>A <see cref="GoogleLongRunningOperation"/> instance that represents the long-running import operation.</returns>
    public async Task<GoogleLongRunningOperation?> ImportFilesAsync(string corpusName, ImportRagFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await FileManager.ImportRagFilesAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Imports files into a specified RAG corpus using Google Cloud Storage (GCS) as a source, with optional parsing and transformation configurations.
    /// </summary>
    /// <param name="corpusName">The name of the RAG corpus where the files are to be imported.</param>
    /// <param name="source">The Google Cloud Storage (GCS) source containing the files to be imported.</param>
    /// <param name="parsingConfig">Optional configuration to define how the files should be parsed upon import.</param>
    /// <param name="transformationConfig">Optional configuration to define how the files should be transformed during the import process.</param>
    /// <param name="cancellationToken">Token to monitor and handle request cancellation.</param>
    /// <returns>An asynchronous operation of type <see cref="GoogleLongRunningOperation"/> representing the import process.</returns>
    public async Task<GoogleLongRunningOperation?> ImportFilesAsync(string corpusName, GcsSource source,
        RagFileParsingConfig? parsingConfig = null, RagFileTransformationConfig? transformationConfig = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ImportRagFilesRequest();
        request.AddSource(source);
        if (request.ImportRagFilesConfig != null)
        {
            request.ImportRagFilesConfig.RagFileParsingConfig = parsingConfig;
            request.ImportRagFilesConfig.RagFileTransformationConfig = transformationConfig;
        }

        return await ImportFilesAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Imports files from a Google Drive source to a specified RAG corpus with optional configurations for parsing and transformation.
    /// </summary>
    /// <param name="corpusName">The name of the RAG corpus where the files are to be imported.</param>
    /// <param name="googleDriveSource">The Google Drive source from which the files will be imported.</param>
    /// <param name="parsingConfig">Optional configuration specifying how the files should be parsed.</param>
    /// <param name="transformationConfig">Optional configuration specifying how the files should be transformed during the import process.</param>
    /// <param name="cancellationToken">Token to monitor and respond to cancellation requests for the import operation.</param>
    /// <returns>An asynchronous operation of type <see cref="GoogleLongRunningOperation"/> representing the status of the import process.</returns>
    public async Task<GoogleLongRunningOperation?> ImportFilesAsync(string corpusName,
        GoogleDriveSource googleDriveSource,
        RagFileParsingConfig? parsingConfig = null, RagFileTransformationConfig? transformationConfig = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ImportRagFilesRequest();
        request.AddSource(googleDriveSource);
        if (request.ImportRagFilesConfig != null)
        {
            request.ImportRagFilesConfig.RagFileParsingConfig = parsingConfig;
            request.ImportRagFilesConfig.RagFileTransformationConfig = transformationConfig;
        }

        return await ImportFilesAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously imports files into a specified Retrieval-Augmented Generation (RAG) corpus
    /// from a Slack source, with optional parsing and transformation configurations.
    /// </summary>
    /// <param name="corpusName">The name of the RAG corpus into which the files will be imported.</param>
    /// <param name="slackSource">The Slack source containing the files to be imported into the RAG corpus.</param>
    /// <param name="parsingConfig">Optional configuration for parsing the files during the import process.</param>
    /// <param name="transformationConfig">Optional configuration for applying transformations to the files during the import process.</param>
    /// <param name="cancellationToken">A token that allows the operation to be canceled.</param>
    /// <returns>An instance of <see cref="GoogleLongRunningOperation"/> that represents the ongoing asynchronous import operation.</returns>
    public async Task<GoogleLongRunningOperation?> ImportFilesAsync(string corpusName, SlackSource slackSource,
        RagFileParsingConfig? parsingConfig = null, RagFileTransformationConfig? transformationConfig = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ImportRagFilesRequest();
        request.AddSource(slackSource);
        if (request.ImportRagFilesConfig != null)
        {
            request.ImportRagFilesConfig.RagFileParsingConfig = parsingConfig;
            request.ImportRagFilesConfig.RagFileTransformationConfig = transformationConfig;
        }

        return await ImportFilesAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously imports files into a specified Retrieval-Augmented Generation (RAG) corpus
    /// from a Jira source, with optional parsing and transformation configurations.
    /// </summary>
    /// <param name="corpusName">The name of the RAG corpus into which the files will be imported.</param>
    /// <param name="jiraSource">The Jira source containing the files to be imported into the RAG corpus.</param>
    /// <param name="parsingConfig">Optional configuration for parsing the files during the import process.</param>
    /// <param name="transformationConfig">Optional configuration for applying transformations to the files during the import process.</param>
    /// <param name="cancellationToken">A token that allows the operation to be canceled.</param>
    /// <returns>An instance of <see cref="GoogleLongRunningOperation"/> that represents the ongoing asynchronous import operation.</returns>
    public async Task<GoogleLongRunningOperation?> ImportFilesAsync(string corpusName, JiraSource jiraSource,
        RagFileParsingConfig? parsingConfig = null, RagFileTransformationConfig? transformationConfig = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ImportRagFilesRequest();
        request.AddSource(jiraSource);
        if (request.ImportRagFilesConfig != null)
        {
            request.ImportRagFilesConfig.RagFileParsingConfig = parsingConfig;
            request.ImportRagFilesConfig.RagFileTransformationConfig = transformationConfig;
        }

        return await ImportFilesAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously imports files into a specified Retrieval-Augmented Generation (RAG) corpus
    /// from a SharePoint source, with optional parsing and transformation configurations.
    /// </summary>
    /// <param name="corpusName">The name of the RAG corpus into which the files will be imported.</param>
    /// <param name="sharePointSources">The SharePoint source containing the files to be imported into the RAG corpus.</param>
    /// <param name="parsingConfig">Optional configuration for parsing the files during the import process.</param>
    /// <param name="transformationConfig">Optional configuration for applying transformations to the files during the import process.</param>
    /// <param name="cancellationToken">A token that allows the operation to be canceled.</param>
    /// <returns>An instance of <see cref="GoogleLongRunningOperation"/> that represents the ongoing asynchronous import operation.</returns>
    public async Task<GoogleLongRunningOperation?> ImportFilesAsync(string corpusName,
        SharePointSources sharePointSources,
        RagFileParsingConfig? parsingConfig = null, RagFileTransformationConfig? transformationConfig = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ImportRagFilesRequest();
        request.AddSource(sharePointSources);
        if (request.ImportRagFilesConfig != null)
        {
            request.ImportRagFilesConfig.RagFileParsingConfig = parsingConfig;
            request.ImportRagFilesConfig.RagFileTransformationConfig = transformationConfig;
        }

        return await ImportFilesAsync(corpusName, request, cancellationToken).ConfigureAwait(false);
    }

    #endregion
    
    #region File Management

    /// <summary>
    /// Retrieves a specified RAG file by its unique file identifier.
    /// </summary>
    /// <param name="fileId">The unique identifier of the RAG file to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="RagFile"/> instance representing the requested RAG file, or null if the file is not found.</returns>
    public async Task<RagFile?> GetFileAsync(string fileId,
        CancellationToken cancellationToken = default)
    {
        return await FileManager.GetRagFileAsync(fileId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a paginated list of RAG (Retrieval-Augmented Generation) files within the specified corpus.
    /// </summary>
    /// <param name="corpusId">The unique identifier of the corpus containing the RAG files.</param>
    /// <param name="pageSize">The desired number of files to retrieve per page. If null, the default server-defined page size is used.</param>
    /// <param name="pageToken">The token for continuing the listing from a specific point in the pagination.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ListRagFilesResponse"/> object containing the list of RAG files and pagination details, or null if no files are found.</returns>
    public async Task<ListRagFilesResponse?> ListFilesAsync(string corpusId, int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        return await FileManager.ListRagFilesAsync(corpusId, pageSize, pageToken, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a specified RAG (Retrieval-Augmented Generation) file by its unique file identifier.
    /// </summary>
    /// <param name="fileId">The unique identifier of the RAG file to be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. No value is returned upon completion.</returns>
    public async Task DeleteFileAsync(string fileId,
        CancellationToken cancellationToken = default)
    {
        await FileManager.DeleteRagFileAsync(fileId, cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion
}