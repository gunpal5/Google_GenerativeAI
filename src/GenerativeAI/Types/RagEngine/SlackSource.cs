using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// The Slack source for the ImportRagFilesRequest.
/// </summary>
public class SlackSource
{
    /// <summary>
    /// Required. The Slack channels.
    /// </summary>
    [JsonPropertyName("channels")]
    public System.Collections.Generic.ICollection<SlackSourceSlackChannels>? Channels { get; set; } 
}