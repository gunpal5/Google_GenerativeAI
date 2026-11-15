using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Live;
using GenerativeAI.Types;
using Moq;
using Shouldly;

namespace GenerativeAI.Tests.Live;

/// <summary>
/// Unit tests for <see cref="MultiModalLiveClient"/> using an injected in-memory WebSocket.
/// </summary>
public class MultiModalLiveClient_Tests
{
    private const string ModelName = "gemini-2.0-flash-live-001";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private static async Task<(MultiModalLiveClient Client, FakeWebsocketClient Socket)> ConnectAsync(
        Action<MultiModalLiveClient>? configure = null, ICollection<SafetySetting>? safetySettings = null,
        string? systemInstruction = null, bool inputAudioTranscriptionEnabled = false, bool autoSendSetup = false)
    {
        var client = new MultiModalLiveClient(new GoogleAIPlatformAdapter("test-key"), ModelName,
            safetySettings: safetySettings, systemInstruction: systemInstruction,
            inputAudioTranscriptionEnabled: inputAudioTranscriptionEnabled);
        FakeWebsocketClient? socket = null;
        client.WebsocketClientFactory = url => socket = new FakeWebsocketClient(url);
        configure?.Invoke(client);

        await client.ConnectAsync(autoSendSetup, TestContext.Current.CancellationToken);

        socket.ShouldNotBeNull();
        return (client, socket);
    }

    private static JsonElement LastSent(FakeWebsocketClient socket)
    {
        socket.SentMessages.ShouldNotBeEmpty();
        using var doc = JsonDocument.Parse(socket.SentMessages[^1]);
        return doc.RootElement.Clone();
    }

    private static Task<T> WaitForEventAsync<T>(Action<EventHandler<T>> subscribe)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        subscribe((_, e) => tcs.TrySetResult(e));
        return tcs.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken);
    }

    #region Connection

    [Fact]
    public async Task ConnectAsync_UsesFactoryWithPlatformUrl_AndStartsClient()
    {
        string? receivedUrl = null;
        var client = new MultiModalLiveClient(new GoogleAIPlatformAdapter("test-key"), ModelName);
        FakeWebsocketClient? socket = null;
        client.WebsocketClientFactory = url =>
        {
            receivedUrl = url;
            return socket = new FakeWebsocketClient(url);
        };
        var connected = false;
        client.Connected += (_, _) => connected = true;

        await client.ConnectAsync(autoSendSetup: false, TestContext.Current.CancellationToken);

        receivedUrl.ShouldNotBeNull();
        receivedUrl.ShouldStartWith("wss://");
        receivedUrl.ShouldContain("key=test-key");
        socket.ShouldNotBeNull();
        socket.IsRunning.ShouldBeTrue();
        client.Client.ShouldBeSameAs(socket);
        connected.ShouldBeTrue();
        socket.SentMessages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ConnectAsync_WithAutoSendSetup_SendsSetupMessage()
    {
        var (client, socket) = await ConnectAsync(autoSendSetup: true);
        using (client)
        {
            var root = LastSent(socket);
            root.GetProperty("setup").GetProperty("model").GetString().ShouldBe("models/" + ModelName);
        }
    }

    [Fact]
    public async Task ConnectAsync_FactoryReturnsNull_Throws()
    {
        using var client = new MultiModalLiveClient(new GoogleAIPlatformAdapter("test-key"), ModelName);
        client.WebsocketClientFactory = _ => null!;

        await Should.ThrowAsync<InvalidOperationException>(() =>
            client.ConnectAsync(false, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DisconnectAsync_StopsAndDisposesInjectedClient()
    {
        var (client, socket) = await ConnectAsync();

        await client.DisconnectAsync(TestContext.Current.CancellationToken);

        socket.IsRunning.ShouldBeFalse();
        socket.IsDisposed.ShouldBeTrue();
        client.Client.ShouldBeNull();
        client.Dispose();
    }

    [Fact]
    public async Task Send_WhenNotConnected_ThrowsInvalidOperationException()
    {
        using var client = new MultiModalLiveClient(new GoogleAIPlatformAdapter("test-key"), ModelName);
        ErrorEventArgs? error = null;
        client.ErrorOccurred += (_, e) => error = e;

        await Should.ThrowAsync<InvalidOperationException>(() =>
            client.SendRealtimeTextAsync("hi", TestContext.Current.CancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() =>
            client.SendSetupAsync(TestContext.Current.CancellationToken));
        await Should.ThrowAsync<InvalidOperationException>(() =>
            client.SendActivityStartAsync(TestContext.Current.CancellationToken));

        error.ShouldNotBeNull();
        error.GetException().ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Send_AfterDisconnect_ThrowsInvalidOperationException()
    {
        var (client, _) = await ConnectAsync();
        using (client)
        {
            await client.DisconnectAsync(TestContext.Current.CancellationToken);

            await Should.ThrowAsync<InvalidOperationException>(() =>
                client.SendAudioStreamEndAsync(TestContext.Current.CancellationToken));
        }
    }

    #endregion

    #region Realtime input

    [Fact]
    public async Task SendActivityStartAsync_SendsEmptyActivityStart()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendActivityStartAsync(TestContext.Current.CancellationToken);

            var root = LastSent(socket);
            root.EnumerateObject().Select(p => p.Name).ShouldBe(new[] { "realtimeInput" });
            var realtimeInput = root.GetProperty("realtimeInput");
            realtimeInput.EnumerateObject().Select(p => p.Name).ShouldBe(new[] { "activityStart" });
            var activityStart = realtimeInput.GetProperty("activityStart");
            activityStart.ValueKind.ShouldBe(JsonValueKind.Object);
            activityStart.EnumerateObject().ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task SendActivityEndAsync_SendsEmptyActivityEnd()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendActivityEndAsync(TestContext.Current.CancellationToken);

            var realtimeInput = LastSent(socket).GetProperty("realtimeInput");
            realtimeInput.EnumerateObject().Select(p => p.Name).ShouldBe(new[] { "activityEnd" });
            var activityEnd = realtimeInput.GetProperty("activityEnd");
            activityEnd.ValueKind.ShouldBe(JsonValueKind.Object);
            activityEnd.EnumerateObject().ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task SendAudioStreamEndAsync_SendsAudioStreamEndTrue()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendAudioStreamEndAsync(TestContext.Current.CancellationToken);

            var realtimeInput = LastSent(socket).GetProperty("realtimeInput");
            realtimeInput.EnumerateObject().Select(p => p.Name).ShouldBe(new[] { "audioStreamEnd" });
            realtimeInput.GetProperty("audioStreamEnd").GetBoolean().ShouldBeTrue();
        }
    }

    [Fact]
    public async Task SendVideoFrameAsync_SendsVideoBlob()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            byte[] frame = [1, 2, 3, 4];

            await client.SendVideoFrameAsync(frame, "image/png", TestContext.Current.CancellationToken);

            var video = LastSent(socket).GetProperty("realtimeInput").GetProperty("video");
            video.GetProperty("mimeType").GetString().ShouldBe("image/png");
            video.GetProperty("data").GetString().ShouldBe(Convert.ToBase64String(frame));
        }
    }

    [Fact]
    public async Task SendVideoFrameAsync_DefaultsToJpeg()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendVideoFrameAsync([9], cancellationToken: TestContext.Current.CancellationToken);

            LastSent(socket).GetProperty("realtimeInput").GetProperty("video").GetProperty("mimeType").GetString()
                .ShouldBe("image/jpeg");
        }
    }

    [Fact]
    public async Task SendRealtimeTextAsync_SendsRealtimeText()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendRealtimeTextAsync("Hello there", TestContext.Current.CancellationToken);

            var realtimeInput = LastSent(socket).GetProperty("realtimeInput");
            realtimeInput.EnumerateObject().Select(p => p.Name).ShouldBe(new[] { "text" });
            realtimeInput.GetProperty("text").GetString().ShouldBe("Hello there");
        }
    }

    [Fact]
    public async Task SendAudioAsync_SendsAudioBlobWithDefaultMimeType()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            byte[] audio = [10, 20, 30];

            await client.SendAudioAsync(audio, cancellationToken: TestContext.Current.CancellationToken);

            var blob = LastSent(socket).GetProperty("realtimeInput").GetProperty("audio");
            blob.GetProperty("mimeType").GetString().ShouldBe("audio/pcm;rate=16000");
            blob.GetProperty("data").GetString().ShouldBe(Convert.ToBase64String(audio));
        }
    }

    [Fact]
    public async Task SendRealtimeInputAsync_SendsGivenInput()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendRealtimeInputAsync(new BidiGenerateContentRealtimeInput
            {
                Audio = new Blob { Data = "AAAA", MimeType = "audio/pcm;rate=24000" },
                Text = "caption"
            }, TestContext.Current.CancellationToken);

            var realtimeInput = LastSent(socket).GetProperty("realtimeInput");
            realtimeInput.GetProperty("audio").GetProperty("data").GetString().ShouldBe("AAAA");
            realtimeInput.GetProperty("audio").GetProperty("mimeType").GetString().ShouldBe("audio/pcm;rate=24000");
            realtimeInput.GetProperty("text").GetString().ShouldBe("caption");
            realtimeInput.TryGetProperty("video", out _).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task SendRealtimeInputAsync_Null_ThrowsArgumentNullException()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await Should.ThrowAsync<ArgumentNullException>(() =>
                client.SendRealtimeInputAsync(null!, TestContext.Current.CancellationToken));
            socket.SentMessages.ShouldBeEmpty();
        }
    }

    #endregion

    #region Client content / tool response

    [Fact]
    public async Task SentTextAsync_SendsCompleteUserTurn()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SentTextAsync("What is up?", TestContext.Current.CancellationToken);

            var clientContent = LastSent(socket).GetProperty("clientContent");
            clientContent.GetProperty("turnComplete").GetBoolean().ShouldBeTrue();
            var turn = clientContent.GetProperty("turns")[0];
            turn.GetProperty("role").GetString().ShouldBe("user");
            turn.GetProperty("parts")[0].GetProperty("text").GetString().ShouldBe("What is up?");
        }
    }

    [Fact]
    public async Task SendClientContentAsync_SendsClientContent()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendClientContentAsync(new BidiGenerateContentClientContent
            {
                Turns = [new Content("earlier", Roles.Model)],
                TurnComplete = false
            }, TestContext.Current.CancellationToken);

            var clientContent = LastSent(socket).GetProperty("clientContent");
            clientContent.GetProperty("turnComplete").GetBoolean().ShouldBeFalse();
            clientContent.GetProperty("turns")[0].GetProperty("role").GetString().ShouldBe("model");
        }
    }

    [Fact]
    public async Task SendToolResponseAsync_SendsFunctionResponses()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendToolResponseAsync(new BidiGenerateContentToolResponse
            {
                FunctionResponses = [new FunctionResponse { Id = "call-1", Name = "get_weather" }]
            }, TestContext.Current.CancellationToken);

            var response = LastSent(socket).GetProperty("toolResponse").GetProperty("functionResponses")[0];
            response.GetProperty("id").GetString().ShouldBe("call-1");
            response.GetProperty("name").GetString().ShouldBe("get_weather");
        }
    }

    #endregion

    #region Context update

    [Fact]
    public async Task SendContextUpdateAsync_WithInstructionAndTools_SendsBoth()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendContextUpdateAsync("Be brief.", [new Tool { GoogleSearch = new GoogleSearchTool() }],
                TestContext.Current.CancellationToken);

            var contextUpdate = LastSent(socket).GetProperty("contextUpdate");
            var instruction = contextUpdate.GetProperty("systemInstruction");
            instruction.GetProperty("parts")[0].GetProperty("text").GetString().ShouldBe("Be brief.");
            var tools = contextUpdate.GetProperty("tools").GetProperty("tools");
            tools.GetArrayLength().ShouldBe(1);
            tools[0].TryGetProperty("googleSearch", out _).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task SendContextUpdateAsync_EmptyTools_SendsEmptyListToClearTools()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendContextUpdateAsync(null, Array.Empty<Tool>(), TestContext.Current.CancellationToken);

            var contextUpdate = LastSent(socket).GetProperty("contextUpdate");
            contextUpdate.TryGetProperty("systemInstruction", out _).ShouldBeFalse();
            var tools = contextUpdate.GetProperty("tools").GetProperty("tools");
            tools.ValueKind.ShouldBe(JsonValueKind.Array);
            tools.GetArrayLength().ShouldBe(0);
        }
    }

    [Fact]
    public async Task SendContextUpdateAsync_NullTools_OmitsToolsKey()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendContextUpdateAsync("New instruction", cancellationToken: TestContext.Current.CancellationToken);

            var contextUpdate = LastSent(socket).GetProperty("contextUpdate");
            contextUpdate.TryGetProperty("tools", out _).ShouldBeFalse();
            contextUpdate.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString()
                .ShouldBe("New instruction");
        }
    }

    [Fact]
    public async Task SendContextUpdateAsync_Object_SendsGivenUpdate()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendContextUpdateAsync(new BidiGenerateContentContextUpdate
            {
                Tools = new ContextUpdateTools { Tools = [new Tool { CodeExecution = new CodeExecutionTool() }] }
            }, TestContext.Current.CancellationToken);

            var contextUpdate = LastSent(socket).GetProperty("contextUpdate");
            contextUpdate.TryGetProperty("systemInstruction", out _).ShouldBeFalse();
            contextUpdate.GetProperty("tools").GetProperty("tools")[0].TryGetProperty("codeExecution", out _)
                .ShouldBeTrue();
        }
    }

    [Fact]
    public async Task SendContextUpdateAsync_Null_ThrowsArgumentNullException()
    {
        var (client, _) = await ConnectAsync();
        using (client)
        {
            await Should.ThrowAsync<ArgumentNullException>(() =>
                client.SendContextUpdateAsync((BidiGenerateContentContextUpdate)null!,
                    TestContext.Current.CancellationToken));
        }
    }

    #endregion

    #region Setup

    [Fact]
    public async Task SendSetupAsync_IncludesAllConfiguredSettings()
    {
        var safety = new List<SafetySetting>
        {
            new() { Category = HarmCategory.HARM_CATEGORY_HARASSMENT, Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH }
        };
        var (client, socket) = await ConnectAsync(c =>
        {
            c.RealtimeInputConfig = new RealtimeInputConfig
            {
                AutomaticActivityDetection = new AutomaticActivityDetection { Disabled = true, SilenceDurationMs = 500 }
            };
            c.SessionResumption = new SessionResumptionConfig { Handle = "prev-handle" };
            c.ContextWindowCompression = new ContextWindowCompressionConfig
            {
                TriggerTokens = 1000,
                SlidingWindow = new SlidingWindow { TargetTokens = 500 }
            };
            c.Proactivity = new ProactivityConfig { ProactiveAudio = true };
            c.HistoryConfig = new HistoryConfig { InitialHistoryInClientContent = true };
            c.UseGoogleSearch = true;
        }, safetySettings: safety, systemInstruction: "You are helpful.");
        using (client)
        {
            await client.SendSetupAsync(TestContext.Current.CancellationToken);

            var setup = LastSent(socket).GetProperty("setup");
            setup.GetProperty("model").GetString().ShouldBe("models/" + ModelName);
            setup.GetProperty("generationConfig").GetProperty("responseModalities")[0].GetString().ShouldBe("TEXT");
            setup.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString()
                .ShouldBe("You are helpful.");
            setup.GetProperty("tools")[0].TryGetProperty("googleSearch", out _).ShouldBeTrue();

            var safetySetting = setup.GetProperty("safetySettings")[0];
            safetySetting.GetProperty("category").GetString().ShouldBe("HARM_CATEGORY_HARASSMENT");
            safetySetting.GetProperty("threshold").GetString().ShouldBe("BLOCK_ONLY_HIGH");

            var detection = setup.GetProperty("realtimeInputConfig").GetProperty("automaticActivityDetection");
            detection.GetProperty("disabled").GetBoolean().ShouldBeTrue();
            detection.GetProperty("silenceDurationMs").GetInt32().ShouldBe(500);

            setup.GetProperty("sessionResumption").GetProperty("handle").GetString().ShouldBe("prev-handle");

            var compression = setup.GetProperty("contextWindowCompression");
            compression.GetProperty("triggerTokens").GetInt32().ShouldBe(1000);
            compression.GetProperty("slidingWindow").GetProperty("targetTokens").GetInt32().ShouldBe(500);

            setup.GetProperty("proactivity").GetProperty("proactiveAudio").GetBoolean().ShouldBeTrue();
            setup.GetProperty("historyConfig").GetProperty("initialHistoryInClientContent").GetBoolean()
                .ShouldBeTrue();

            setup.TryGetProperty("inputAudioTranscription", out _).ShouldBeFalse();
            setup.TryGetProperty("outputAudioTranscription", out _).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task SendSetupAsync_WithDefaults_OmitsOptionalSettings()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            await client.SendSetupAsync(TestContext.Current.CancellationToken);

            var setup = LastSent(socket).GetProperty("setup");
            foreach (var key in new[]
                     {
                         "systemInstruction", "tools", "safetySettings", "realtimeInputConfig", "sessionResumption",
                         "contextWindowCompression", "proactivity", "historyConfig", "inputAudioTranscription",
                         "outputAudioTranscription"
                     })
            {
                setup.TryGetProperty(key, out _).ShouldBeFalse(key);
            }
        }
    }

    [Fact]
    public async Task SendSetupAsync_TranscriptionEnabledFlags_SendEmptyConfigs()
    {
        var (client, socket) = await ConnectAsync(c => c.OutputAudioTranscriptionEnabled = true,
            inputAudioTranscriptionEnabled: true);
        using (client)
        {
            await client.SendSetupAsync(TestContext.Current.CancellationToken);

            var setup = LastSent(socket).GetProperty("setup");
            setup.GetProperty("inputAudioTranscription").EnumerateObject().ShouldBeEmpty();
            setup.GetProperty("outputAudioTranscription").EnumerateObject().ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task SendSetupAsync_TranscriptionConfig_OverridesEnabledFlag()
    {
        var (client, socket) = await ConnectAsync(c =>
        {
            c.InputAudioTranscriptionEnabled = false;
            c.InputAudioTranscriptionConfig = new AudioTranscriptionConfig { LanguageCodes = ["en-US"] };
            c.OutputAudioTranscriptionConfig = new AudioTranscriptionConfig { Diarization = true };
        });
        using (client)
        {
            await client.SendSetupAsync(TestContext.Current.CancellationToken);

            var setup = LastSent(socket).GetProperty("setup");
            setup.GetProperty("inputAudioTranscription").GetProperty("languageCodes")[0].GetString()
                .ShouldBe("en-US");
            setup.GetProperty("outputAudioTranscription").GetProperty("diarization").GetBoolean().ShouldBeTrue();
        }
    }

    [Fact]
    public async Task SendSetupAsync_FunctionTools_AreSentAsTools()
    {
        var tool = new Mock<IFunctionTool>();
        tool.Setup(t => t.AsTool()).Returns(new Tool
        {
            FunctionDeclarations = [new FunctionDeclaration { Name = "get_weather", Description = "Weather" }]
        });
        var (client, socket) = await ConnectAsync(c => c.FunctionTools = [tool.Object]);
        using (client)
        {
            await client.SendSetupAsync(TestContext.Current.CancellationToken);

            var tools = LastSent(socket).GetProperty("setup").GetProperty("tools");
            tools.GetArrayLength().ShouldBe(1);
            tools[0].GetProperty("functionDeclarations")[0].GetProperty("name").GetString().ShouldBe("get_weather");
        }
    }

    #endregion

    #region Server messages

    [Fact]
    public async Task ResumableSessionResumptionUpdate_SetsHandleAndRaisesEvent()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var received = WaitForEventAsync<LiveServerSessionResumptionUpdate>(h =>
                client.SessionResumableUpdateReceived += h);

            socket.ReceiveText("""{"sessionResumptionUpdate":{"newHandle":"h1","resumable":true}}""");

            var update = await received;
            update.NewHandle.ShouldBe("h1");
            update.Resumable.ShouldBe(true);
            client.LastSessionResumptionHandle.ShouldBe("h1");
        }
    }

    [Fact]
    public async Task NonResumableSessionResumptionUpdate_DoesNotOverwriteHandle()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var first = WaitForEventAsync<LiveServerSessionResumptionUpdate>(h =>
                client.SessionResumableUpdateReceived += h);
            socket.ReceiveText("""{"sessionResumptionUpdate":{"newHandle":"h1","resumable":true}}""");
            await first;

            var updates = new List<LiveServerSessionResumptionUpdate>();
            var second = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.SessionResumableUpdateReceived += (_, e) =>
            {
                lock (updates)
                {
                    updates.Add(e);
                    if (updates.Count == 3) second.TrySetResult(true);
                }
            };

            socket.ReceiveText("""{"sessionResumptionUpdate":{"newHandle":"h2","resumable":false}}""");
            socket.ReceiveText("""{"sessionResumptionUpdate":{"resumable":true}}""");
            socket.ReceiveText("""{"sessionResumptionUpdate":{}}""");
            await second.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken);

            client.LastSessionResumptionHandle.ShouldBe("h1");
        }
    }

    [Fact]
    public async Task GoAway_RaisesGoAwayReceivedWithTimeLeft()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var received = WaitForEventAsync<LiveServerGoAway>(h => client.GoAwayReceived += h);

            socket.ReceiveText("""{"goAway":{"timeLeft":"10s"}}""");

            var goAway = await received;
            goAway.TimeLeft.ShouldBe("10s");
            goAway.TimeLeftDuration.ShouldBe(TimeSpan.FromSeconds(10));
        }
    }

    [Fact]
    public async Task ServerContent_Text_RaisesTextChunkAndMessageReceived()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var chunks = new List<TextChunkReceivedArgs>();
            client.TextChunkReceived += (_, e) => { lock (chunks) chunks.Add(e); };
            var message = WaitForEventAsync<MessageReceivedEventArgs>(h =>
                client.MessageReceived += h);

            socket.ReceiveText(
                """{"serverContent":{"modelTurn":{"role":"model","parts":[{"text":"Hello"},{"text":" world"}]},"turnComplete":true}}""");

            var args = await message;
            args.Payload.ServerContent.ShouldNotBeNull();
            lock (chunks)
            {
                chunks.Select(c => c.Text).ShouldBe(new[] { "Hello", " world" });
                chunks.ShouldAllBe(c => c.IsTurnFinish);
            }
        }
    }

    [Fact]
    public async Task ServerContent_Transcriptions_RaiseTranscriptionEvents()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var input = WaitForEventAsync<Transcription>(h => client.InputTranscriptionReceived += h);
            var output = WaitForEventAsync<Transcription>(h => client.OutputTranscriptionReceived += h);

            socket.ReceiveText(
                """{"serverContent":{"inputTranscription":{"text":"hi there"},"outputTranscription":{"text":"hello","finished":true}}}""");

            (await input).Text.ShouldBe("hi there");
            var outputTranscription = await output;
            outputTranscription.Text.ShouldBe("hello");
            outputTranscription.Finished.ShouldBe(true);
        }
    }

    [Fact]
    public async Task ServerContent_Audio_RaisesChunkAndCompletedEvents()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            byte[] pcm = [1, 0, 2, 0, 3, 0];
            var chunk = WaitForEventAsync<AudioBufferReceivedEventArgs>(h =>
                client.AudioChunkReceived += h);
            var completed = WaitForEventAsync<AudioBufferReceivedEventArgs>(h =>
                client.AudioReceiveCompleted += h);

            socket.ReceiveText(
                """{"serverContent":{"modelTurn":{"parts":[{"inlineData":{"mimeType":"audio/pcm;rate=24000","data":""" +
                "\"" + Convert.ToBase64String(pcm) + "\"}}]}}}");
            socket.ReceiveText("""{"serverContent":{"turnComplete":true}}""");

            var chunkArgs = await chunk;
            chunkArgs.Buffer.ShouldBe(pcm);
            chunkArgs.HeaderInfo.SampleRate.ShouldBe(24000);
            (await completed).Buffer.ShouldBe(pcm);
        }
    }

    [Fact]
    public async Task ServerContent_Interrupted_RaisesGenerationInterruptedOnce()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var interruptions = 0;
            client.GenerationInterrupted += (_, _) => Interlocked.Increment(ref interruptions);
            var sentinel = WaitForEventAsync<LiveServerGoAway>(h => client.GoAwayReceived += h);

            socket.ReceiveText("""{"serverContent":{"interrupted":true}}""");
            // Messages are processed in order, so once the sentinel arrives the interruption has been handled.
            socket.ReceiveText("""{"goAway":{"timeLeft":"1s"}}""");
            await sentinel;

            Volatile.Read(ref interruptions).ShouldBe(1);
        }
    }

    [Fact]
    public async Task ToolCall_InvokesFunctionToolAndSendsToolResponse()
    {
        var tool = new Mock<IFunctionTool>();
        tool.Setup(t => t.IsContainFunction("get_weather")).Returns(true);
        tool.Setup(t => t.CallAsync(It.IsAny<FunctionCall>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FunctionCall call, CancellationToken _) => new FunctionResponse
            {
                Id = call.Id,
                Name = call.Name,
            });
        var (client, socket) = await ConnectAsync(c => c.FunctionTools = [tool.Object]);
        using (client)
        {
            var sent = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            socket.TextSent += json =>
            {
                if (json.Contains("toolResponse", StringComparison.Ordinal)) sent.TrySetResult(json);
            };

            socket.ReceiveText(
                """{"toolCall":{"functionCalls":[{"id":"call-7","name":"get_weather","args":{"city":"Paris"}}]}}""");

            using var doc = JsonDocument.Parse(await sent.Task.WaitAsync(Timeout, TestContext.Current.CancellationToken));
            var response = doc.RootElement.GetProperty("toolResponse").GetProperty("functionResponses")[0];
            response.GetProperty("id").GetString().ShouldBe("call-7");
            response.GetProperty("name").GetString().ShouldBe("get_weather");
            tool.Verify(t => t.CallAsync(It.Is<FunctionCall>(c => c.Name == "get_weather"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task InvalidJson_IsIgnoredAndLaterMessagesStillProcessed()
    {
        var (client, socket) = await ConnectAsync();
        using (client)
        {
            var received = WaitForEventAsync<LiveServerGoAway>(h => client.GoAwayReceived += h);

            socket.ReceiveText("{not json");
            socket.ReceiveText("""{"goAway":{"timeLeft":"5s"}}""");

            (await received).TimeLeftDuration.ShouldBe(TimeSpan.FromSeconds(5));
        }
    }

    #endregion
}
