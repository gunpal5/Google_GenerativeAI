using Websocket.Client;

namespace GenerativeAI.Live;

/// <summary>
/// Provides event data after client creation.
/// </summary>
public class ClientCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the newly-created WebSocket client instance.
    /// </summary>
    public IWebsocketClient Client { get; }

    /// <summary>
    /// Initializes a new instance of the ClientCreatedEventArgs class.
    /// </summary>
    /// <param name="client">The newly-created WebSocket client instance.</param>
    public ClientCreatedEventArgs(IWebsocketClient client)
    {
        Client = client;
    }
}