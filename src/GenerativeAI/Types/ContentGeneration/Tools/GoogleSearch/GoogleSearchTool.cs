using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// GoogleSearch tool type. Tool to support Google Search in Model. Powered by Google.
/// </summary>
/// <seealso href="https://ai.google.dev/api/caching#GoogleSearch">See Official API Documentation</seealso> 
public class GoogleSearchTool // Renamed to GoogleSearchTool
{
    /// <summary>
    /// The kinds of search to use (web and/or image). Defaults to web search.
    /// </summary>
    [JsonPropertyName("searchTypes")]
    public SearchTypes? SearchTypes { get; set; }

    /// <summary>
    /// Restricts results to pages published within this time range.
    /// </summary>
    [JsonPropertyName("timeRangeFilter")]
    public Interval? TimeRangeFilter { get; set; }
}