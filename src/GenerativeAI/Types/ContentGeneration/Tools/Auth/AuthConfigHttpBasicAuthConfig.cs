using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for HTTP Basic Authentication.
/// </summary>
public class AuthConfigHttpBasicAuthConfig
{
    /// <summary>
    /// Required. The name of the SecretManager secret version resource storing the base64 encoded credentials.
    /// Format: projects/{project}/secrets/{secret}/versions/{version}
    /// If specified, the secretmanager.versions.access permission should be granted to Vertex AI Extension Service Agent
    /// on the specified resource.
    /// </summary>
    [JsonPropertyName("credentialSecret")]
    public string? CredentialSecret { get; set; }
}
