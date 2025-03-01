using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// Specifies the advanced parsing for RagFiles.
/// </summary>
public class RagFileParsingConfigLlmParser
{
    /// <summary>
    /// The prompt to use for parsing. If not specified, a default prompt will be used.
    /// </summary>
    [JsonPropertyName("customParsingPrompt")]
    public string? CustomParsingPrompt { get; set; }

    /// <summary>
    /// The maximum number of requests the job is allowed to make to the LLM model per minute. Consult https://cloud.google.com/vertex-ai/generative-ai/docs/quotas and your document size to set an appropriate value here. If unspecified, a default value of 5000 QPM would be used.
    /// </summary>
    [JsonPropertyName("maxParsingRequestsPerMin")]
    public int? MaxParsingRequestsPerMin { get; set; }

    /// <summary>
    /// The name of a LLM model used for parsing. Format: * `projects/{project_id}/locations/{location}/publishers/{publisher}/models/{model}`
    /// </summary>
    [JsonPropertyName("modelName")]
    public string? ModelName { get; set; }
}