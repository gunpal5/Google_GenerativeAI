using System.IO;
using NAudio.Wave;

namespace TwoWayAudioCommunicationWpf.AudioHelper;

public class NAudioHelper
{
    public event EventHandler<byte[]> AudioDataReceived;
    private BufferedWaveProvider? bufferedWaveProvider = null; //new BufferedWaveProvider(new WaveFormat(16000, 16, 1));
    private WaveOutEvent? waveOut = null;

    public bool IsRecording;

    public void BufferWavePlay(byte[] bytes, int sampleRate = 24000, int channels = 1, int bitsPerSample = 16)
    {
        if (bufferedWaveProvider == null)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                //... (write WAV header - same as previous example)
                writer.Write(bytes); // Write the raw audio data

                // 2. Use BufferedWaveProvider:
                memoryStream.Position = 0; // Reset stream position
                var waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);
                bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
                bufferedWaveProvider.BufferDuration = new TimeSpan(0, 15, 0);
                // Add the entire WAV data to the buffer
                bufferedWaveProvider.AddSamples(memoryStream.ToArray(), 0, (int)memoryStream.Length);

                waveOut = new WaveOutEvent();
                waveOut.Init(bufferedWaveProvider);
                waveOut.Play();
            }
        }
        else
        {
            bufferedWaveProvider.AddSamples(bytes, 0, bytes.Length);
        }
    }

    public void StopPlayback()
    {
        waveOut?.Stop();
        this.bufferedWaveProvider = null;
    }
    public void ClearPlayback()
    {
        this.bufferedWaveProvider = null;
    }
    
    private WaveInEvent waveIn;
    private WaveFileWriter writer;

    public void StartRecording(int deviceIndex, int sampleRate = 16000, int channels = 1, int bitsPerSample = 16)
    {
        waveIn = new WaveInEvent();
        waveIn.DeviceNumber = deviceIndex;
        waveIn.WaveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);
        waveIn.DataAvailable += WaveIn_DataAvailable;
        waveIn.RecordingStopped += WaveIn_RecordingStopped;

        waveIn.StartRecording();
        IsRecording = true;
    }

    public void StopRecording()
    {
        waveIn?.StopRecording();
        IsRecording = false;
    }

    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
    {
        //Detect Voice and Send Event
       // if(DetectVoice(e))
            this.AudioDataReceived?.Invoke(this, e.Buffer);
    }

    bool DetectVoice(WaveInEventArgs e)
    {
        // Convert bytes to samples (shorts)
        short[] samples = new short[e.BytesRecorded / 2];
        Buffer.BlockCopy(e.Buffer, 0, samples, 0, e.BytesRecorded);

        // Calculate RMS (Root Mean Square) energy
        double rms = CalculateRMS(samples);

        // Threshold for voice activity
        double threshold = 200; // Adjust this value

        return (rms > threshold);
    }
    
    private double CalculateRMS(short[] samples)
    {
        double sum = 0;
        foreach (short sample in samples)
        {
            sum += sample * sample;
        }
        double mean = sum / samples.Length;
        return Math.Sqrt(mean);
    }

    private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
    {
        
    }
    public static List<string> GetAvailableMicrophones()
    {
        List<string> microphones = new List<string>();

        for (int n = 0; n < WaveIn.DeviceCount; n++)
        {
            var capabilities = WaveIn.GetCapabilities(n);
            microphones.Add(capabilities.ProductName);
        }

        return microphones;
    }

    public async Task PlayAudioAsync(string responseFile)
    {
        var bytes = await File.ReadAllBytesAsync(responseFile);
        BufferWavePlay(bytes);
    }

    public double GetMicLevel(byte[] bytes)
    {
        short[] samples = new short[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);

        var value = samples.Max(b => b);
        
        //var ushortValue=  BitConverter.ToUInt32(BitConverter.GetBytes(ushort.MaxValue));

        var maxLevel = value*100/(double)65535;
        return maxLevel;
    }
}