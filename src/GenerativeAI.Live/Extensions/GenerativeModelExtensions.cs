using GenerativeAI.Clients;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI.Live.Extensions;

/// <summary>
/// Provides extension methods for GenerativeModel to create MultiModalLiveClient instances.
/// </summary>
public static class GenerativeModelExtensions
{
    /// <summary>
    /// Creates a new MultiModalLiveClient instance from the GenerativeModel.
    /// </summary>
    /// <param name="generativeModel">The GenerativeModel to create the client from.</param>
    /// <param name="config">Optional generation configuration. If null, uses the model's default config.</param>
    /// <param name="safetySettings">Optional safety settings. If null, uses the model's default safety settings.</param>
    /// <param name="systemInstruction">Optional system instruction. If null, uses the model's default system instruction.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <returns>A new MultiModalLiveClient instance.</returns>
    public static MultiModalLiveClient CreateMultiModalLiveClient(this GenerativeModel generativeModel, GenerationConfig? config = null,
        ICollection<SafetySetting>? safetySettings = null,
        string? systemInstruction = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(generativeModel);
        var client = new MultiModalLiveClient(generativeModel.Platform, generativeModel.Model, config ?? generativeModel.Config, safetySettings ?? generativeModel.SafetySettings, systemInstruction ?? generativeModel.SystemInstruction);

        client.AddFunctionTools(generativeModel.FunctionTools, generativeModel.ToolConfig);
        return client;
    }
}