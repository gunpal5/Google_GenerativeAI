using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Request to generate a completion from the model.
/// </summary>
/// <seealso href="https://ai.google.dev/api/tokens#GenerateContentRequest">See Official API Documentation</seealso> 
public class GenerateContentRequestForCountToken: GenerateContentRequest
{
    /// <summary>
    /// Required. The name of the Model to use for generating the completion.
    /// Format: models/{model}.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="GenerateContentRequestForCountToken"/> with the provided model and base request.
    /// </summary>
    /// <param name="modelName">The name of the model to use.</param>
    /// <param name="baseRequest">The base <see cref="GenerateContentRequest"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="model"/> is null or empty.</exception>
    public GenerateContentRequestForCountToken(string modelName, GenerateContentRequest baseRequest)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("Model cannot be null or empty", nameof(modelName));
        }

        Model = modelName;
        Contents = baseRequest.Contents;
        Tools = baseRequest.Tools;
        ToolConfig = baseRequest.ToolConfig;
        SafetySettings = baseRequest.SafetySettings;
        SystemInstruction = baseRequest.SystemInstruction;
        GenerationConfig = baseRequest.GenerationConfig;
        CachedContent = baseRequest.CachedContent;
    }
}