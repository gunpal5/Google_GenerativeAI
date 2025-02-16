using GenerativeAI.Clients;
using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

public partial class GeminiModel:GenerativeModel
{
    public GeminiModel(IPlatformAdapter platform, string model, GenerationConfig config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null, HttpClient? httpClient = null, ILogger? logger = null) : base(platform, model, config, safetySettings, systemInstruction, httpClient, logger)
    {
        InitClients();
    }

    public GeminiModel(string apiKey, ModelParams modelParams, HttpClient? client = null, ILogger? logger = null) : base(apiKey, modelParams, client, logger)
    {
        InitClients();
    }

    public GeminiModel(FileClient files, int timeoutForFileStateCheck, string apiKey, string model, GenerationConfig config = null, ICollection<SafetySetting>? safetySettings = null, string? systemInstruction = null, HttpClient? httpClient = null, ILogger? logger = null) : base(apiKey, model, config, safetySettings, systemInstruction, httpClient, logger)
    {
        InitClients();
    }

    
    private void InitClients()
    {
        Files = new FileClient(this.Platform, this.HttpClient, this.Logger);
    }

    public FileClient Files { get; set; }

  
}