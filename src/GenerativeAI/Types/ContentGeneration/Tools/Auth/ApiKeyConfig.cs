using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for authentication with API key.
/// </summary>
public class ApiKeyConfig
{
    /// <summary>
    /// The API key to be used in the request directly.
    /// </summary>
    [JsonPropertyName("apiKeyString")]
    public string? ApiKeyString { get; set; }
}
