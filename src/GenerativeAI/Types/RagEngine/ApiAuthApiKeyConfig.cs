using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The API secret.
/// </summary>
public class ApiAuthApiKeyConfig
{
    /// <summary>
    /// Required. The SecretManager secret version resource name storing API key. e.g. projects/{project}/secrets/{secret}/versions/{version}
    /// </summary>
    [JsonPropertyName("apiKeySecretVersion")]
    public string? ApiKeySecretVersion { get; set; } 
}