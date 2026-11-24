using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline tests for live interactions (python-genai 3.5 <c>interactions.connect</c> and
/// <c>interactions.auth_tokens</c>): the auth-token HTTP client, the WebSocket wire rules and the connection state
/// machine over an in-memory <see cref="WebSocket"/>. No network connections are opened.
/// </summary>
public class LiveInteractions_Tests
{
    private const string TokenJson = """{"auth_token": "auth_tokens/tok_123", "expires_at": "2026-10-23T17:00:00Z"}""";

    // ------------------------------------------------------------------------------------------------------------
    // Auth tokens (HTTP)
    // ------------------------------------------------------------------------------------------------------------

    private static (InteractionsClient Client, RecordingHttpHandler Handler) CreateClient()
    {
        var handler = new RecordingHttpHandler();
        return (new InteractionsClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task AuthTokens_Create_PostsToInteractionAuthTokens()
    {
        var (client, handler) = CreateClient();
        handler.Reply(TokenJson);

        var token = await client.AuthTokens.CreateAsync("int_001", cancellationToken: TestContext.Current.CancellationToken);

        token.AuthToken.ShouldBe("auth_tokens/tok_123");
        token.ExpiresAt.ShouldBe(new DateTimeOffset(2026, 10, 23, 17, 0, 0, TimeSpan.Zero));
        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.ToString().ShouldBe("https://generativelanguage.googleapis.com/v1beta/interactions/int_001/auth_tokens");
        request.Headers["x-goog-api-key"].ShouldBe("test-key");
        request.Body!.ToJsonString().ShouldBe("{}");
    }

    [Fact]
    public async Task AuthTokens_Create_SendsExpireTimeAndApiVersion()
    {
        var (client, handler) = CreateClient();
        handler.Reply(TokenJson);

        await client.AuthTokens.CreateAsync("interactions/int_001",
            new InteractionAuthTokenCreateParams { ExpireTime = new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero) },
            apiVersion: "v1alpha", cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Url.ToString().ShouldBe("https://generativelanguage.googleapis.com/v1alpha/interactions/int_001/auth_tokens");
        DateTimeOffset.Parse(request.Body!["expire_time"]!.GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture)
            .ShouldBe(new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero));
    }

    [Theory]
    [InlineData("tok_123")]
    [InlineData("auth_tokens/tok_123")]
    public async Task AuthTokens_Delete_StripsPrefix(string token)
    {
        var (client, handler) = CreateClient();

        await client.AuthTokens.DeleteAsync("int_001", token, cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Delete);
        request.Url.ToString().ShouldBe(
            "https://generativelanguage.googleapis.com/v1beta/interactions/int_001/auth_tokens/tok_123");
    }

    [Fact]
    public async Task AuthTokens_Create_SurfacesApiErrors()
    {
        var (client, handler) = CreateClient();
        handler.Reply("""{"error": {"code": 404, "message": "Interaction not found", "status": "NOT_FOUND"}}""",
            HttpStatusCode.NotFound);

        var ex = await Should.ThrowAsync<ApiException>(() =>
            client.AuthTokens.CreateAsync("missing", cancellationToken: TestContext.Current.CancellationToken));
        ex.ErrorCode.ShouldBe(404);
    }

    [Fact]
    public async Task AuthTokens_And_Connect_RejectVertex()
    {
        var vertex = new InteractionsClient(new VertextPlatformAdapter("project", "us-central1", accessToken: "token",
            validateAccessToken: false));

        await Should.ThrowAsync<NotSupportedException>(() => vertex.AuthTokens.CreateAsync("int_001",
            cancellationToken: TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() => vertex.AuthTokens.DeleteAsync("int_001", "tok",
            cancellationToken: TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() => vertex.ConnectAsync("int_001",
            cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Connect_RejectsEmptyInteractionId(string id)
    {
        var (client, _) = CreateClient();
        await Should.ThrowAsync<ArgumentException>(() =>
            client.ConnectAsync(id, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Connect_RejectsNonPositiveTimeout()
    {
        var (client, _) = CreateClient();
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => client.ConnectAsync("int_001",
            new InteractionConnectOptions { Timeout = TimeSpan.Zero }, TestContext.Current.CancellationToken));
    }

    // ------------------------------------------------------------------------------------------------------------
    // Wire rules
    // ------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("https://generativelanguage.googleapis.com", "v1beta",
        "wss://generativelanguage.googleapis.com/v1beta/interactions/live")]
    [InlineData("http://127.0.0.1:8080/", "v1beta", "ws://127.0.0.1:8080/v1beta/interactions/live")]
    [InlineData("https://gateway.example.com/genai/?key=proxy-key", "v1beta",
        "wss://gateway.example.com/genai/v1beta/interactions/live?key=proxy-key")]
    [InlineData("https://example.com", "", "wss://example.com/interactions/live")]
    public void BuildWebSocketUrl_FollowsPython(string baseUrl, string version, string expected)
    {
        InteractionWebSocketProtocol.BuildWebSocketUrl(baseUrl, version).ToString().ShouldBe(expected);
    }

    [Fact]
    public void BuildHandshakeHeaders_SendsApiKeyAndRevision()
    {
        var headers = InteractionWebSocketProtocol.BuildHandshakeHeaders(
            new[] { KeyValuePair.Create("x-goog-api-key", "test-api-key") }, null);

        headers["x-goog-api-key"].ShouldBe("test-api-key");
        headers["api-revision"].ShouldBe(InteractionWebSocketProtocol.DefaultApiRevision);
        headers.Count.ShouldBe(2);
    }

    [Fact]
    public void BuildHandshakeHeaders_ExtraHeadersReplaceCaseInsensitively()
    {
        var headers = InteractionWebSocketProtocol.BuildHandshakeHeaders(
            new[] { KeyValuePair.Create("Authorization", "Bearer a") },
            new Dictionary<string, string> { ["authorization"] = "Bearer b", ["X-Custom"] = "1" },
            apiRevision: null);

        headers.Count.ShouldBe(2);
        headers.Keys.ShouldContain("authorization");
        headers["Authorization"].ShouldBe("Bearer b");
        headers["x-custom"].ShouldBe("1");
    }

    [Fact]
    public void BuildHandshakeHeaders_AuthTokenStripsAuthorization()
    {
        var headers = InteractionWebSocketProtocol.BuildHandshakeHeaders(
            new[] { KeyValuePair.Create("x-goog-api-key", "auth_tokens/ephemeral") },
            new Dictionary<string, string> { ["Authorization"] = "Bearer secret-oauth-token", ["X-Goog-Api-Key"] = "other" });

        headers.ContainsKey("Authorization").ShouldBeFalse();
        headers["x-goog-api-key"].ShouldBe("auth_tokens/ephemeral");
    }

    [Fact]
    public void SerializeClientPayload_WritesSingleEventsAndLists()
    {
        ShouldBeJson(InteractionWebSocketProtocol.SerializeClientPayload(InteractionInputEvent.FromText("Hello!")),
            """{"event_type":"input","content":[{"type":"text","text":"Hello!"}]}""");

        var batch = InteractionClientEventBatch.FromEvents(new InteractionClientEvent[]
        {
            InteractionInputEvent.FromText("First"), new CompleteInteractionEvent()
        });
        var json = InteractionWebSocketProtocol.SerializeClientPayload(batch);
        json.ShouldNotContain("\n");
        ShouldBeJson(json,
            """{"events":[{"event_type":"input","content":[{"type":"text","text":"First"}]},{"event_type":"interaction.complete"}]}""");
    }

    private static void ShouldBeJson(string actual, string expected)
    {
        JsonNode.DeepEquals(JsonNode.Parse(actual), JsonNode.Parse(expected))
            .ShouldBeTrue($"Expected {expected} but was {actual}");
    }

    [Fact]
    public void SerializeClientPayload_RejectsEmptyBatch()
    {
        Should.Throw<ArgumentException>(() => InteractionWebSocketProtocol.SerializeClientPayload(
            new InteractionClientEventBatch(new InteractionClientEventList { Events = new() })));
    }

    [Fact]
    public void PayloadHasCancel_DetectsSingleAndBatchedCancel()
    {
        InteractionWebSocketProtocol.PayloadHasCancel(new CancelInteractionEvent()).ShouldBeTrue();
        InteractionWebSocketProtocol.PayloadHasCancel(InteractionClientEventBatch.FromEvents(
            new InteractionClientEvent[] { InteractionInputEvent.FromText("x"), new CancelInteractionEvent() })).ShouldBeTrue();
        InteractionWebSocketProtocol.PayloadHasCancel(new CompleteInteractionEvent()).ShouldBeFalse();
    }

    [Fact]
    public void ParseServerFrame_SplitsBatchesInOrder()
    {
        var events = InteractionWebSocketProtocol.ParseServerFrame("""
            {"events": [
              {"event_type": "step.start", "index": 0, "step": {"type": "model_output"}},
              {"event_type": "step.delta", "index": 0, "delta": {"type": "text", "text": "Hi"}},
              {"event_type": "future.event", "x": 1},
              {"event_type": "interaction.created", "interaction_id": "int_1"},
              {"event_type": "interaction.completed"}
            ]}
            """);

        events.Select(e => e.EventType).ShouldBe(new[]
        {
            "step.start", "step.delta", "future.event", "interaction.created", "interaction.completed"
        });
        events[0].ShouldBeOfType<InteractionStepStartEvent>();
        events[1].ShouldBeOfType<InteractionStepDeltaEvent>();
        events[2].ShouldBeOfType<UnknownInteractionSseEvent>();
        events[3].ShouldBeOfType<LiveInteractionCreatedEvent>().InteractionId.ShouldBe("int_1");
        events[4].ShouldBeOfType<LiveInteractionCompletedEvent>();
    }

    [Fact]
    public void ParseServerFrame_ReadsSingleEventsAndEmptyBatches()
    {
        InteractionWebSocketProtocol.ParseServerFrame("""{"event_type": "interaction.connected"}""")
            .ShouldHaveSingleItem().ShouldBeOfType<InteractionConnectedEvent>();
        InteractionWebSocketProtocol.ParseServerFrame("""{"events": []}""").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("\"not-a-dict\"")]
    [InlineData("123")]
    [InlineData("{}")]
    [InlineData("""{"event_type": ""}""")]
    [InlineData("""{"event_type": 1}""")]
    [InlineData("""{"events": {"event_type": "step.start", "index": 0}}""")]
    [InlineData("""{"events": ["not-a-dict"]}""")]
    [InlineData("""{"events": [{"event_type": "step.start", "index": 0}, {}]}""")]
    public void ParseServerFrame_RequiresEventType(string frame)
    {
        var ex = Should.Throw<InteractionWebSocketException>(() => InteractionWebSocketProtocol.ParseServerFrame(frame));
        ex.Message.ShouldBe("Live interaction server events must have an event_type.");
        ex.CloseCode.ShouldBeNull();
    }

    [Fact]
    public void ParseServerFrame_RejectsInvalidJsonAndUtf8()
    {
        Should.Throw<InteractionWebSocketException>(() => InteractionWebSocketProtocol.ParseServerFrame("not-json"))
            .Message.ShouldContain("valid JSON");
        Should.Throw<InteractionWebSocketException>(() => InteractionWebSocketProtocol.ParseServerFrame(new byte[] { 0xff }))
            .Message.ShouldContain("UTF-8 JSON text");
    }

    [Theory]
    [InlineData("interaction.completed", true)]
    [InlineData("interaction.canceled", true)]
    [InlineData("interaction.cancelled", true)]
    [InlineData("reconnection_required", true)]
    [InlineData("step.stop", false)]
    [InlineData("error", false)]
    public void IsTerminalEventType_MatchesPython(string type, bool expected)
    {
        InteractionWebSocketProtocol.IsTerminalEventType(type).ShouldBe(expected);
    }

    // ------------------------------------------------------------------------------------------------------------
    // Connection state machine (in-memory WebSocket)
    // ------------------------------------------------------------------------------------------------------------

    private static async Task<List<InteractionSseEvent>> ReadAllAsync(InteractionWebSocketConnection connection)
    {
        var events = new List<InteractionSseEvent>();
        await foreach (var item in connection.ReceiveAsync(TestContext.Current.CancellationToken))
            events.Add(item);
        return events;
    }

    [Fact]
    public async Task Connection_SendsTextEventsAndBatches()
    {
        var socket = new FakeWebSocket();
        await using var connection = new InteractionWebSocketConnection(socket, "int_live_001");
        var ct = TestContext.Current.CancellationToken;

        await connection.SendAsync("Describe this image.", ct);
        await connection.SendAsync(new InteractionInputEvent
        {
            Content = new List<InteractionContent>
            {
                new InteractionImageContent { MimeType = "image/jpeg", Data = "BASE64_IMAGE" }
            }
        }, ct);
        await connection.SendAsync(new InteractionClientEvent[]
        {
            InteractionInputEvent.FromText("First batched input."), InteractionInputEvent.FromText("Second batched input.")
        }, ct);

        socket.Sent.Count.ShouldBe(3);
        ShouldBeJson(socket.Sent[0], """{"event_type":"input","content":[{"type":"text","text":"Describe this image."}]}""");
        JsonNode.Parse(socket.Sent[1])!["content"]![0]!["mime_type"]!.GetValue<string>().ShouldBe("image/jpeg");
        JsonNode.Parse(socket.Sent[2])!["events"]!.AsArray().Count.ShouldBe(2);
        await Should.ThrowAsync<ArgumentException>(() =>
            connection.SendAsync(Array.Empty<InteractionClientEvent>(), ct));
    }

    [Fact]
    public async Task Connection_ReceivesUntilCompletedAndClosesSocket()
    {
        var socket = new FakeWebSocket();
        socket.EnqueueText("""{"event_type": "interaction.connected"}""");
        socket.EnqueueText("""{"events": [{"event_type": "step.start", "index": 0, "step": {"type": "model_output"}}, {"event_type": "step.delta", "index": 0, "delta": {"type": "text", "text": "Hello"}}]}""");
        socket.EnqueueText("""{"events": [{"event_type": "step.stop", "index": 0}, {"event_type": "interaction.completed"}, {"event_type": "step.delta", "index": 0, "delta": {"type": "text", "text": "trailing"}}]}""");
        socket.EnqueueClose(WebSocketCloseStatus.NormalClosure);
        await using var connection = new InteractionWebSocketConnection(socket, "int_1");

        var events = await ReadAllAsync(connection);

        events.Select(e => e.EventType).ShouldBe(new[]
        {
            "interaction.connected", "step.start", "step.delta", "step.stop", "interaction.completed"
        });
        ((InteractionTextDelta)((InteractionStepDeltaEvent)events[2]).Delta!).Text.ShouldBe("Hello");
        connection.IsTerminal.ShouldBeTrue();
        socket.CloseCalls.ShouldBe(1);
        var reads = socket.ReceiveCalls;
        (await ReadAllAsync(connection)).ShouldBeEmpty();
        socket.ReceiveCalls.ShouldBe(reads);
        await Should.ThrowAsync<InvalidOperationException>(() => connection.SendAsync("late", TestContext.Current.CancellationToken));
        await connection.CompleteAsync(TestContext.Current.CancellationToken); // no-op once terminal
        await connection.CancelAsync(TestContext.Current.CancellationToken);
        socket.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Connection_KeepsPendingEventsAfterBreak()
    {
        var socket = new FakeWebSocket();
        socket.EnqueueText("""{"events": [{"event_type": "step.stop", "index": 0}, {"event_type": "step.stop", "index": 1}, {"event_type": "interaction.completed"}]}""");
        await using var connection = new InteractionWebSocketConnection(socket);

        await foreach (var first in connection.ReceiveAsync(TestContext.Current.CancellationToken))
        {
            first.ShouldBeOfType<InteractionStepStopEvent>().Index.ShouldBe(0);
            break;
        }

        (await ReadAllAsync(connection)).Count.ShouldBe(2);
        socket.ReceiveCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Connection_CloseBeforeTerminalIsAnError()
    {
        var socket = new FakeWebSocket();
        socket.EnqueueClose(WebSocketCloseStatus.InternalServerError, "controlled drop");
        await using var connection = new InteractionWebSocketConnection(socket);

        await connection.CompleteAsync(TestContext.Current.CancellationToken);
        var ex = await Should.ThrowAsync<InteractionWebSocketException>(() => ReadAllAsync(connection));

        ex.CloseCode.ShouldBe(1011);
        ex.Message.ShouldNotContain("controlled drop");
        socket.Sent.ShouldBe(new[] { """{"event_type":"interaction.complete"}""" });
    }

    [Fact]
    public async Task Connection_ProtocolErrorDetaches()
    {
        var socket = new FakeWebSocket();
        socket.EnqueueText("""{"event_type": "interaction.connected"}""");
        socket.EnqueueText("not-json");
        await using var connection = new InteractionWebSocketConnection(socket);

        var received = new List<InteractionSseEvent>();
        var ex = await Should.ThrowAsync<InteractionWebSocketException>(async () =>
        {
            await foreach (var item in connection.ReceiveAsync(TestContext.Current.CancellationToken))
                received.Add(item);
        });

        received.ShouldHaveSingleItem().ShouldBeOfType<InteractionConnectedEvent>();
        ex.CloseCode.ShouldBeNull();
        connection.IsDetached.ShouldBeTrue();
        var state = await Should.ThrowAsync<InvalidOperationException>(() =>
            connection.SendAsync("after protocol failure", TestContext.Current.CancellationToken));
        state.Message.ShouldContain("detached");
    }

    [Fact]
    public async Task Connection_DetachSendsDisconnectOnce()
    {
        var socket = new FakeWebSocket();
        var connection = new InteractionWebSocketConnection(socket, "int_1");

        await connection.DetachAsync(TestContext.Current.CancellationToken);
        await connection.DetachAsync(TestContext.Current.CancellationToken);
        await connection.DisposeAsync();

        socket.Sent.ShouldBe(new[] { """{"event_type":"interaction.disconnect"}""" });
        socket.CloseCalls.ShouldBe(1);
        connection.IsTerminal.ShouldBeFalse();
        await Should.ThrowAsync<InvalidOperationException>(() => connection.CompleteAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Connection_DetachEndsPendingReceive()
    {
        var socket = new FakeWebSocket();
        await using var connection = new InteractionWebSocketConnection(socket);

        var reading = ReadAllAsync(connection);
        await socket.WaitForReceiveAsync();
        await connection.DetachAsync(TestContext.Current.CancellationToken);

        (await reading).ShouldBeEmpty();
        socket.Sent.ShouldBe(new[] { """{"event_type":"interaction.disconnect"}""" });
    }

    [Fact]
    public async Task Connection_CancelIsTerminalAndEndsPendingReceive()
    {
        var socket = new FakeWebSocket();
        await using var connection = new InteractionWebSocketConnection(socket);

        var reading = ReadAllAsync(connection);
        await socket.WaitForReceiveAsync();
        await connection.CancelAsync(TestContext.Current.CancellationToken);
        await connection.CancelAsync(TestContext.Current.CancellationToken);
        await connection.DetachAsync(TestContext.Current.CancellationToken);

        (await reading).ShouldBeEmpty();
        connection.IsTerminal.ShouldBeTrue();
        socket.Sent.ShouldBe(new[] { """{"event_type":"interaction.cancel"}""" });
    }

    [Fact]
    public async Task Connection_SendingCancelEventMakesItTerminal()
    {
        var socket = new FakeWebSocket();
        await using var connection = new InteractionWebSocketConnection(socket);

        await connection.SendAsync(new InteractionClientEvent[] { InteractionInputEvent.FromText("x"), new CancelInteractionEvent() },
            TestContext.Current.CancellationToken);

        connection.IsTerminal.ShouldBeTrue();
        socket.CloseCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Connection_SendOnClosedSocketThrowsWithCloseCode()
    {
        var socket = new FakeWebSocket { FailSends = true };
        await using var connection = new InteractionWebSocketConnection(socket);

        var ex = await Should.ThrowAsync<InteractionWebSocketException>(() =>
            connection.CompleteAsync(TestContext.Current.CancellationToken));
        ex.CloseCode.ShouldBe(1006);

        await connection.DetachAsync(TestContext.Current.CancellationToken); // no disconnect after a connection error
        socket.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Connection_TakesIdFromCreatedEventButNotConnected()
    {
        var socket = new FakeWebSocket();
        socket.EnqueueText("""{"event_type": "interaction.created", "interaction_id": "int_new"}""");
        socket.EnqueueText("""{"event_type": "interaction.completed"}""");
        await using var created = new InteractionWebSocketConnection(socket);
        await ReadAllAsync(created);
        created.Id.ShouldBe("int_new");

        var socket2 = new FakeWebSocket();
        socket2.EnqueueText("""{"event_type": "interaction.connected", "interaction_id": "other"}""");
        socket2.EnqueueText("""{"event_type": "interaction.completed"}""");
        await using var connected = new InteractionWebSocketConnection(socket2, "int_1");
        await ReadAllAsync(connected);
        connected.Id.ShouldBe("int_1");
    }

    [Fact]
    public async Task Connection_ErrorEventsAreYielded()
    {
        var socket = new FakeWebSocket();
        socket.EnqueueText("""{"event_type": "error", "error": {"code": "invalid_request", "message": "bad"}}""");
        socket.EnqueueText("""{"event_type": "interaction.completed"}""");
        await using var connection = new InteractionWebSocketConnection(socket);

        var events = await ReadAllAsync(connection);
        events.Select(e => e.EventType).ShouldBe(new[] { "error", "interaction.completed" });
    }

    /// <summary>An in-memory <see cref="WebSocket"/> fed with queued server frames.</summary>
    private sealed class FakeWebSocket : WebSocket
    {
        private readonly Queue<(byte[]? Data, WebSocketCloseStatus? Close, string? Reason)> _incoming = new();
        private readonly SemaphoreSlim _available = new(0);
        private readonly TaskCompletionSource<bool> _receiving = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private WebSocketState _state = WebSocketState.Open;
        private WebSocketCloseStatus? _closeStatus;

        public List<string> Sent { get; } = new();
        public int CloseCalls { get; private set; }
        public int ReceiveCalls { get; private set; }
        public bool FailSends { get; set; }

        public void EnqueueText(string json)
        {
            _incoming.Enqueue((Encoding.UTF8.GetBytes(json), null, null));
            _available.Release();
        }

        public void EnqueueClose(WebSocketCloseStatus status, string reason = "")
        {
            _incoming.Enqueue((null, status, reason));
            _available.Release();
        }

        public Task WaitForReceiveAsync() => _receiving.Task;

        public override WebSocketCloseStatus? CloseStatus => _closeStatus;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => _state;
        public override string? SubProtocol => null;

        public override void Abort() => _state = WebSocketState.Aborted;

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription,
            CancellationToken cancellationToken)
        {
            CloseCalls++;
            _closeStatus ??= closeStatus;
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription,
            CancellationToken cancellationToken) => CloseAsync(closeStatus, statusDescription, cancellationToken);

        public override void Dispose() => _available.Dispose();

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer,
            CancellationToken cancellationToken)
        {
            ReceiveCalls++;
            _receiving.TrySetResult(true);
            try
            {
                await _available.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _state = WebSocketState.Aborted;
                throw;
            }

            var (data, close, reason) = _incoming.Dequeue();
            if (close != null)
            {
                _closeStatus = close;
                _state = WebSocketState.CloseReceived;
                return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, close, reason);
            }

            data!.CopyTo(buffer.Array!, buffer.Offset);
            return new WebSocketReceiveResult(data!.Length, WebSocketMessageType.Text, true);
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
            CancellationToken cancellationToken)
        {
            if (FailSends || _state != WebSocketState.Open)
                throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely);
            Sent.Add(Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));
            return Task.CompletedTask;
        }
    }
}
