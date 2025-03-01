using System.Text.Json.Serialization;

namespace GenerativeAI.Types.RagEngine;

/// <summary>
/// SlackChannels contains the Slack channels and corresponding access token.
/// </summary>
public class SlackSourceSlackChannels
{
    /// <summary>
    /// Required. The SecretManager secret version resource name (e.g. projects/{project}/secrets/{secret}/versions/{version}) storing the Slack channel access token that has access to the slack channel IDs. See: https://api.slack.com/tutorials/tracks/getting-a-token.
    /// </summary>
    [JsonPropertyName("apiKeyConfig")]
    public ApiAuthApiKeyConfig? ApiKeyConfig { get; set; } 

    /// <summary>
    /// Required. The Slack channel IDs.
    /// </summary>
    [JsonPropertyName("channels")]
    public System.Collections.Generic.ICollection<SlackSourceSlackChannelsSlackChannel>? Channels { get; set; } 
}