using System.Net.WebSockets;

namespace GenerativeAI.Clients;

/// <summary>
/// [Experimental] Options for opening a live interaction WebSocket with
/// <see cref="InteractionsClient.ConnectAsync(string, InteractionConnectOptions?, CancellationToken)"/>.
/// </summary>
public sealed class InteractionConnectOptions
{
    /// <summary>
    /// The API version of the endpoint (e.g. <c>v1beta</c>). Defaults to the platform's API version.
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Additional handshake headers. They replace headers of the same name (case-insensitively), except that an
    /// ephemeral <c>auth_tokens/...</c> API key always wins over <c>x-goog-api-key</c>/<c>Authorization</c>.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// The time allowed for opening the WebSocket and sending the initial event (default 30 seconds).
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The <c>api-revision</c> handshake header (default <see cref="InteractionWebSocketProtocol.DefaultApiRevision"/>);
    /// set to <c>null</c> to omit it.
    /// </summary>
    public string? ApiRevision { get; set; } = InteractionWebSocketProtocol.DefaultApiRevision;

    /// <summary>
    /// Configures the <see cref="ClientWebSocketOptions"/> (proxy, certificates, keep-alive) before the handshake
    /// headers are applied; handshake headers set here are overwritten by the SDK's headers.
    /// </summary>
    public Action<ClientWebSocketOptions>? ConfigureWebSocket { get; set; }
}
