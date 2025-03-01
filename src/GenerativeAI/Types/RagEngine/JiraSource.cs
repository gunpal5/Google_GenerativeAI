using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The Jira source for the ImportRagFilesRequest.
/// </summary>
public class JiraSource
{
    /// <summary>
    /// Required. The Jira queries.
    /// </summary>
    [JsonPropertyName("jiraQueries")]
    public System.Collections.Generic.ICollection<JiraSourceJiraQueries>? JiraQueries { get; set; } 
}