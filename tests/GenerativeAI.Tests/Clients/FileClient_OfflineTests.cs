using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline tests for the <see cref="FileClient"/> download and register methods.
/// </summary>
public class FileClient_OfflineTests
{
    private const string GeminiBase = "https://generativelanguage.googleapis.com/v1beta";

    private static (FileClient Client, RecordingHttpHandler Handler) Gemini(string? accessToken = null)
    {
        var handler = new RecordingHttpHandler();
        var platform = new GoogleAIPlatformAdapter("key", accessToken: accessToken, validateAccessToken: false);
        return (new FileClient(platform, new HttpClient(handler)), handler);
    }

    private static (FileClient Client, RecordingHttpHandler Handler) Vertex()
    {
        var handler = new RecordingHttpHandler();
        var platform = new VertextPlatformAdapter("p", "us-central1", accessToken: "t", validateAccessToken: false);
        return (new FileClient(platform, new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task Download_GetsMediaFromDownloadEndpoint()
    {
        var (client, handler) = Gemini();
        var payload = new byte[] { 1, 2, 3, 4, 5 };
        handler.ReplyBytes(payload, "video/mp4");

        var data = await client.DownloadFileAsync("files/abc123", TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe($"{GeminiBase}/files/abc123:download?alt=media");
        request.Headers["x-goog-api-key"].ShouldBe("key");
        data.ShouldBe(payload);
    }

    [Fact]
    public async Task Download_AcceptsIdsUrisFilesAndStreams()
    {
        var (client, handler) = Gemini();
        handler.ReplyBytes(new byte[] { 7, 8 });

        await client.DownloadFileAsync("abc123", TestContext.Current.CancellationToken);
        await client.DownloadFileAsync(
            "https://generativelanguage.googleapis.com/v1beta/files/xyz789:download?alt=media",
            TestContext.Current.CancellationToken);
        await client.DownloadFileAsync(new RemoteFile { Name = "files/gen1", DownloadUri = "https://example/download" },
            TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        await client.DownloadFileAsync("files/abc123", destination, TestContext.Current.CancellationToken);

        handler.Requests.Select(r => r.Url.AbsoluteUri).ShouldBe(new[]
        {
            $"{GeminiBase}/files/abc123:download?alt=media",
            $"{GeminiBase}/files/xyz789:download?alt=media",
            $"{GeminiBase}/files/gen1:download?alt=media",
            $"{GeminiBase}/files/abc123:download?alt=media"
        });
        destination.ToArray().ShouldBe(new byte[] { 7, 8 });
    }

    [Fact]
    public async Task Download_RejectsUploadedFilesAndVertex()
    {
        var (client, handler) = Gemini();
        await Should.ThrowAsync<ArgumentException>(() =>
            client.DownloadFileAsync(new RemoteFile { Name = "files/uploaded" }, TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();

        var (vertex, _) = Vertex();
        await Should.ThrowAsync<NotSupportedException>(() =>
            vertex.DownloadFileAsync("files/abc", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Register_PostsUrisWithExplicitAccessToken()
    {
        var (client, handler) = Gemini();
        handler.Reply("""
            {"files": [{"name": "files/g1", "uri": "https://generativelanguage.googleapis.com/v1beta/files/g1", "mimeType": "text/plain", "source": "REGISTERED", "state": "ACTIVE"}]}
            """);

        var response = await client.RegisterFilesAsync(new[] { "gs://bucket/a.txt", "gs://bucket/b.txt" },
            accessToken: "oauth-token", quotaProjectId: "quota-project",
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{GeminiBase}/files:register");
        request.Headers["Authorization"].ShouldBe("Bearer oauth-token");
        request.Headers["x-goog-user-project"].ShouldBe("quota-project");
        request.Body!["uris"]!.AsArray().Select(n => n!.GetValue<string>())
            .ShouldBe(new[] { "gs://bucket/a.txt", "gs://bucket/b.txt" });
        var file = response.Files.ShouldHaveSingleItem();
        file.Name.ShouldBe("files/g1");
        file.MimeType.ShouldBe("text/plain");
        file.Source.ShouldBe(Source.REGISTERED);
    }

    [Fact]
    public async Task Register_UsesPlatformAccessTokenAndIsGeminiOnly()
    {
        var (client, handler) = Gemini(accessToken: "platform-token");
        handler.Reply("""{"files": []}""");

        await client.RegisterFilesAsync(new[] { "gs://bucket/a.txt" },
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Headers["Authorization"].ShouldBe("Bearer platform-token");
        request.Headers.ContainsKey("x-goog-user-project").ShouldBeFalse();

        var (vertex, _) = Vertex();
        await Should.ThrowAsync<NotSupportedException>(() => vertex.RegisterFilesAsync(new[] { "gs://b/a" },
            cancellationToken: TestContext.Current.CancellationToken));
    }
}
