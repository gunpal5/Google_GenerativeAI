using GenerativeAI.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenerativeAI.Web;

/// <summary>
/// Interface for GenerativeAI service that provides access to AI models.
/// </summary>
public interface IGenerativeAiService
{
    /// <summary>
    /// Gets the underlying GenerativeAI platform instance.
    /// </summary>
    IGenerativeAI Platform { get; }

    /// <summary>
    /// Creates a generative model instance.
    /// </summary>
    /// <param name="modelName">
    /// The name of the model to create. When omitted, <see cref="GenerativeAIOptions.Model"/> is used, falling back to
    /// <see cref="GoogleAIModels.DefaultGeminiModel"/>.
    /// </param>
    /// <returns>A generative model instance.</returns>
    IGenerativeModel CreateInstance(string? modelName = null);
}

/// <summary>
/// Service implementation for GenerativeAI that provides access to AI models.
/// </summary>
public class GenerativeAIService : IGenerativeAiService
{
    private readonly IGenerativeAI _platform;
    private readonly IGoogleAuthenticator? _authenticator;
    private readonly string? _defaultModel;
    
    /// <summary>
    /// Gets or sets the logger instance.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Initializes a new instance of the GenerativeAIService class.
    /// </summary>
    /// <param name="options">The configuration options for GenerativeAI.</param>
    public GenerativeAIService(IOptions<GenerativeAIOptions> options)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options == null) throw new ArgumentNullException(nameof(options));
#endif
        this._authenticator = options.Value.Authenticator;
        this._defaultModel = options.Value.Model;
        if (options.Value.IsVertex == true)
        {
            var platformAdapter = new VertextPlatformAdapter(
                projectId: options.Value.ProjectId,
                region: options.Value.Region,
                expressMode: options.Value.ExpressMode ?? false,
                apiKey: options.Value.Credentials?.ApiKey,
                apiVersion: options.Value.ApiVersion,
                authenticator: options.Value.Authenticator);

            _platform = new VertexAI(platformAdapter, logger: Logger);
        }
        else
        {
            if (options.Value.Credentials?.ApiKey == null)
                throw new InvalidOperationException("API Key is required for Google AI configuration.");
            // A null ApiVersion lets the adapter apply GOOGLE_GENAI_API_VERSION or its default.
            var platformAdapter = new GoogleAIPlatformAdapter(options.Value.Credentials.ApiKey,
                apiVersion: options.Value.ApiVersion, logger: this.Logger);
            _platform = new GoogleAi(platformAdapter, logger: this.Logger);
        }
    }

    /// <summary>
    /// Gets the underlying GenerativeAI platform instance.
    /// </summary>
    public IGenerativeAI Platform { get => _platform; }

    /// <summary>
    /// Creates a generative model instance.
    /// </summary>
    /// <param name="modelName">
    /// The name of the model to create. When omitted, <see cref="GenerativeAIOptions.Model"/> is used, falling back to
    /// <see cref="GoogleAIModels.DefaultGeminiModel"/>.
    /// </param>
    /// <returns>A generative model instance.</returns>
    public IGenerativeModel CreateInstance(string? modelName = null)
    {
        var model = !string.IsNullOrWhiteSpace(modelName) ? modelName!
            : !string.IsNullOrWhiteSpace(_defaultModel) ? _defaultModel!
            : GoogleAIModels.DefaultGeminiModel;
        return _platform.CreateGenerativeModel(model);
    }
}