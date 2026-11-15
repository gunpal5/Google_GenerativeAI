using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline unit tests for the RAG Engine configuration API (Serverless / Spanner mode).
/// </summary>
public class RagEngineConfig_Tests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Queue<string> _responses = new();

        public List<(HttpMethod Method, Uri Url, string? Body)> Requests { get; } = new();

        public CapturingHandler Enqueue(string body)
        {
            _responses.Enqueue(body);
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.Method, request.RequestUri!, body));
            var responseBody = _responses.Count > 0 ? _responses.Dequeue() : "{}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }

    private static VertextPlatformAdapter CreatePlatform() =>
        new("test-project", "us-central1", accessToken: "test-token", validateAccessToken: false);

    private static string ConfigUrl(VertextPlatformAdapter platform) =>
        $"https://us-central1-aiplatform.googleapis.com/{platform.GetApiVersion()}/projects/test-project/locations/us-central1/ragEngineConfig";

    [Fact]
    public void SerializerContext_ProvidesTypeInfoForRagEngineConfigTypes()
    {
        TypesSerializerContext.Default.GetTypeInfo(typeof(RagEngineConfig)).ShouldNotBeNull();
        TypesSerializerContext.Default.GetTypeInfo(typeof(RagManagedDbConfig)).ShouldNotBeNull();
        TypesSerializerContext.Default.GetTypeInfo(typeof(RagManagedDbSpanner)).ShouldNotBeNull();
    }

    [Fact]
    public async Task GetRagEngineConfigAsync_UsesLocationPath_AndParsesSpannerMode()
    {
        var platform = CreatePlatform();
        var handler = new CapturingHandler().Enqueue("""
            {
              "name": "projects/test-project/locations/us-central1/ragEngineConfig",
              "ragManagedDbConfig": { "spanner": { "basic": {} } }
            }
            """);
        var client = new RagCorpusClient(platform, new HttpClient(handler));

        var config = await client.GetRagEngineConfigAsync(TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe(ConfigUrl(platform));
        config.ShouldNotBeNull();
        config.Name.ShouldBe("projects/test-project/locations/us-central1/ragEngineConfig");
        config.RagManagedDbConfig!.Spanner!.Basic.ShouldNotBeNull();
        config.RagManagedDbConfig.Serverless.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateRagEngineConfigAsync_PatchesServerlessBody()
    {
        var platform = CreatePlatform();
        var handler = new CapturingHandler().Enqueue("""{ "name": "projects/test-project/locations/us-central1/operations/1", "done": true }""");
        var client = new RagCorpusClient(platform, new HttpClient(handler));

        var operation = await client.UpdateRagEngineConfigAsync(
            new RagEngineConfig { RagManagedDbConfig = RagManagedDbConfig.CreateServerless() },
            TestContext.Current.CancellationToken);

        operation.ShouldNotBeNull();
        operation.Done.ShouldBe(true);
        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.Method.ShouldBe("PATCH");
        request.Url.AbsoluteUri.ShouldBe(ConfigUrl(platform));
        var body = JsonNode.Parse(request.Body!)!.AsObject();
        body.ContainsKey("name").ShouldBeFalse();
        var dbConfig = body["ragManagedDbConfig"]!.AsObject();
        dbConfig.ContainsKey("serverless").ShouldBeTrue();
        dbConfig.ContainsKey("spanner").ShouldBeFalse();
    }

    [Fact]
    public void CreateSpannerScaled_SetsOnlyTheScaledTier()
    {
        var config = RagManagedDbConfig.CreateSpannerScaled();

        config.Serverless.ShouldBeNull();
        config.Spanner!.Scaled.ShouldNotBeNull();
        config.Spanner.Basic.ShouldBeNull();
        config.Spanner.Unprovisioned.ShouldBeNull();
    }
}
