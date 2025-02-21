namespace GenerativeAI.Live;

/// <summary>
/// Represents the header information of an audio file.
/// </summary>
public class AudioHeaderInfo
{
    /// <summary>
    /// Indicates whether the audio file contains a header.
    /// </summary>
    public bool HasHeader { get; set; }

    /// <summary>
    /// The sample rate of the audio file in Hz.
    /// </summary>
    public int SampleRate { get; set; }

    /// <summary>
    /// The number of audio channels (e.g., 1 for mono, 2 for stereo).
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// The number of bits per sample in the audio data.
    /// </summary>
    public int BitsPerSample { get; set; }
}