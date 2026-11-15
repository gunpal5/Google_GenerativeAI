using GenerativeAI.Types;

namespace GenerativeAI.Live;

/// <summary>
/// Provides event data.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the payload of the received message.
    /// </summary>
    public LiveServerMessage Payload { get; }

    /// <summary>
    /// Initializes a new instance of the MessageReceivedEventArgs class.
    /// </summary>
    /// <param name="payload">The payload of the received message.</param>
    public MessageReceivedEventArgs(LiveServerMessage payload)
    {
        Payload = payload;
    }
}