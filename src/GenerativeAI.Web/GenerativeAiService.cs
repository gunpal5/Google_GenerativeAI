using GenerativeAI.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenerativeAI.Web;

public interface IGenerativeAiService
{
    IGenerativeAI Platform { get; }

    IGenerativeModel CreateInstance(string modelName = GoogleAIModels.DefaultGeminiModel);
}

public class GenerativeAIService : IGenerativeAiService
{
    private readonly IGenerativeAI _platform;
    private readonly IGoogleAuthenticator? _authenticator;
    public ILogger? Logger { get; set; }

    public GenerativeAIService(IOptions<GenerativeAIOptions> options)
    {
        this._authenticator = options.Value.Authenticator;
        if (options.Value.IsVertex == true)
        {
            var platformAdapter = new VertextPlatformAdapter(options.Value.ProjectId, options.Value.Region,
                options.Value.ExpressMode??false, options.Value.Credentials?.ApiKey, options.Value.ApiVersion,
                authenticator: options.Value.Authenticator);

            _platform = new VertexAI(platformAdapter, logger: Logger);
        }
        else
        {
            var platformAdapter = new GoogleAIPlatformAdapter(options.Value.Credentials.ApiKey, options.Value.ApiVersion,
                logger: this.Logger);
            _platform = new GoogleAi(platformAdapter, logger: this.Logger);
        }
    }

    public IGenerativeAI Platform { get => _platform; }
  

    public IGenerativeModel CreateInstance(string modelName = GoogleAIModels.DefaultGeminiModel)
    {
        return _platform.CreateGenerativeModel(modelName);
    }
}