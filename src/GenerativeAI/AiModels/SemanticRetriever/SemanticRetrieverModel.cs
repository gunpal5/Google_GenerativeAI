using System.Diagnostics.Metrics;
using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// Represents a semantic retriever model used for working with documents, corpora, chunks,
/// and corpus permissions. This model requires a platform adapter with authentication for initialization.
/// </summary>
public partial class SemanticRetrieverModel : BaseModel
{
    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    public string ModelName { get; set; }

    /// <summary>
    /// Gets or sets the corpora manager, which provides operations for managing corpora,
    /// documents, chunks, and corpus permissions.
    /// </summary>
    public CorporaManager CorporaManager { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticRetrieverModel"/> class.
    /// </summary>
    /// <param name="modelName">The name of the semantic retriever model.</param>
    /// <param name="platform">The platform adapter providing necessary infrastructure, including authentication.</param>
    /// <param name="httpClient">The optional HTTP client for making requests.</param>
    /// <param name="logger">The optional logger for logging events and debugging information.</param>
    /// <exception cref="GenerativeAIException">Thrown when the platform authenticator is not provided.</exception>
    public SemanticRetrieverModel(IPlatformAdapter platform,
        string? modelName,
        ICollection<SafetySetting>? safetySettings = null,
        HttpClient? httpClient = null,
        ILogger? logger = null
       ) : base(platform, httpClient, logger)
    {
        this.ModelName = modelName;
        this.SafetySettings = safetySettings?.ToList();
        this.CorporaManager = new CorporaManager(platform, httpClient, logger);
    }

    /// <summary>
    /// Gets or sets the list of safety settings that define harm categories and thresholds
    /// to control the content moderation for model interactions.
    /// </summary>
    public List<SafetySetting>? SafetySettings { get; set; } = null;

    /// <summary>
    /// Represents a semantic retriever model designed to interact with a platform to fetch and process semantic data.
    /// </summary>
    public SemanticRetrieverModel(string modelName,
        string apiKey,
        IGoogleAuthenticator authenticator,
        HttpClient? httpClient = null,
        ICollection<SafetySetting>? safetySettings = null,
        ILogger? logger = null) : this(new GoogleAIPlatformAdapter(apiKey, authenticator: authenticator), modelName, safetySettings, httpClient,
        logger)
    {
        
    }

    public SemanticRetrieverChatSession CreateChatSession(string corpusName, AnswerStyle answerStyle = AnswerStyle.VERBOSE, List<Content>? history = null,List<SafetySetting>? safetySettings =null)
    {
        var chatSession = new SemanticRetrieverChatSession(this, corpusName, answerStyle, history, safetySettings??this.SafetySettings);
        return chatSession;
    }
}