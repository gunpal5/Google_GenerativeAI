using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Update of the session resumption state. Only sent if <see cref="SessionResumptionConfig"/> was set in the setup message.
/// </summary>
public class LiveServerSessionResumptionUpdate
{
    /// <summary>
    /// New handle that represents a state that can be resumed. Empty if <see cref="Resumable"/> is false.
    /// Pass it as <see cref="SessionResumptionConfig.Handle"/> when reconnecting.
    /// </summary>
    [JsonPropertyName("newHandle")]
    public string? NewHandle { get; set; }

    /// <summary>
    /// True if the current session can be resumed at this point. Resumption is not possible at some points,
    /// for example while the model is executing function calls or generating.
    /// </summary>
    [JsonPropertyName("resumable")]
    public bool? Resumable { get; set; }

    /// <summary>
    /// Index of the last message sent by the client that is included in the state represented by this handle.
    /// Only present when <see cref="SessionResumptionConfig.Transparent"/> is enabled. Messages after this index
    /// should be re-sent after reconnecting.
    /// </summary>
    [JsonPropertyName("lastConsumedClientMessageIndex")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? LastConsumedClientMessageIndex { get; set; }

    /// <summary>
    /// Not sent by the Live API; always null. Use <see cref="NewHandle"/>.
    /// </summary>
    [Obsolete("Not part of the Live API. Use NewHandle.", false)]
    [JsonPropertyName("resumptionToken")]
    public string? ResumptionToken { get; set; }

    /// <summary>
    /// Not sent by the Live API; always null.
    /// </summary>
    [Obsolete("Not part of the Live API.", false)]
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Not sent by the Live API; always null. Use <see cref="Resumable"/>.
    /// </summary>
    [Obsolete("Not part of the Live API. Use Resumable.", false)]
    [JsonPropertyName("status")]
    public SessionResumptionStatus? Status { get; set; }
}
