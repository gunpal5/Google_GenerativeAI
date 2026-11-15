using GenerativeAI.Clients;
using GenerativeAI.Live;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.LiveTests;

/// <summary>
/// Live tests for the Live API (<see cref="MultiModalLiveClient"/>) and ephemeral auth tokens
/// (<see cref="AuthTokensClient"/>). Skipped unless GOOGLE_API_KEY or GEMINI_API_KEY is set.
/// Responses use the AUDIO modality (what the Live models support) with output transcription, and every prompt asks
/// for a one-word answer so each session costs a few seconds of audio.
/// </summary>
public class MultiModalLive_LiveTests : TestBase
{
    private const string LiveModel = GoogleAIModels.Gemini38Live;
    private static readonly TimeSpan SetupTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(45);

    public MultiModalLive_LiveTests(ITestOutputHelper helper) : base(helper)
    {
    }

    private static GenerationConfig AudioConfig() => new()
    {
        ResponseModalities = new List<Modality> { Modality.AUDIO }
    };

    [Fact]
    public async Task EphemeralToken_ConnectsToConstrainedEndpoint()
    {
        var platform = LiveTestHelpers.RequireGeminiPlatform();
        var ct = TestContext.Current.CancellationToken;
        var tokens = new AuthTokensClient(platform);

        // Tokens are short-lived and single-use; they expire on their own (there is no delete API).
        var token = await LiveTestHelpers.OrSkipAsync(() => tokens.CreateAsync(new CreateAuthTokenConfig
        {
            Uses = 1,
            ExpireTime = DateTime.UtcNow.AddMinutes(10),
            NewSessionExpireTime = DateTime.UtcNow.AddMinutes(2),
            LiveConnectConstraints = new LiveClientSetup
            {
                Model = LiveModel,
                GenerationConfig = AudioConfig()
            }
        }, ct), "Ephemeral auth tokens");

        token.Name.ShouldNotBeNullOrEmpty();
        token.Name!.ShouldStartWith("auth_tokens/");
        Console?.WriteLine($"Created token (expires {token.ExpireTime})");

        using var client = new MultiModalLiveClient(new GoogleAIPlatformAdapter(token.Name), LiveModel, AudioConfig())
        {
            OutputAudioTranscriptionEnabled = true
        };
        var watcher = new LiveSessionWatcher(client, Console);
        try
        {
            await client.ConnectAsync(cancellationToken: ct);
            await watcher.WaitAsync(watcher.SetupComplete.Task, SetupTimeout, "setupComplete");

            await client.SentTextAsync("Reply with one word: hello", ct);
            await watcher.WaitAsync(watcher.Response, ResponseTimeout, "a model response");
            await watcher.WaitAsync(watcher.TurnComplete, ResponseTimeout, "turnComplete");

            Console?.WriteLine($"Text/transcription: '{watcher.Text}', audio bytes: {watcher.AudioBytes}");
            (watcher.AudioBytes > 0 || !string.IsNullOrWhiteSpace(watcher.Text)).ShouldBeTrue();
        }
        finally
        {
            await client.DisconnectAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task ManualActivity_SessionResumption_AndContextUpdate()
    {
        var platform = LiveTestHelpers.RequireGeminiPlatform();
        var ct = TestContext.Current.CancellationToken;

        using var client = new MultiModalLiveClient(platform, LiveModel, AudioConfig(),
            systemInstruction: "You are a helpful assistant. Answer in one word.")
        {
            OutputAudioTranscriptionEnabled = true,
            SessionResumption = new SessionResumptionConfig(),
            RealtimeInputConfig = new RealtimeInputConfig
            {
                AutomaticActivityDetection = new AutomaticActivityDetection { Disabled = true }
            }
        };
        var watcher = new LiveSessionWatcher(client, Console);
        try
        {
            await client.ConnectAsync(cancellationToken: ct);
            await watcher.WaitAsync(watcher.SetupComplete.Task, SetupTimeout, "setupComplete");

            // Turn 1: manual activity signalling around audio (1 s of 16 kHz PCM silence). The server rejects
            // realtime *text* between activity markers ("Precondition check failed"), so the markers wrap audio.
            await client.SendActivityStartAsync(ct);
            await client.SendAudioAsync(new byte[32000], cancellationToken: ct);
            await client.SendActivityEndAsync(ct);
            await watcher.WaitAsync(watcher.TurnComplete, ResponseTimeout, "turnComplete of the first turn");
            Console?.WriteLine($"Turn 1: '{watcher.Text}', audio bytes: {watcher.AudioBytes}");

            // Session resumption handles are sent by the server at its own pace; log rather than fail.
            if (string.IsNullOrEmpty(client.LastSessionResumptionHandle))
                Console?.WriteLine(
                    $"No session resumption handle received yet ({watcher.ResumptionUpdates} updates); not failing on it.");
            else
                Console?.WriteLine("LastSessionResumptionHandle was set.");

            // Turn 2: update the system instruction mid-session; the session must keep working.
            await client.SendContextUpdateAsync("You are a pirate. Answer in one word.", cancellationToken: ct);
            watcher.StartNewTurn();
            await client.SendRealtimeTextAsync("Say a greeting.", ct);
            await watcher.WaitAsync(watcher.TurnComplete, ResponseTimeout,
                "turnComplete after the context update");
            Console?.WriteLine($"After context update: '{watcher.Text}', audio bytes: {watcher.AudioBytes}");
            (watcher.AudioBytes > 0 || !string.IsNullOrWhiteSpace(watcher.Text)).ShouldBeTrue();
            watcher.Failure.Task.IsFaulted.ShouldBeFalse();
        }
        finally
        {
            await client.DisconnectAsync(CancellationToken.None);
        }
    }
}
