using GenerativeAI.Types;

namespace GenerativeAI.Live;

/// <summary>
/// Provides data for the <see cref="LiveMusicClient.AudioChunkReceived"/> event.
/// </summary>
public class LiveMusicAudioChunkReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveMusicAudioChunkReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="chunk">The audio chunk received from the server.</param>
    /// <param name="sampleRate">The sample rate of the audio, in Hz.</param>
    /// <param name="channels">The number of audio channels.</param>
    public LiveMusicAudioChunkReceivedEventArgs(AudioChunk chunk, int sampleRate, int channels)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(chunk);
#else
        if (chunk == null) throw new ArgumentNullException(nameof(chunk));
#endif
        Chunk = chunk;
        SampleRate = sampleRate;
        Channels = channels;
    }

    /// <summary>
    /// Gets the audio chunk as received from the server.
    /// </summary>
    public AudioChunk Chunk { get; }

    /// <summary>
    /// Gets the decoded raw audio bytes (16-bit PCM unless <see cref="MimeType"/> says otherwise).
    /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays - raw audio buffer
    public byte[] Data => Chunk.Data ?? Array.Empty<byte>();
#pragma warning restore CA1819

    /// <summary>
    /// Gets the MIME type of the audio chunk (for example <c>audio/l16;rate=48000;channels=2</c>), if provided.
    /// </summary>
    public string? MimeType => Chunk.MimeType;

    /// <summary>
    /// Gets the sample rate of the audio, in Hz. Parsed from the <c>rate=</c> parameter of the MIME type,
    /// or 48000 (Lyria RealTime's output rate) when not specified.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the number of audio channels. Parsed from the <c>channels=</c> parameter of the MIME type,
    /// or 2 (Lyria RealTime outputs stereo) when not specified.
    /// </summary>
    public int Channels { get; }

    /// <summary>
    /// Gets the prompts and config used to generate this chunk, if provided by the server.
    /// </summary>
    public LiveMusicSourceMetadata? SourceMetadata => Chunk.SourceMetadata;
}

/// <summary>
/// Provides data for the <see cref="LiveMusicClient.FilteredPromptReceived"/> event.
/// </summary>
public class LiveMusicFilteredPromptEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveMusicFilteredPromptEventArgs"/> class.
    /// </summary>
    /// <param name="filteredPrompt">The prompt that was filtered.</param>
    public LiveMusicFilteredPromptEventArgs(LiveMusicFilteredPrompt filteredPrompt)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(filteredPrompt);
#else
        if (filteredPrompt == null) throw new ArgumentNullException(nameof(filteredPrompt));
#endif
        FilteredPrompt = filteredPrompt;
    }

    /// <summary>
    /// Gets the prompt that was filtered, with the reason.
    /// </summary>
    public LiveMusicFilteredPrompt FilteredPrompt { get; }

    /// <summary>
    /// Gets the text of the filtered prompt.
    /// </summary>
    public string? Text => FilteredPrompt.Text;

    /// <summary>
    /// Gets the reason the prompt was filtered.
    /// </summary>
    public string? FilteredReason => FilteredPrompt.FilteredReason;
}

/// <summary>
/// Provides data for the <see cref="LiveMusicClient.MessageReceived"/> event.
/// </summary>
public class LiveMusicMessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveMusicMessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">The message received from the server.</param>
    public LiveMusicMessageReceivedEventArgs(LiveMusicServerMessage message)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(message);
#else
        if (message == null) throw new ArgumentNullException(nameof(message));
#endif
        Message = message;
    }

    /// <summary>
    /// Gets the message received from the server.
    /// </summary>
    public LiveMusicServerMessage Message { get; }
}
