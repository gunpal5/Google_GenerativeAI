using System.Text;

namespace GenerativeAI.Live.Helper;

/// <summary>
/// Provides utility methods for working with audio data, including methods to add WAV headers to raw audio data 
/// and validate WAV file headers.
/// </summary>
public static class AudioHelper
{
    /// <summary>
    /// Adds a WAV file header to the given raw audio data.
    /// </summary>
    /// <param name="audioData">The raw audio data to which the header will be added.</param>
    /// <param name="numChannels">The number of audio channels (e.g., 1 for mono, 2 for stereo).</param>
    /// <param name="sampleRate">The sample rate of the audio (e.g., 44100 for 44.1kHz).</param>
    /// <param name="bitsPerSample">The number of bits per sample (e.g., 16 for 16-bit audio).</param>
    /// <returns>A byte array containing the audio data with the WAV header prepended.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="audioData"/> is null.</exception>
    public static byte[] AddWaveHeader(byte[] audioData, int numberOfChannels, int sampleRate, int bitsPerSample2)
    {
        if (audioData == null)
        {
            throw new ArgumentNullException(nameof(audioData));
        }

        var numChannels =(ushort) BitConverter.ToUInt16(BitConverter.GetBytes(numberOfChannels));
        var bitsPerSample =(ushort) BitConverter.ToUInt16(BitConverter.GetBytes(bitsPerSample2));


        using (var memoryStream = new MemoryStream())
        using (var binaryWriter = new BinaryWriter(memoryStream))
        {
            // RIFF chunk
            binaryWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
            binaryWriter.Write(36 + audioData.Length); // ChunkSize (36 + data size)
            binaryWriter.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            binaryWriter.Write(Encoding.ASCII.GetBytes("fmt "));
            binaryWriter.Write(16); // Subchunk1Size (16 for PCM)
            binaryWriter.Write((ushort)1); // AudioFormat (1 for PCM)
            binaryWriter.Write(numChannels);
            binaryWriter.Write(sampleRate);
            int byteRate = sampleRate * numChannels * bitsPerSample / 8;
            binaryWriter.Write(byteRate);
            binaryWriter.Write((ushort)(numChannels * bitsPerSample / 8)); // BlockAlign
            binaryWriter.Write(bitsPerSample);

            // data chunk
            binaryWriter.Write(Encoding.ASCII.GetBytes("data"));
            binaryWriter.Write(audioData.Length); // Subchunk2Size (data size)
            binaryWriter.Write(audioData);

            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// Validates whether the given byte array contains a valid WAV file header.
    /// </summary>
    /// <param name="buffer">The byte array to validate.</param>
    /// <returns><c>true</c> if the buffer contains a valid WAV header; otherwise, <c>false</c>.</returns>
    public static bool IsValidWaveHeader(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 44) // Minimum WAV header size
        {
            return false;
        }

        using (var stream = new MemoryStream(buffer))
        using (var reader = new BinaryReader(stream))
        {
            try
            {
                // RIFF chunk
                string riff = Encoding.ASCII.GetString(reader.ReadBytes(4));
                uint chunkSize = reader.ReadUInt32();
                string wave = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (riff != "RIFF" || wave != "WAVE")
                {
                    return false;
                }

                // fmt chunk
                string fmt = Encoding.ASCII.GetString(reader.ReadBytes(4));
                uint fmtChunkSize = reader.ReadUInt32();

                if (fmt != "fmt ")
                {
                    return false;
                }

                ushort audioFormat = reader.ReadUInt16();
                ushort numChannels = reader.ReadUInt16();
                uint sampleRate = reader.ReadUInt32();
                uint byteRate = reader.ReadUInt32();
                ushort blockAlign = reader.ReadUInt16();
                ushort bitsPerSample = reader.ReadUInt16();

                if (audioFormat != 1) // PCM
                {
                    return false;
                }

                // data chunk
                string data = Encoding.ASCII.GetString(reader.ReadBytes(4));
                uint dataChunkSize = reader.ReadUInt32();

                if (data != "data")
                {
                    return false;
                }

                return true; 
            }
            catch (Exception)
            {
                return false; 
            }
        }
    }
}