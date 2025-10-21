using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Config for Google Service Account Authentication.
/// </summary>
public class AuthConfigGoogleServiceAccountConfig
{
    /// <summary>
    /// Optional. The service account that the extension execution service runs as.
    /// If the service account is specified, the iam.serviceAccounts.getAccessToken permission
    /// should be granted to Vertex AI Extension Service Agent on the specified service account.
    /// If not specified, the Vertex AI Extension Service Agent will be used to execute the Extension.
    /// </summary>
    [JsonPropertyName("serviceAccount")]
    public string? ServiceAccount { get; set; }
}
