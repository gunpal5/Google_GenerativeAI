namespace GenerativeAI.Live;

/// <summary>
/// Provides event data.
/// </summary>
public class AudioBufferReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the audio data buffer.
    /// </summary>
    public byte[] Buffer { get; set; }

    /// <summary>
    /// Gets or sets the header information for the audio data.
    /// </summary>
    public AudioHeaderInfo HeaderInfo { get; set; }

    public AudioBufferReceivedEventArgs(byte[] buffer, AudioHeaderInfo audioHeaderInfo)
    {
        this.Buffer = buffer;
        HeaderInfo = audioHeaderInfo;
    }
}