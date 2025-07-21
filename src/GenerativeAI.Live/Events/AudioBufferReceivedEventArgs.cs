using GenerativeAI.Types;

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
    
    /// <summary>
    /// Gets or sets the transcription of the input audio.
    /// </summary>
    public Transcription? InputTranscription { get; set; }
    
    /// <summary>
    /// Gets or sets the transcription of the output audio.
    /// </summary>
    public Transcription? OutputTranscription { get; set; }

    /// <summary>
    /// Initializes a new instance of the AudioBufferReceivedEventArgs class.
    /// </summary>
    /// <param name="buffer">The audio buffer data.</param>
    /// <param name="audioHeaderInfo">The audio header information.</param>
    public AudioBufferReceivedEventArgs(byte[] buffer, AudioHeaderInfo audioHeaderInfo)
    {
        this.Buffer = buffer;
        HeaderInfo = audioHeaderInfo;
    }
}