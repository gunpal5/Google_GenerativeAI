using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// Represents the Vertex AI implementation of the generative AI platform.
/// </summary>
public class VertexAI : GenAI, IGenerativeAI
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAI"/> class with a platform adapter, an optional HTTP client, and an optional logger.
    /// </summary>
    /// <param name="platformAdapter">The platform adapter to use for requests.</param>
    /// <param name="client">Optional HTTP client for making requests.</param>
    /// <param name="logger">Optional logger to log messages.</param>
    public VertexAI(IPlatformAdapter platformAdapter, HttpClient? client = null, ILogger? logger = null)
        : base(platformAdapter, client, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAI"/> class with project-specific configuration and credentials.
    /// </summary>
    /// <param name="projectId">The ID of the Google Cloud project.</param>
    /// <param name="region">The region to use for the Vertex AI platform.</param>
    /// <param name="accessToken">The access token for authorization.</param>
    /// <param name="expressMode">Specifies whether express mode is enabled.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="apiVersion">The API version to use.</param>
    /// <param name="httpClient">Optional HTTP client for making requests.</param>
    /// <param name="authenticator">Optional Google authenticator for handling authentication.</param>
    /// <param name="logger">Optional logger to log messages.</param>
    public VertexAI(string? projectId = null,
        string? region = null,
        string? accessToken = null,
        bool expressMode = false,
        string? apiKey = null,
        string apiVersion = ApiVersions.v1Beta1,
        HttpClient? httpClient = null,
        IGoogleAuthenticator? authenticator = null,
        ILogger? logger = null)
        : this(new VertextPlatformAdapter(
                projectId,
                region,
                expressMode,
                apiKey,
                accessToken,
                apiVersion,
                authenticator: authenticator),
            httpClient,
            logger)
    {
    }

    /// <summary>
    /// Creates and returns a new instance of the <see cref="ImageTextModel"/> class initialized with the platform adapter, HTTP client, and logger available in the current <see cref="VertexAI"/> instance.
    /// </summary>
    /// <returns>An instance of <see cref="ImageTextModel"/> configured with the current platform adapter, HTTP client, and logger.</returns>
    public ImageTextModel CreateImageTextModel()
    {
        return new ImageTextModel(this.Platform, this.HttpClient, this.Logger);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="VertexRagManager"/> class, which provides functions for managing Vertex AI Retrieval-Augmented Generation functionality.
    /// </summary>
    /// <returns>A new instance of <see cref="VertexRagManager"/> configured with the current platform, HTTP client, and logger.</returns>
    public VertexRagManager CreateRagManager()
    {
        return new VertexRagManager(this.Platform, this.HttpClient, this.Logger);
    }

    /// <summary>
    /// Creates an instance of the <see cref="GenerativeModel"/> class with the specified model name, generation configuration, safety settings, system instructions, and an optional corpus ID for Retriever-Augmented Generation (RAG).
    /// </summary>
    /// <param name="modelName">The name of the generative model to create.</param>
    /// <param name="config">Optional configuration for controlling generation behavior.</param>
    /// <param name="safetyRatings">Optional collection of safety settings to apply during the generation process.</param>
    /// <param name="systemInstruction">Optional instruction to guide model behavior during generation.</param>
    /// <param name="corpusIdForRag">Optional identifier for the corpus to use with the Retriever-Augmented Generation (RAG) tool.</param>
    /// <param name="ragRetrievalConfig">Configuration settings for the Retriever-Augmented Generation (RAG) tool.</param>
    /// <returns>A new <see cref="GenerativeModel"/> instance configured with the specified parameters.</returns>
    public GenerativeModel CreateGenerativeModel(string modelName, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetyRatings = null, string? systemInstruction = null,
        string? corpusIdForRag = null, RagRetrievalConfig? ragRetrievalConfig = null)
    {
        var model = base.CreateGenerativeModel(modelName, config, safetyRatings, systemInstruction);

        if (!string.IsNullOrEmpty(corpusIdForRag))
        {
            model.UseVertexRetrievalTool(corpusIdForRag, ragRetrievalConfig);
        }

        return model;
    }
}