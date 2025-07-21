using System.Net.WebSockets;
using Websocket.Client;

namespace GenerativeAI.Live;

/// <summary>
/// Provides extension for WebsocketClient.
/// </summary>
public static class WebSocketClientExtensions
{
    /// <summary>
    /// Extends a ClientWebSocket with reconnection capabilities.
    /// </summary>
    /// <param name="webSocketClient">The ClientWebSocket to extend with reconnection functionality.</param>
    /// <param name="url">The WebSocket URL to connect to.</param>
    /// <returns>An IWebsocketClient with reconnection capabilities enabled.</returns>
    public static IWebsocketClient WithReconnect(this ClientWebSocket webSocketClient, string url)
    {
        var client = new WebsocketClient(new Uri(url), () => webSocketClient)
        {
            IsReconnectionEnabled = true,
            ReconnectTimeout = TimeSpan.FromSeconds(30)
        };

        return client;
    }
}