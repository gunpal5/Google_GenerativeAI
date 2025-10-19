using Websocket.Client;

namespace GenerativeAI.Live;

/// <summary>
/// Provides event data after client creation.
/// </summary>
public class ClientCreatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the transcription of the output audio.
    /// </summary>
    public IWebsocketClient Client { get; set; }

    /// <summary>
    /// Initializes a new instance of the ClientCreatedEventArgs class.
    /// </summary>
    /// <param name="client">The newly-created WebSocket client instance.</param>
    public ClientCreatedEventArgs(IWebsocketClient client)
    {
        Client = client;
    }
}