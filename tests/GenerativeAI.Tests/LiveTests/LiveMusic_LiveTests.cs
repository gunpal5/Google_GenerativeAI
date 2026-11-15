using GenerativeAI.Live;
using GenerativeAI.Live.Events;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live test for Lyria RealTime music generation (<see cref="LiveMusicClient"/>). Skipped unless GOOGLE_API_KEY or
/// GEMINI_API_KEY is set. Stops as soon as the first audio chunk arrives.
/// </summary>
public class LiveMusic_LiveTests : TestBase
{
    public LiveMusic_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public async Task Connect_Prompt_Play_ReceivesAudio_Stop()
    {
        var platform = LiveTestHelpers.RequireGeminiPlatform();
        var ct = TestContext.Current.CancellationToken;

        using var client = new LiveMusicClient(platform, GoogleAIModels.Music.LyriaRealtimeExp);
        var firstChunk = new TaskCompletionSource<LiveMusicAudioChunkReceivedEventArgs>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var closed = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.AudioChunkReceived += (_, e) =>
        {
            if (e.Data.Length > 0)
                firstChunk.TrySetResult(e);
        };
        client.FilteredPromptReceived += (_, e) => Console?.WriteLine($"Prompt filtered: {e.Text} ({e.FilteredReason})");
        client.Disconnected += (_, e) =>
        {
            if (e is ErrorMessageEventArgs message && !string.IsNullOrEmpty(message.ErrorMessage))
                closed.TrySetResult(message.ErrorMessage);
        };
        client.ErrorOccurred += (_, e) => Console?.WriteLine($"Live Music error: {e.GetException().Message}");

        try
        {
            // Waits for setupComplete (throws TimeoutException after SetupTimeout).
            await client.ConnectAsync(cancellationToken: ct);
            client.IsSetupComplete.ShouldBeTrue();

            await client.SetWeightedPromptsAsync(new[] { new WeightedPrompt { Text = "calm piano", Weight = 1.0 } }, ct);
            await client.SetMusicGenerationConfigAsync(new LiveMusicGenerationConfig { Bpm = 90 }, ct);
            await client.PlayAsync(ct);

            var timeout = Task.Delay(TimeSpan.FromSeconds(20), ct);
            var finished = await Task.WhenAny(firstChunk.Task, closed.Task, timeout);
            if (finished == closed.Task)
                Assert.Fail($"The server closed the Live Music session: {await closed.Task}");
            if (finished == timeout)
                Assert.Fail("No audio chunk was received within 20 seconds of play.");

            var chunk = await firstChunk.Task;
            Console?.WriteLine(
                $"First chunk: {chunk.Data.Length} bytes, {chunk.MimeType}, {chunk.SampleRate} Hz, {chunk.Channels} ch");
            chunk.Data.Length.ShouldBeGreaterThan(0);

            await client.StopAsync(ct);
        }
        finally
        {
            await client.DisconnectAsync(CancellationToken.None);
        }
    }
}
