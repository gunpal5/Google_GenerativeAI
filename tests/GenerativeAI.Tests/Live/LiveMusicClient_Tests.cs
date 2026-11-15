using GenerativeAI.Core;
using GenerativeAI.Live;
using GenerativeAI.Live.Events;
using GenerativeAI.Types;
using Moq;
using Shouldly;
using Websocket.Client;

namespace GenerativeAI.Tests.Live;

/// <summary>
/// Offline unit tests for <see cref="LiveMusicClient"/> using an injected in-memory WebSocket.
/// </summary>
public class LiveMusicClient_Tests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    private const string SetupJson = "{\"setup\":{\"model\":\"models/lyria-realtime-exp\"}}";
    private const string SetupCompleteJson = "{\"setupComplete\":{}}";

    /// <summary>Creates a client whose fake socket answers the setup message with setupComplete.</summary>
    private static async Task<(LiveMusicClient Client, FakeWebsocketClient Socket)> ConnectAsync(
        Action<LiveMusicClient>? configure = null)
    {
        var client = new LiveMusicClient("test-key");
        FakeWebsocketClient? socket = null;
        client.WebsocketClientFactory = url =>
        {
            socket = new FakeWebsocketClient(url);
            socket.TextSent += text =>
            {
                if (text.StartsWith("{\"setup\"", StringComparison.Ordinal))
                    socket.ReceiveText(SetupCompleteJson);
            };
            return socket;
        };
        configure?.Invoke(client);

        await client.ConnectAsync(cancellationToken: TestContext.Current.CancellationToken);

        socket.ShouldNotBeNull();
        return (client, socket);
    }

    private static Task<T> WaitForEventAsync<T>(Action<EventHandler<T>> subscribe)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        subscribe((_, e) => tcs.TrySetResult(e));
        return tcs.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("Condition was not met in time.");
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }

    #region Construction and connection

    [Fact]
    public void Constructor_WithVertexAdapter_Throws()
    {
        var adapter = new Mock<IPlatformAdapter>().Object;
        Should.Throw<NotSupportedException>(() => new LiveMusicClient(adapter));
    }

    [Fact]
    public void Constructor_NormalizesModelName()
    {
        using var client = new LiveMusicClient("test-key", "lyria-realtime-exp");
        client.ModelName.ShouldBe("models/lyria-realtime-exp");
        using var defaultClient = new LiveMusicClient("test-key");
        defaultClient.ModelName.ShouldBe(GoogleAIModels.Music.LyriaRealtimeExp);
    }

    [Fact]
    public async Task ConnectAsync_PassesLiveMusicUrlToFactory_AndSendsSetup()
    {
        string? receivedUrl = null;
        using var client = new LiveMusicClient(new GoogleAIPlatformAdapter("test-key"));
        FakeWebsocketClient? socket = null;
        client.WebsocketClientFactory = url =>
        {
            receivedUrl = url;
            return socket = new FakeWebsocketClient(url);
        };
        var connected = false;
        client.Connected += (_, _) => connected = true;

        await client.ConnectAsync(waitForSetupComplete: false, TestContext.Current.CancellationToken);

        receivedUrl.ShouldBe(
            "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1alpha.GenerativeService.BidiGenerateMusic?key=test-key");
        socket.ShouldNotBeNull();
        socket.IsRunning.ShouldBeTrue();
        client.Client.ShouldBeSameAs(socket);
        connected.ShouldBeTrue();
        socket.SentMessages.ShouldBe(new[] { SetupJson });
        client.IsSetupComplete.ShouldBeFalse();
    }

    [Fact]
    public async Task ConnectAsync_UsesConfiguredApiVersion()
    {
        string? receivedUrl = null;
        using var client = new LiveMusicClient("test-key") { ApiVersion = "v1beta" };
        client.WebsocketClientFactory = url => new FakeWebsocketClient(receivedUrl = url);

        await client.ConnectAsync(false, TestContext.Current.CancellationToken);

        receivedUrl.ShouldNotBeNull();
        receivedUrl.ShouldContain("google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateMusic");
    }

    [Fact]
    public async Task ConnectAsync_WaitsForSetupComplete_AndRaisesSetupCompleted()
    {
        var setupCompleted = 0;
        var (client, socket) = await ConnectAsync(c => c.SetupCompleted += (_, _) => Interlocked.Increment(ref setupCompleted));
        using (client)
        {
            client.IsSetupComplete.ShouldBeTrue();
            await WaitUntilAsync(() => Volatile.Read(ref setupCompleted) == 1);
            socket.SentMessages.ShouldBe(new[] { SetupJson });
        }
    }

    [Fact]
    public async Task ConnectAsync_TimesOut_WhenSetupNotAcknowledged()
    {
        using var client = new LiveMusicClient("test-key") { SetupTimeout = TimeSpan.FromMilliseconds(100) };
        client.WebsocketClientFactory = url => new FakeWebsocketClient(url);
        System.IO.ErrorEventArgs? error = null;
        client.ErrorOccurred += (_, e) => error = e;

        await Should.ThrowAsync<TimeoutException>(() => client.ConnectAsync(true, TestContext.Current.CancellationToken));
        error.ShouldNotBeNull();
    }

    [Fact]
    public async Task ConnectAsync_Fails_WhenConnectionClosesBeforeSetupComplete()
    {
        using var client = new LiveMusicClient("test-key");
        client.WebsocketClientFactory = url =>
        {
            var socket = new FakeWebsocketClient(url);
            socket.TextSent += _ => socket.SimulateDisconnection(DisconnectionType.ByServer, "Model not found");
            return socket;
        };

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => client.ConnectAsync(true, TestContext.Current.CancellationToken));
        ex.Message.ShouldContain("Model not found");
    }

    [Fact]
    public async Task ConnectAsync_Twice_ReleasesPreviousSocketAndSubscriptions()
    {
        var sockets = new List<FakeWebsocketClient>();
        using var client = new LiveMusicClient("test-key");
        client.WebsocketClientFactory = url =>
        {
            var s = new FakeWebsocketClient(url);
            sockets.Add(s);
            return s;
        };

        await client.ConnectAsync(false, TestContext.Current.CancellationToken);
        await client.ConnectAsync(false, TestContext.Current.CancellationToken);

        sockets.Count.ShouldBe(2);
        sockets[0].IsDisposed.ShouldBeTrue();
        sockets[0].HadMessageSubscribersAtDispose.ShouldBeFalse();
        client.Client.ShouldBeSameAs(sockets[1]);
    }

    [Fact]
    public async Task DisconnectAsync_StopsAndReleasesSocket_AndSendThrowsAfterwards()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.DisconnectAsync(TestContext.Current.CancellationToken);

            socket.IsRunning.ShouldBeFalse();
            socket.IsDisposed.ShouldBeTrue();
            socket.HadMessageSubscribersAtDispose.ShouldBeFalse();
            client.Client.ShouldBeNull();
            await Should.ThrowAsync<InvalidOperationException>(() => client.PlayAsync(TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task Dispose_DisconnectsSocket()
    {
        var (client, socket) = await ConnectAsync();
        client.Dispose();
        socket.IsDisposed.ShouldBeTrue();
        await Should.ThrowAsync<ObjectDisposedException>(() => client.ConnectAsync(false, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Send_WhenNotConnected_ThrowsAndRaisesError()
    {
        using var client = new LiveMusicClient("test-key");
        System.IO.ErrorEventArgs? error = null;
        client.ErrorOccurred += (_, e) => error = e;

        await Should.ThrowAsync<InvalidOperationException>(() => client.PlayAsync(TestContext.Current.CancellationToken));
        error.ShouldNotBeNull();
    }

    #endregion

    #region Client messages

    [Fact]
    public async Task SetWeightedPromptsAsync_SendsClientContent()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SetWeightedPromptsAsync(new[]
            {
                new WeightedPrompt { Text = "minimal techno", Weight = 1.0 },
                new WeightedPrompt { Text = "piano", Weight = 0.5 }
            }, TestContext.Current.CancellationToken);

            socket.SentMessages[^1].ShouldBe(
                "{\"clientContent\":{\"weightedPrompts\":[{\"text\":\"minimal techno\",\"weight\":1},{\"text\":\"piano\",\"weight\":0.5}]}}");
        }
    }

    [Fact]
    public async Task SetWeightedPromptsAsync_SingleText_UsesWeightOne()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SetWeightedPromptsAsync("jazz", TestContext.Current.CancellationToken);
            socket.SentMessages[^1].ShouldBe(
                "{\"clientContent\":{\"weightedPrompts\":[{\"text\":\"jazz\",\"weight\":1}]}}");
        }
    }

    [Fact]
    public async Task SetWeightedPromptsAsync_Empty_Throws()
    {
        var (client, _) = await ConnectAsync();
        using (client)
        {
            await Should.ThrowAsync<ArgumentException>(() =>
                client.SetWeightedPromptsAsync(Array.Empty<WeightedPrompt>(), TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public async Task SetMusicGenerationConfigAsync_SendsAllFieldsInCamelCaseWithEnumStrings()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SetMusicGenerationConfigAsync(new LiveMusicGenerationConfig
            {
                Temperature = 1.1,
                TopK = 40,
                Seed = 42,
                Guidance = 4.0,
                Bpm = 120,
                Density = 0.5,
                Brightness = 0.7,
                Scale = Scale.D_MAJOR_B_MINOR,
                MuteBass = true,
                MuteDrums = false,
                OnlyBassAndDrums = false,
                MusicGenerationMode = MusicGenerationMode.VOCALIZATION
            }, TestContext.Current.CancellationToken);

            socket.SentMessages[^1].ShouldBe(
                "{\"musicGenerationConfig\":{\"temperature\":1.1,\"topK\":40,\"seed\":42,\"guidance\":4,\"bpm\":120," +
                "\"density\":0.5,\"brightness\":0.7,\"scale\":\"D_MAJOR_B_MINOR\",\"muteBass\":true,\"muteDrums\":false," +
                "\"onlyBassAndDrums\":false,\"musicGenerationMode\":\"VOCALIZATION\"}}");
        }
    }

    [Fact]
    public async Task SetMusicGenerationConfigAsync_OmitsUnsetFields()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SetMusicGenerationConfigAsync(new LiveMusicGenerationConfig { Bpm = 90 },
                TestContext.Current.CancellationToken);
            socket.SentMessages[^1].ShouldBe("{\"musicGenerationConfig\":{\"bpm\":90}}");
        }
    }

    [Fact]
    public async Task PlaybackControls_SendExpectedSignals()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var ct = TestContext.Current.CancellationToken;
            await client.PlayAsync(ct);
            await client.PauseAsync(ct);
            await client.StopAsync(ct);
            await client.ResetContextAsync(ct);

            socket.SentMessages.Skip(1).ShouldBe(new[]
            {
                "{\"playbackControl\":\"PLAY\"}",
                "{\"playbackControl\":\"PAUSE\"}",
                "{\"playbackControl\":\"STOP\"}",
                "{\"playbackControl\":\"RESET_CONTEXT\"}"
            });
        }
    }

    #endregion

    #region Server messages

    [Fact]
    public async Task AudioChunk_IsDecodedAndRaised_WithFormatFromMimeType()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var received = WaitForEventAsync<LiveMusicAudioChunkReceivedEventArgs>(h => client.AudioChunkReceived += h);
            var pcm = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            socket.ReceiveText(
                "{\"serverContent\":{\"audioChunks\":[{\"data\":\"" + Convert.ToBase64String(pcm) +
                "\",\"mimeType\":\"audio/l16;rate=44100;channels=1\",\"sourceMetadata\":{\"clientContent\":" +
                "{\"weightedPrompts\":[{\"text\":\"jazz\",\"weight\":1.0}]},\"musicGenerationConfig\":{\"bpm\":100}}}]}}");

            var e = await received;
            e.Data.ShouldBe(pcm);
            e.MimeType.ShouldBe("audio/l16;rate=44100;channels=1");
            e.SampleRate.ShouldBe(44100);
            e.Channels.ShouldBe(1);
            e.SourceMetadata.ShouldNotBeNull();
            e.SourceMetadata!.ClientContent!.WeightedPrompts![0].Text.ShouldBe("jazz");
            e.SourceMetadata.MusicGenerationConfig!.Bpm.ShouldBe(100);
        }
    }

    [Fact]
    public async Task AudioChunks_AreRaisedInOrder_WithDefaultFormat()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var chunks = new List<LiveMusicAudioChunkReceivedEventArgs>();
            client.AudioChunkReceived += (_, e) => { lock (chunks) chunks.Add(e); };

            socket.ReceiveText("{\"serverContent\":{\"audioChunks\":[{\"data\":\"AQI=\"},{\"data\":\"AwQ=\",\"mimeType\":\"audio/l16\"}]}}");
            socket.ReceiveText("{\"serverContent\":{\"audioChunks\":[{\"data\":\"BQY=\"}]}}");

            await WaitUntilAsync(() => { lock (chunks) return chunks.Count == 3; });
            chunks.Select(c => c.Data[0]).ShouldBe(new byte[] { 1, 3, 5 });
            chunks.ShouldAllBe(c => c.SampleRate == 48000 && c.Channels == 2);
        }
    }

    [Fact]
    public async Task BinaryFrame_IsParsed()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var received = WaitForEventAsync<LiveMusicAudioChunkReceivedEventArgs>(h => client.AudioChunkReceived += h);
            socket.ReceiveBinary(System.Text.Encoding.UTF8.GetBytes("{\"serverContent\":{\"audioChunks\":[{\"data\":\"CQo=\"}]}}"));
            (await received).Data.ShouldBe(new byte[] { 9, 10 });
        }
    }

    [Fact]
    public async Task FilteredPrompt_IsRaised()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var filtered = WaitForEventAsync<LiveMusicFilteredPromptEventArgs>(h => client.FilteredPromptReceived += h);

            socket.ReceiveText("{\"filteredPrompt\":{\"text\":\"bad prompt\",\"filteredReason\":\"SAFETY\"}}");

            var e = await filtered;
            e.Text.ShouldBe("bad prompt");
            e.FilteredReason.ShouldBe("SAFETY");
        }
    }

    [Fact]
    public async Task MessageReceived_IsRaisedForEveryMessage()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var message = WaitForEventAsync<LiveMusicMessageReceivedEventArgs>(h => client.MessageReceived += h);
            socket.ReceiveText("{\"filteredPrompt\":{\"text\":\"x\"},\"unknownField\":123}");
            (await message).Message.FilteredPrompt!.Text.ShouldBe("x");
        }
    }

    [Fact]
    public async Task InvalidJson_RaisesError_AndClientKeepsProcessing()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var error = WaitForEventAsync<System.IO.ErrorEventArgs>(h => client.ErrorOccurred += h);
            socket.ReceiveText("{not json");
            (await error).GetException().ShouldNotBeNull();

            var received = WaitForEventAsync<LiveMusicAudioChunkReceivedEventArgs>(h => client.AudioChunkReceived += h);
            socket.ReceiveText("{\"serverContent\":{\"audioChunks\":[{\"data\":\"AQI=\"}]}}");
            (await received).Data.Length.ShouldBe(2);
        }
    }

    [Fact]
    public async Task ThrowingHandler_IsReported_AndDoesNotStopProcessing()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var calls = 0;
            client.AudioChunkReceived += (_, _) =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    throw new InvalidOperationException("handler failure");
            };
            client.ErrorOccurred += (_, _) => throw new InvalidOperationException("error handler failure");
            var error = WaitForEventAsync<System.IO.ErrorEventArgs>(h => client.ErrorOccurred += h);

            socket.ReceiveText("{\"serverContent\":{\"audioChunks\":[{\"data\":\"AQI=\"}]}}");
            (await error).GetException().Message.ShouldBe("handler failure");

            socket.ReceiveText("{\"serverContent\":{\"audioChunks\":[{\"data\":\"AQI=\"}]}}");
            await WaitUntilAsync(() => Volatile.Read(ref calls) == 2);
        }
    }

    [Fact]
    public async Task Disconnection_RaisesDisconnectedWithReason()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var disconnected = new TaskCompletionSource<EventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.Disconnected += (_, e) => disconnected.TrySetResult(e);

            socket.SimulateDisconnection(DisconnectionType.ByServer, "Quota exceeded");

            var e = await disconnected.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken);
            e.ShouldBeOfType<ErrorMessageEventArgs>().ErrorMessage.ShouldBe("Quota exceeded");
        }
    }

    #endregion

    #region Reconnection

    [Fact]
    public async Task Reconnection_ResendsSetup_ThenRestoresPromptsConfigAndPlay()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var ct = TestContext.Current.CancellationToken;
            await client.SetWeightedPromptsAsync("ambient", ct);
            await client.SetMusicGenerationConfigAsync(new LiveMusicGenerationConfig { Bpm = 80 }, ct);
            await client.PlayAsync(ct);
            var before = socket.SentMessages.Count;

            socket.SimulateReconnection(ReconnectionType.Lost);

            await WaitUntilAsync(() => socket.SentMessages.Count == before + 4);
            socket.SentMessages.Skip(before).ShouldBe(new[]
            {
                SetupJson,
                "{\"clientContent\":{\"weightedPrompts\":[{\"text\":\"ambient\",\"weight\":1}]}}",
                "{\"musicGenerationConfig\":{\"bpm\":80}}",
                "{\"playbackControl\":\"PLAY\"}"
            });
        }
    }

    [Fact]
    public async Task Reconnection_DoesNotResumePlay_WhenPaused()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var ct = TestContext.Current.CancellationToken;
            await client.SetWeightedPromptsAsync("ambient", ct);
            await client.PlayAsync(ct);
            await client.PauseAsync(ct);
            var before = socket.SentMessages.Count;

            socket.SimulateReconnection(ReconnectionType.Error);

            await WaitUntilAsync(() => socket.SentMessages.Count == before + 2);
            await Task.Delay(100, ct);
            socket.SentMessages.Skip(before).ShouldBe(new[]
            {
                SetupJson,
                "{\"clientContent\":{\"weightedPrompts\":[{\"text\":\"ambient\",\"weight\":1}]}}"
            });
        }
    }

    [Fact]
    public async Task Reconnection_OnlyResendsSetup_WhenRestoreDisabled()
    {
        var (client, socket) = await ConnectAsync(c => c.RestoreStateOnReconnect = false);
        using (client)
        {
            var ct = TestContext.Current.CancellationToken;
            await client.SetWeightedPromptsAsync("ambient", ct);
            await client.PlayAsync(ct);
            var before = socket.SentMessages.Count;

            socket.SimulateReconnection(ReconnectionType.Lost);
            socket.SimulateReconnection(ReconnectionType.Initial); // ignored

            await WaitUntilAsync(() => socket.SentMessages.Count == before + 1);
            await Task.Delay(100, ct);
            socket.SentMessages.Skip(before).ShouldBe(new[] { SetupJson });
        }
    }

    #endregion
}
