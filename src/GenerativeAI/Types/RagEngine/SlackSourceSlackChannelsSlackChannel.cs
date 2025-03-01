using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// SlackChannel contains the Slack channel ID and the time range to import.
/// </summary>
public class SlackSourceSlackChannelsSlackChannel
{
    /// <summary>
    /// Required. The Slack channel ID.
    /// </summary>
    [JsonPropertyName("channelId")]
    public string? ChannelId { get; set; } 

    /// <summary>
    /// Optional. The ending timestamp for messages to import.
    /// </summary>
    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; } 

    /// <summary>
    /// Optional. The starting timestamp for messages to import.
    /// </summary>
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; } 
}