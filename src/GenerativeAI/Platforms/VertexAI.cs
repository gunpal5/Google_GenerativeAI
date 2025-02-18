using GenerativeAI.Core;
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
}