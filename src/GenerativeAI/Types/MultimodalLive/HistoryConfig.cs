using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Configuration for history exchange between client and server.
/// </summary>
public class HistoryConfig
{
    /// <summary>
    /// If true, after sending <c>setupComplete</c>, the server will wait and at first process <c>clientContent</c>
    /// messages until <c>turnComplete</c> is <c>true</c>. This initial history will not trigger a model call and
    /// may end with model content. After <c>turnComplete</c> is <c>true</c>, the client can start the realtime
    /// conversation via <c>realtimeInput</c>.
    /// </summary>
    [JsonPropertyName("initialHistoryInClientContent")]
    public bool? InitialHistoryInClientContent { get; set; }
}
