using System.Net.WebSockets;
using Websocket.Client;

namespace GenerativeAI.Live;

/// <summary>
/// Provides extension for WebsocketClient.
/// </summary>
public static class WebSocketClientExtensions
{
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