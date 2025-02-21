using GenerativeAI.Types;

namespace GenerativeAI.Live;

/// <summary>
/// Provides event data.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    public BidiResponsePayload Payload { get; }

    public MessageReceivedEventArgs(BidiResponsePayload payload)
    {
        Payload = payload;
    }
}