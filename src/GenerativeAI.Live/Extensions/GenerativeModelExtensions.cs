using GenerativeAI.Clients;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Live.Extensions;

public static class GenerativeModelExtensions
{
    public static MultiModalLiveClient CreateMultiModalLiveClient(this GenerativeModel generativeModel, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetySettings = null,
        string? systemInstruction = null,
        ILogger? logger = null)
    {
        var client = new MultiModalLiveClient(generativeModel.Platform, generativeModel.Model, config ?? generativeModel.Config, safetySettings ?? generativeModel.SafetySettings, systemInstruction ?? generativeModel.SystemInstruction);

        client.AddFunctionTools(generativeModel.FunctionTools, generativeModel.ToolConfig);
        return client;
    }
}