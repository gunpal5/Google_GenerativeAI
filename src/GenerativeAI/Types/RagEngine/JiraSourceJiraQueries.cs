using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// JiraQueries contains the Jira queries and corresponding authentication.
/// </summary>
public class JiraSourceJiraQueries
{
    /// <summary>
    /// Required. The SecretManager secret version resource name (e.g. projects/{project}/secrets/{secret}/versions/{version}) storing the Jira API key. See [Manage API tokens for your Atlassian account](https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/).
    /// </summary>
    [JsonPropertyName("apiKeyConfig")]
    public ApiAuthApiKeyConfig? ApiKeyConfig { get; set; } 

    /// <summary>
    /// A list of custom Jira queries to import. For information about JQL (Jira Query Language), see https://support.atlassian.com/jira-service-management-cloud/docs/use-advanced-search-with-jira-query-language-jql/
    /// </summary>
    [JsonPropertyName("customQueries")]
    public System.Collections.Generic.ICollection<string>? CustomQueries { get; set; } 

    /// <summary>
    /// Required. The Jira email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; } 

    /// <summary>
    /// A list of Jira projects to import in their entirety.
    /// </summary>
    [JsonPropertyName("projects")]
    public System.Collections.Generic.ICollection<string>? Projects { get; set; } 

    /// <summary>
    /// Required. The Jira server URI.
    /// </summary>
    [JsonPropertyName("serverUri")]
    public string? ServerUri { get; set; } 
}