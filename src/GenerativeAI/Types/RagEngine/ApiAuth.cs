using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The generic reusable api auth config. Deprecated. Please use AuthConfig (google/cloud/aiplatform/master/auth.proto) instead.
/// </summary>
public class ApiAuth
{
    /// <summary>
    /// The API secret.
    /// </summary>
    [JsonPropertyName("apiKeyConfig")]
    public ApiAuthApiKeyConfig? ApiKeyConfig { get; set; } 
}