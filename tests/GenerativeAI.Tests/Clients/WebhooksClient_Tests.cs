using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline unit tests for <see cref="WebhooksClient"/> and the per-request <see cref="WebhookConfig"/> (wire format
/// follows python-genai's generated webhooks client and batch converters).
/// </summary>
public class WebhooksClient_Tests
{
    private const string GoogleBase = "https://generativelanguage.googleapis.com/v1beta/webhooks";

    private sealed record CapturedRequest(HttpMethod Method, Uri Url, string? Body, HttpRequestMessage Message);

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Queue<string> _responses = new();

        public List<CapturedRequest> Requests { get; } = new();

        public CapturingHandler Enqueue(string body)
        {
            _responses.Enqueue(body);
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri!, body, request));
            var responseBody = _responses.Count > 0 ? _responses.Dequeue() : "{}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private static (WebhooksClient Client, CapturingHandler Handler) CreateGoogle(params string[] responses)
    {
        var handler = new CapturingHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new WebhooksClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler)), handler);
    }

    private const string WebhookJson = """
        {
          "id": "wh_123",
          "name": "my hook",
          "uri": "https://example.com/hook",
          "subscribed_events": ["batch.succeeded", "video.generated", "future.event"],
          "state": "disabled_due_to_failed_deliveries",
          "new_signing_secret": "whsec_new",
          "signing_secrets": [{"truncated_secret": "whsec_...abc", "expire_time": "2026-12-01T00:00:00Z"}],
          "create_time": "2026-11-01T10:00:00Z",
          "update_time": "2026-11-02T10:00:00Z"
        }
        """;

    [Fact]
    public void SerializerContext_ProvidesTypeInfoForWebhookTypes()
    {
        WebhooksSerializerContext.Default.GetTypeInfo(typeof(Webhook)).ShouldNotBeNull();
        WebhooksSerializerContext.Default.GetTypeInfo(typeof(WebhookListResponse)).ShouldNotBeNull();
        WebhooksSerializerContext.Default.GetTypeInfo(typeof(WebhookUpdate)).ShouldNotBeNull();
        WebhooksSerializerContext.Default.GetTypeInfo(typeof(RotateSigningSecretRequest)).ShouldNotBeNull();
        TypesSerializerContext.Default.GetTypeInfo(typeof(WebhookConfig)).ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_PostsSnakeCaseBody_AndParsesWebhook()
    {
        var (client, handler) = CreateGoogle(WebhookJson);

        var result = await client.CreateAsync("https://example.com/hook",
            new[] { WebhookEvents.BatchSucceeded, WebhookEvents.VideoGenerated }, "my hook",
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase);
        request.Message.Headers.GetValues("x-goog-api-key").ShouldContain("test-key");
        var body = JsonNode.Parse(request.Body!)!.AsObject();
        body["uri"]!.GetValue<string>().ShouldBe("https://example.com/hook");
        body["name"]!.GetValue<string>().ShouldBe("my hook");
        body["subscribed_events"]!.AsArray().Select(n => n!.GetValue<string>())
            .ShouldBe(new[] { "batch.succeeded", "video.generated" });
        body.Count.ShouldBe(3);

        result.Id.ShouldBe("wh_123");
        result.Name.ShouldBe("my hook");
        result.Uri.ShouldBe("https://example.com/hook");
        result.SubscribedEvents.ShouldBe(new[] { "batch.succeeded", "video.generated", "future.event" });
        result.State.ShouldBe(WebhookStates.DisabledDueToFailedDeliveries);
        result.NewSigningSecret.ShouldBe("whsec_new");
        var secret = result.SigningSecrets.ShouldHaveSingleItem();
        secret.TruncatedSecret.ShouldBe("whsec_...abc");
        secret.ExpireTime.ShouldBe("2026-12-01T00:00:00Z");
        result.CreateTime.ShouldBe("2026-11-01T10:00:00Z");
        result.UpdateTime.ShouldBe("2026-11-02T10:00:00Z");
    }

    [Fact]
    public async Task CreateAsync_OmitsNameWhenNotSet()
    {
        var (client, handler) = CreateGoogle(WebhookJson);

        await client.CreateAsync(new CreateWebhookRequest
        {
            Uri = "https://example.com/hook",
            SubscribedEvents = new List<string> { WebhookEvents.InteractionCompleted }
        }, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Requests.Single().Body!)!.AsObject();
        body.ContainsKey("name").ShouldBeFalse();
        body["subscribed_events"]![0]!.GetValue<string>().ShouldBe("interaction.completed");
    }

    [Fact]
    public async Task CreateAsync_ValidatesRequiredFields()
    {
        var (client, handler) = CreateGoogle();

        await Should.ThrowAsync<ArgumentException>(() => client.CreateAsync(new CreateWebhookRequest
        {
            SubscribedEvents = new List<string> { WebhookEvents.BatchFailed }
        }, TestContext.Current.CancellationToken));
        await Should.ThrowAsync<ArgumentException>(() => client.CreateAsync(new CreateWebhookRequest
        {
            Uri = "https://example.com/hook", SubscribedEvents = new List<string>()
        }, TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAsync_GetsById_AndAcceptsResourcePrefix()
    {
        var (client, handler) = CreateGoogle(WebhookJson, WebhookJson);

        var result = await client.GetAsync("wh_123", TestContext.Current.CancellationToken);
        await client.GetAsync("webhooks/wh_123", TestContext.Current.CancellationToken);

        handler.Requests.Count.ShouldBe(2);
        handler.Requests.ShouldAllBe(r => r.Method == HttpMethod.Get);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123");
        handler.Requests[1].Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123");
        handler.Requests[0].Body.ShouldBeNull();
        result.Id.ShouldBe("wh_123");
    }

    [Fact]
    public async Task GetAsync_RejectsEmptyId()
    {
        var (client, _) = CreateGoogle();
        await Should.ThrowAsync<ArgumentException>(() => client.GetAsync(" ", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListAsync_SendsSnakeCasePagingQuery_AndParsesPage()
    {
        var (client, handler) = CreateGoogle($$"""{"webhooks": [{{WebhookJson}}], "next_page_token": "tok 2"}""",
            "{}");

        var page = await client.ListAsync(10, "tok/1", TestContext.Current.CancellationToken);
        await client.ListAsync(cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests[0].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe($"{GoogleBase}?page_size=10&page_token=tok%2F1");
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(GoogleBase);
        page.Webhooks.ShouldHaveSingleItem().Id.ShouldBe("wh_123");
        page.NextPageToken.ShouldBe("tok 2");
    }

    [Fact]
    public async Task ListAllAsync_FollowsNextPageToken()
    {
        var (client, handler) = CreateGoogle(
            """{"webhooks": [{"id": "a", "uri": "u", "subscribed_events": []}], "next_page_token": "p2"}""",
            """{"webhooks": [{"id": "b", "uri": "u", "subscribed_events": []}]}""");

        var ids = new List<string?>();
        await foreach (var webhook in client.ListAllAsync(5, TestContext.Current.CancellationToken))
            ids.Add(webhook.Id);

        ids.ShouldBe(new[] { "a", "b" });
        handler.Requests[0].Url.AbsoluteUri.ShouldBe($"{GoogleBase}?page_size=5");
        handler.Requests[1].Url.AbsoluteUri.ShouldBe($"{GoogleBase}?page_size=5&page_token=p2");
    }

    [Fact]
    public async Task UpdateAsync_PatchesWithUpdateMask_AndOnlySetFields()
    {
        var (client, handler) = CreateGoogle(WebhookJson);

        var result = await client.UpdateAsync("wh_123", new WebhookUpdate
        {
            State = WebhookStates.Disabled,
            SubscribedEvents = new List<string> { WebhookEvents.BatchExpired }
        }, "state,subscribed_events", TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.Method.ShouldBe("PATCH");
        request.Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123?update_mask=state%2Csubscribed_events");
        var body = JsonNode.Parse(request.Body!)!.AsObject();
        body["state"]!.GetValue<string>().ShouldBe("disabled");
        body["subscribed_events"]![0]!.GetValue<string>().ShouldBe("batch.expired");
        body.ContainsKey("uri").ShouldBeFalse();
        body.ContainsKey("name").ShouldBeFalse();
        result.Id.ShouldBe("wh_123");
    }

    [Fact]
    public async Task UpdateAsync_WithoutMask_HasNoQuery()
    {
        var (client, handler) = CreateGoogle(WebhookJson);

        await client.UpdateAsync("webhooks/wh_123", new WebhookUpdate { Uri = "https://example.com/new" },
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123");
        JsonNode.Parse(request.Body!)!["uri"]!.GetValue<string>().ShouldBe("https://example.com/new");
    }

    [Fact]
    public async Task DeleteAsync_SendsDelete()
    {
        var (client, handler) = CreateGoogle("{}");

        await client.DeleteAsync("wh_123", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Delete);
        request.Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123");
    }

    [Fact]
    public async Task PingAsync_PostsEmptyObjectToPingVerb()
    {
        var (client, handler) = CreateGoogle("{}");

        var response = await client.PingAsync("wh_123", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123:ping");
        JsonNode.Parse(request.Body!)!.AsObject().Count.ShouldBe(0);
        response.ShouldNotBeNull();
    }

    [Fact]
    public async Task RotateSigningSecretAsync_PostsRevocationBehavior_AndParsesSecret()
    {
        var (client, handler) = CreateGoogle("""{"secret": "whsec_rotated"}""", """{"secret": "s2"}""");

        var response = await client.RotateSigningSecretAsync("wh_123",
            WebhookRevocationBehaviors.RevokePreviousSecretsImmediately, TestContext.Current.CancellationToken);
        await client.RotateSigningSecretAsync("wh_123", cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests[0];
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{GoogleBase}/wh_123:rotateSigningSecret");
        JsonNode.Parse(request.Body!)!["revocation_behavior"]!.GetValue<string>()
            .ShouldBe("revoke_previous_secrets_immediately");
        JsonNode.Parse(handler.Requests[1].Body!)!.AsObject().Count.ShouldBe(0);
        response.Secret.ShouldBe("whsec_rotated");
    }

    [Fact]
    public async Task VertexAI_UsesProjectLocationPathAndBearerToken()
    {
        var handler = new CapturingHandler().Enqueue(WebhookJson);
        var platform = new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token",
            validateAccessToken: false);
        var client = new WebhooksClient(platform, new HttpClient(handler));

        await client.GetAsync("wh_123", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Url.AbsoluteUri.ShouldBe(
            $"https://us-central1-aiplatform.googleapis.com/{platform.GetApiVersion()}/projects/test-project/locations/us-central1/webhooks/wh_123");
        request.Message.Headers.Authorization!.Scheme.ShouldBe("Bearer");
    }

    [Fact]
    public void GenAIFactory_CreatesWebhooksClient()
    {
        var client = new GoogleAi("test-key").CreateWebhooksClient();
        client.ShouldNotBeNull();
        client.Platform.ShouldBeOfType<GoogleAIPlatformAdapter>();
    }

    [Fact]
    public async Task BatchCreate_SendsWebhookConfigInsideBatch()
    {
        var handler = new CapturingHandler().Enqueue(
            """{"name": "batches/b1", "metadata": {"@type": "type.googleapis.com/google.ai.generativelanguage.v1main.GenerateContentBatch", "model": "models/gemini-3-flash-preview", "state": "BATCH_STATE_PENDING"}}""");
        var batchClient = new BatchClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler));

        var job = await batchClient.CreateAsync("gemini-3-flash-preview", new BatchJobSource { FileName = "files/input" },
            null, new WebhookConfig
            {
                Uris = new List<string> { "https://example.com/hook" },
                UserMetadata = new Dictionary<string, JsonNode?> { ["job"] = "nightly", ["attempt"] = 2 }
            }, TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Url.AbsoluteUri.ShouldBe(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:batchGenerateContent");
        var batch = JsonNode.Parse(request.Body!)!["batch"]!;
        batch["inputConfig"]!["fileName"]!.GetValue<string>().ShouldBe("files/input");
        batch["webhookConfig"]!["uris"]![0]!.GetValue<string>().ShouldBe("https://example.com/hook");
        batch["webhookConfig"]!["userMetadata"]!["job"]!.GetValue<string>().ShouldBe("nightly");
        batch["webhookConfig"]!["userMetadata"]!["attempt"]!.GetValue<int>().ShouldBe(2);
        job.Name.ShouldBe("batches/b1");
    }

    [Fact]
    public async Task BatchCreate_WithoutWebhookConfig_OmitsField()
    {
        var handler = new CapturingHandler().Enqueue("""{"name": "batches/b1", "metadata": {}}""");
        var batchClient = new BatchClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler));

        await batchClient.CreateAsync("gemini-3-flash-preview", new BatchJobSource { FileName = "files/input" },
            cancellationToken: TestContext.Current.CancellationToken);

        JsonNode.Parse(handler.Requests.Single().Body!)!["batch"]!.AsObject().ContainsKey("webhookConfig")
            .ShouldBeFalse();
    }

    [Fact]
    public async Task BatchCreate_WebhookConfigOnVertex_Throws()
    {
        var handler = new CapturingHandler();
        var platform = new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token",
            validateAccessToken: false);
        var batchClient = new BatchClient(platform, new HttpClient(handler));

        await Should.ThrowAsync<NotSupportedException>(() => batchClient.CreateAsync("gemini-3-flash-preview",
            new BatchJobSource { GcsUri = new List<string> { "gs://b/in.jsonl" } }, null,
            new WebhookConfig { Uris = new List<string> { "https://example.com/hook" } },
            TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public void WebhookConfig_RoundTripsThroughTypesContext()
    {
        var json = JsonSerializer.Serialize(new WebhookConfig { Uris = new List<string> { "u" } },
            TypesSerializerContext.Default.WebhookConfig);
        var parsed = JsonSerializer.Deserialize(json, TypesSerializerContext.Default.WebhookConfig)!;
        parsed.Uris.ShouldBe(new[] { "u" });
        parsed.UserMetadata.ShouldBeNull();
    }
}
