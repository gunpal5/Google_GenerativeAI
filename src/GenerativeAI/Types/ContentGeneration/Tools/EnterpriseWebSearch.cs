using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Tool to search public web data, powered by Vertex AI Search and Sec4 compliance.
/// </summary>
public class EnterpriseWebSearch
{
    /// <summary>
    /// Optional. List of domains to be excluded from the search results. The default limit is 2000 domains.
    /// </summary>
    [JsonPropertyName("excludeDomains")]
    public List<string>? ExcludeDomains { get; set; }
}
