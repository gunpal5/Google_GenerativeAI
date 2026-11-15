using System.Globalization;
using System.Text.Json.Serialization;

namespace GenerativeAI.Types;

/// <summary>
/// Server notice that the connection will be closed soon. The client should
/// reconnect (for example using a session resumption handle) before <see cref="TimeLeft"/> elapses.
/// </summary>
public class LiveServerGoAway
{
    /// <summary>
    /// The remaining time before the connection is terminated, as a protobuf duration string (for example "30s").
    /// </summary>
    [JsonPropertyName("timeLeft")]
    public string? TimeLeft { get; set; }

    /// <summary>
    /// <see cref="TimeLeft"/> parsed as a <see cref="TimeSpan"/>, or null if it is missing or not a valid duration.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? TimeLeftDuration
    {
        get
        {
            var value = TimeLeft;
            if (value is null || value.Length == 0 || value[value.Length - 1] != 's')
                return null;

#if NET6_0_OR_GREATER
            var number = value.AsSpan(0, value.Length - 1);
#else
            var number = value.Substring(0, value.Length - 1);
#endif
            return double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                ? TimeSpan.FromSeconds(seconds)
                : null;
        }
    }

    /// <summary>
    /// Not sent by the Live API; always null. Use <see cref="TimeLeft"/>.
    /// </summary>
    [Obsolete("The Live API GoAway message only contains timeLeft. Use TimeLeft or TimeLeftDuration.", false)]
    [JsonPropertyName("errorCode")]
    public int? ErrorCode { get; set; }

    /// <summary>
    /// Not sent by the Live API; always null. Use <see cref="TimeLeft"/>.
    /// </summary>
    [Obsolete("The Live API GoAway message only contains timeLeft. Use TimeLeft or TimeLeftDuration.", false)]
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Not sent by the Live API; always null. A GoAway always means the client should reconnect.
    /// </summary>
    [Obsolete("The Live API GoAway message only contains timeLeft. A GoAway always means the client should reconnect.", false)]
    [JsonPropertyName("reconnect")]
    public bool? Reconnect { get; set; }

    /// <summary>
    /// Not sent by the Live API; always null. Use <see cref="TimeLeft"/>.
    /// </summary>
    [Obsolete("The Live API GoAway message only contains timeLeft. Use TimeLeft or TimeLeftDuration.", false)]
    [JsonPropertyName("retryAfterSeconds")]
    public int? RetryAfterSeconds { get; set; }
}
