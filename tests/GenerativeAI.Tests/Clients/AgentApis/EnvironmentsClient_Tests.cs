using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>
/// Offline tests of <see cref="EnvironmentsClient"/> (wire format follows python-genai's
/// <c>_gaos/environments.py</c>, <c>_gaos/files.py</c> and <c>_gaos/_internal.py</c>).
/// </summary>
public class EnvironmentsClient_Tests
{
    private const string Base = "https://generativelanguage.googleapis.com/v1beta/environments";

    private const string EnvironmentJson = """
        {
          "id": "env-1", "status": "active", "network": {"allowlist": [{"domain": "api.github.com", "credential": "gh"}]},
          "sources": [{"type": "repository", "source": "https://github.com/a/b", "target": "/src"}],
          "file_count": "42", "size_bytes": "1024", "created": "2026-11-01T00:00:00Z", "last_accessed": "2026-11-02T00:00:00Z"
        }
        """;

    private static (EnvironmentsClient Client, AgentApiTestHandler Handler) Create(params string[] responses)
    {
        var handler = new AgentApiTestHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new EnvironmentsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task CreateAsync_PostsSourcesAndNetwork()
    {
        var (client, handler) = Create(EnvironmentJson);

        var environment = await client.CreateAsync(new CreateEnvironmentRequest
        {
            Network = EnvironmentNetworkModes.Disabled,
            Sources = new List<InteractionEnvironmentSource>
            {
                new() { Type = EnvironmentSourceTypes.Gcs, Source = "gs://my-bucket/data", Target = "/data" }
            }
        }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(Base);
        var body = JsonNode.Parse(request.Body!)!;
        body["network"]!.GetValue<string>().ShouldBe("disabled");
        body["sources"]![0]!["type"]!.GetValue<string>().ShouldBe("gcs");
        body["sources"]![0]!["source"]!.GetValue<string>().ShouldBe("gs://my-bucket/data");
        body.AsObject().ContainsKey("from_environment").ShouldBeFalse();

        environment.Id.ShouldBe("env-1");
        environment.Status.ShouldBe(EnvironmentStatuses.Active);
        environment.FileCount.ShouldBe(42);
        environment.SizeBytes.ShouldBe(1024);
        environment.Network!["allowlist"]![0]!["domain"]!.GetValue<string>().ShouldBe("api.github.com");
        environment.Sources!.Single().Type.ShouldBe(EnvironmentSourceTypes.Repository);
        environment.LastAccessed.ShouldBe("2026-11-02T00:00:00Z");
    }

    [Fact]
    public async Task CreateAsync_FromEnvironmentClone()
    {
        var (client, handler) = Create(EnvironmentJson);
        await client.CreateAsync(new CreateEnvironmentRequest { FromEnvironment = "environments/env-0" },
            TestContext.Current.CancellationToken);
        JsonNode.Parse(handler.Requests.Single().Body!)!["from_environment"]!.GetValue<string>()
            .ShouldBe("environments/env-0");
    }

    [Fact]
    public async Task GetDeleteAndList_UseExpectedUrls()
    {
        var (client, handler) = Create(EnvironmentJson, "{}",
            """{"environments":[{"id":"env-1"}],"next_page_token":"n"}""");

        await client.GetAsync("environments/env-1", TestContext.Current.CancellationToken);
        await client.DeleteAsync("env-1", TestContext.Current.CancellationToken);
        var page = await client.ListAsync(3, cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests[0].Url.AbsoluteUri.ShouldBe(Base + "/env-1");
        handler.Requests[1].Method.ShouldBe(HttpMethod.Delete);
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(Base + "/env-1");
        handler.Requests[2].Url.AbsoluteUri.ShouldBe(Base + "?page_size=3");
        page.Environments!.Single().Id.ShouldBe("env-1");
    }

    [Fact]
    public async Task ListFilesAsync_UsesFilesPathAndQuery()
    {
        var (client, handler) = Create("""
            {"files":[{"name":"main.py","path":"src/main.py","type":"file","mime_type":"text/x-python",
                       "size_bytes":"12","created":"2026-11-01T00:00:00Z","modified":"2026-11-01T00:00:01Z"},
                      {"name":"lib","path":"src/lib","type":"directory"}],
             "next_page_token":"x"}
            """);

        var page = await client.ListFilesAsync("env-1", "/src", recursive: true, pageSize: 50,
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe(Base + "/env-1/files/src?page_size=50&recursive=true");
        page.Files!.Count.ShouldBe(2);
        page.Files[0].SizeBytes.ShouldBe(12);
        page.Files[0].MimeType.ShouldBe("text/x-python");
        page.Files[1].Type.ShouldBe(EnvironmentFileTypes.Directory);
        page.NextPageToken.ShouldBe("x");
    }

    [Fact]
    public async Task DownloadFileAsync_RequestsAltMedia()
    {
        var (client, handler) = Create("file-content");

        var bytes = await client.DownloadFileAsync("env-1", "src/main.py", TestContext.Current.CancellationToken);

        handler.Requests.Single().Url.AbsoluteUri.ShouldBe(Base + "/env-1/files/src/main.py?alt=media");
        Encoding.UTF8.GetString(bytes).ShouldBe("file-content");
    }

    [Fact]
    public async Task UploadFileAsync_UsesResumableUploadProtocol()
    {
        var handler = new AgentApiTestHandler()
            .Enqueue("", new Dictionary<string, string> { ["X-Goog-Upload-URL"] = "https://upload.example/session/1" })
            .Enqueue("""{"name":"data.tar","path":"work/data.tar","type":"file","size_bytes":"5"}""");
        var client = new EnvironmentsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler));

        var result = await client.UploadFileAsync("environments/env-1", "/work/data.tar",
            new MemoryStream(Encoding.UTF8.GetBytes("hello")), "application/x-tar", overwrite: true, extract: false,
            cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests.Count.ShouldBe(2);
        var start = handler.Requests[0];
        start.Method.ShouldBe(HttpMethod.Put);
        start.Url.AbsoluteUri.ShouldBe(
            "https://generativelanguage.googleapis.com/upload/v1beta/environments/env-1/files/work/data.tar?extract=false&overwrite=true");
        start.Headers["X-Goog-Upload-Protocol"].ShouldBe("resumable");
        start.Headers["X-Goog-Upload-Command"].ShouldBe("start");
        start.Headers["X-Goog-Upload-Header-Content-Length"].ShouldBe("5");
        start.Headers["X-Goog-Upload-Header-Content-Type"].ShouldBe("application/x-tar");

        var upload = handler.Requests[1];
        upload.Method.ShouldBe(HttpMethod.Post);
        upload.Url.AbsoluteUri.ShouldBe("https://upload.example/session/1");
        upload.Headers["X-Goog-Upload-Command"].ShouldBe("upload, finalize");
        upload.Headers["X-Goog-Upload-Offset"].ShouldBe("0");
        upload.Body.ShouldBe("hello");

        result.Files!.Single().Path.ShouldBe("work/data.tar");
        result.Files[0].SizeBytes.ShouldBe(5);
    }

    [Fact]
    public async Task UploadFileAsync_NormalizesFileListAndWrappedFileResponses()
    {
        var handler = new AgentApiTestHandler()
            .Enqueue("", new Dictionary<string, string> { ["X-Goog-Upload-URL"] = "https://upload.example/a" })
            .Enqueue("""{"files":[{"path":"a"},{"path":"b"}]}""")
            .Enqueue("", new Dictionary<string, string> { ["X-Goog-Upload-URL"] = "https://upload.example/b" })
            .Enqueue("""{"file":{"path":"c"}}""");
        var client = new EnvironmentsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler));

        var list = await client.UploadFileAsync("env-1", "x", new MemoryStream(new byte[] { 1 }),
            cancellationToken: TestContext.Current.CancellationToken);
        var wrapped = await client.UploadFileAsync("env-1", "c", new MemoryStream(new byte[] { 1 }),
            cancellationToken: TestContext.Current.CancellationToken);

        list.Files!.Select(f => f.Path).ShouldBe(new[] { "a", "b" });
        wrapped.Files!.Single().Path.ShouldBe("c");
        handler.Requests[0].Headers["X-Goog-Upload-Header-Content-Type"].ShouldBe("application/octet-stream");
    }

    [Fact]
    public async Task VertexAI_UsesProjectLocationPath()
    {
        var handler = new AgentApiTestHandler().Enqueue(EnvironmentJson);
        var platform = AgentApiTestHandler.VertexPlatform();
        var client = new EnvironmentsClient(platform, new HttpClient(handler));

        await client.GetAsync("env-1", TestContext.Current.CancellationToken);

        handler.Requests.Single().Url.AbsoluteUri.ShouldBe(
            $"https://us-central1-aiplatform.googleapis.com/{platform.GetApiVersion()}/projects/test-project/locations/us-central1/environments/env-1");
    }
}
