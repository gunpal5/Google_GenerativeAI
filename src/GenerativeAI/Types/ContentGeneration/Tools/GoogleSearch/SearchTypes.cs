using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// The kinds of Google Search the model may use. Leave unset for web search only.
/// </summary>
public class SearchTypes
{
    /// <summary>
    /// Enables web search.
    /// </summary>
    [JsonPropertyName("webSearch")]
    public WebSearch? WebSearch { get; set; }

    /// <summary>
    /// Enables image search, so responses can be grounded in images.
    /// </summary>
    [JsonPropertyName("imageSearch")]
    public ImageSearch? ImageSearch { get; set; }
}

/// <summary>
/// Web search settings. Currently has no options; set an instance to enable web search.
/// </summary>
public class WebSearch
{
}

/// <summary>
/// Image search settings. Currently has no options; set an instance to enable image search.
/// </summary>
public class ImageSearch
{
}

/// <summary>
/// A time interval. Both ends are optional.
/// </summary>
public class Interval
{
    /// <summary>
    /// Inclusive start of the interval.
    /// </summary>
    [JsonPropertyName("startTime")]
    public Timestamp? StartTime { get; set; }

    /// <summary>
    /// Exclusive end of the interval.
    /// </summary>
    [JsonPropertyName("endTime")]
    public Timestamp? EndTime { get; set; }
}
