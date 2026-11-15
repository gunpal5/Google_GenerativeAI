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

    /// <summary>
    /// The resource name of the Secret Manager secret version that stores the API key, in the form
    /// projects/{project}/secrets/{secret}/versions/{version}. When set, it takes precedence over the API key string.
    /// The Vertex AI Extension Service Agent needs secret access permission on the resource.
    /// </summary>
    [JsonPropertyName("apiKeySecret")]
    public string? ApiKeySecret { get; set; }

    /// <summary>
    /// The location in the HTTP request where the API key is sent.
    /// </summary>
    [JsonPropertyName("httpElementLocation")]
    public HttpElementLocation? HttpElementLocation { get; set; }

    /// <summary>
    /// The name of the parameter that carries the API key in the request, such as the query parameter name used to
    /// pass the key.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
