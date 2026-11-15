using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline tests for <see cref="FileSearchStoresClient"/>: request shape (URL, method, query, headers, JSON body)
/// and deserialization of wire-format responses, checked against python-genai's file_search_stores/documents modules.
/// </summary>
public class FileSearchStoresClient_Tests
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";
    private const string UploadSessionUrl = "https://generativelanguage.googleapis.com/upload/v1beta/fileSearchStores/my-store:uploadToFileSearchStore?upload_id=abc&upload_protocol=resumable";

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri Url,
        Dictionary<string, string> Headers,
        byte[] Body,
        string? ContentType)
    {
        public string BodyText => Encoding.UTF8.GetString(Body);
        public JsonNode Json => JsonNode.Parse(BodyText)!;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Queue<(string Json, Dictionary<string, string>? Headers)> _responses = new();

        public List<CapturedRequest> Requests { get; } = new();

        public CapturingHandler Respond(string json, Dictionary<string, string>? headers = null)
        {
            _responses.Enqueue((json, headers));
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value),
                StringComparer.OrdinalIgnoreCase);
            var body = request.Content == null
                ? Array.Empty<byte>()
                : await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri!, headers, body,
                request.Content?.Headers.ContentType?.MediaType));

            var (json, responseHeaders) = _responses.Count > 0 ? _responses.Dequeue() : ("{}", null);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (responseHeaders != null)
            {
                foreach (var header in responseHeaders)
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return response;
        }
    }

    private static (FileSearchStoresClient Client, CapturingHandler Handler) Create()
    {
        var handler = new CapturingHandler();
        var client = new FileSearchStoresClient(new GoogleAIPlatformAdapter("test-api-key"), new HttpClient(handler));
        return (client, handler);
    }

    private const string StoreJson = """
        {
          "name": "fileSearchStores/my-store-123a456b789c",
          "displayName": "My Store",
          "createTime": "2025-11-01T10:00:00.123456Z",
          "updateTime": "2025-11-02T11:30:00Z",
          "activeDocumentsCount": "3",
          "pendingDocumentsCount": "1",
          "failedDocumentsCount": "0",
          "sizeBytes": "4096",
          "embeddingModel": "models/gemini-embedding-001"
        }
        """;

    private const string DocumentJson = """
        {
          "name": "fileSearchStores/my-store/documents/doc-1",
          "displayName": "Doc One",
          "state": "STATE_ACTIVE",
          "sizeBytes": "1234",
          "mimeType": "application/pdf",
          "createTime": "2025-11-01T10:00:00Z",
          "updateTime": "2025-11-01T10:05:00Z",
          "customMetadata": [
            { "key": "author", "stringValue": "Robert Graves" },
            { "key": "year", "numericValue": 1934 },
            { "key": "tags", "stringListValue": { "values": ["history", "rome"] } }
          ]
        }
        """;

    #region Stores

    [Fact]
    public async Task CreateFileSearchStore_PostsDisplayNameAndEmbeddingModel()
    {
        var (client, handler) = Create();
        handler.Respond(StoreJson);

        var store = await client.CreateFileSearchStoreAsync("My Store", "gemini-embedding-001",
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores");
        request.Url.Query.ShouldBeEmpty();
        request.Headers["x-goog-api-key"].ShouldBe("test-api-key");
        var body = request.Json.AsObject();
        body.Count.ShouldBe(2);
        body["displayName"]!.GetValue<string>().ShouldBe("My Store");
        body["embeddingModel"]!.GetValue<string>().ShouldBe("models/gemini-embedding-001");

        store.Name.ShouldBe("fileSearchStores/my-store-123a456b789c");
        store.DisplayName.ShouldBe("My Store");
        store.ActiveDocumentsCount.ShouldBe(3);
        store.PendingDocumentsCount.ShouldBe(1);
        store.FailedDocumentsCount.ShouldBe(0);
        store.SizeBytes.ShouldBe(4096);
        store.EmbeddingModel.ShouldBe("models/gemini-embedding-001");
        store.CreateTime.ShouldNotBeNull();
        store.UpdateTime.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateFileSearchStore_WithoutOptions_SendsEmptyBody()
    {
        var (client, handler) = Create();
        handler.Respond("""{"name": "fileSearchStores/random-123"}""");

        var store = await client.CreateFileSearchStoreAsync(cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests.ShouldHaveSingleItem().Json.AsObject().Count.ShouldBe(0);
        store.Name.ShouldBe("fileSearchStores/random-123");
    }

    [Theory]
    [InlineData("my-store")]
    [InlineData("fileSearchStores/my-store")]
    public async Task GetFileSearchStore_GetsByName(string name)
    {
        var (client, handler) = Create();
        handler.Respond(StoreJson);

        var store = await client.GetFileSearchStoreAsync(name, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores/my-store");
        store.DisplayName.ShouldBe("My Store");
    }

    [Fact]
    public async Task GetFileSearchStore_RejectsForeignResourceName()
    {
        var (client, _) = Create();
        await Should.ThrowAsync<ArgumentException>(() =>
            client.GetFileSearchStoreAsync("corpora/abc", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListFileSearchStores_SendsPagingQueryAndParsesNextPageToken()
    {
        var (client, handler) = Create();
        handler.Respond($$"""{"fileSearchStores": [{{StoreJson}}, {"name": "fileSearchStores/b"}], "nextPageToken": "next/token=="}""");

        var page = await client.ListFileSearchStoresAsync(10, "tok+en/1", TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.GetLeftPart(UriPartial.Path).ShouldBe($"{BaseUrl}/fileSearchStores");
        request.Url.Query.ShouldBe("?pageSize=10&pageToken=tok%2Ben%2F1");
        page.FileSearchStores!.Count.ShouldBe(2);
        page.FileSearchStores[0].ActiveDocumentsCount.ShouldBe(3);
        page.FileSearchStores[1].Name.ShouldBe("fileSearchStores/b");
        page.NextPageToken.ShouldBe("next/token==");
    }

    [Fact]
    public async Task ListFileSearchStores_WithoutPaging_HasNoQuery()
    {
        var (client, handler) = Create();
        handler.Respond("{}");

        var page = await client.ListFileSearchStoresAsync(cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests.ShouldHaveSingleItem().Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores");
        page.FileSearchStores.ShouldBeNull();
        page.NextPageToken.ShouldBeNull();
    }

    [Fact]
    public async Task ListAllFileSearchStores_FollowsNextPageToken()
    {
        var (client, handler) = Create();
        handler.Respond("""{"fileSearchStores": [{"name": "fileSearchStores/a"}], "nextPageToken": "p2"}""")
            .Respond("""{"fileSearchStores": [{"name": "fileSearchStores/b"}]}""");

        var names = new List<string?>();
        await foreach (var store in client.ListAllFileSearchStoresAsync(1, TestContext.Current.CancellationToken))
            names.Add(store.Name);

        names.ShouldBe(new[] { "fileSearchStores/a", "fileSearchStores/b" });
        handler.Requests.Count.ShouldBe(2);
        handler.Requests[0].Url.Query.ShouldBe("?pageSize=1");
        handler.Requests[1].Url.Query.ShouldBe("?pageSize=1&pageToken=p2");
    }

    [Theory]
    [InlineData(true, "?force=true")]
    [InlineData(false, "?force=false")]
    [InlineData(null, "")]
    public async Task DeleteFileSearchStore_SendsDeleteWithForce(bool? force, string expectedQuery)
    {
        var (client, handler) = Create();

        await client.DeleteFileSearchStoreAsync("my-store", force, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Delete);
        request.Url.GetLeftPart(UriPartial.Path).ShouldBe($"{BaseUrl}/fileSearchStores/my-store");
        request.Url.Query.ShouldBe(expectedQuery);
        request.Body.ShouldBeEmpty();
    }

    #endregion

    #region Upload / import / operations

    private const string PendingUploadOperationJson = """
        {"name": "fileSearchStores/my-store/upload/operations/op-1", "done": false}
        """;

    private const string DoneUploadOperationJson = """
        {
          "name": "fileSearchStores/my-store/upload/operations/op-1",
          "done": true,
          "metadata": {"@type": "type.googleapis.com/google.ai.generativelanguage.v1main.UploadToFileSearchStoreMetadata"},
          "response": {
            "@type": "type.googleapis.com/google.ai.generativelanguage.v1main.UploadToFileSearchStoreResponse",
            "parent": "fileSearchStores/my-store",
            "documentName": "fileSearchStores/my-store/documents/doc-1"
          }
        }
        """;

    [Fact]
    public async Task UploadToFileSearchStore_UsesResumableProtocol()
    {
        var (client, handler) = Create();
        handler.Respond("{}", new Dictionary<string, string> { ["X-Goog-Upload-URL"] = UploadSessionUrl })
            .Respond(PendingUploadOperationJson);

        var path = Path.Combine(Path.GetTempPath(), $"fs-upload-{Guid.NewGuid():N}.txt");
        var content = "File Search test content";
        await File.WriteAllTextAsync(path, content, TestContext.Current.CancellationToken);
        var progress = new List<double>();
        try
        {
            var operation = await client.UploadToFileSearchStoreAsync("my-store", path,
                new UploadToFileSearchStoreConfig
                {
                    DisplayName = "My Doc",
                    CustomMetadata = new List<CustomMetadata>
                    {
                        new("author") { StringValue = "Robert Graves" },
                        new("year") { NumericValue = 1934 }
                    },
                    ChunkingConfig = new ChunkingConfig
                    {
                        WhiteSpaceConfig = new WhiteSpaceConfig { MaxTokensPerChunk = 200, MaxOverlapTokens = 20 }
                    }
                }, progress.Add, TestContext.Current.CancellationToken);

            handler.Requests.Count.ShouldBe(2);

            var start = handler.Requests[0];
            start.Method.ShouldBe(HttpMethod.Post);
            start.Url.AbsoluteUri.ShouldBe(
                "https://generativelanguage.googleapis.com/upload/v1beta/fileSearchStores/my-store:uploadToFileSearchStore");
            start.Headers["X-Goog-Upload-Protocol"].ShouldBe("resumable");
            start.Headers["X-Goog-Upload-Command"].ShouldBe("start");
            start.Headers["X-Goog-Upload-Header-Content-Length"].ShouldBe(Encoding.UTF8.GetByteCount(content).ToString());
            start.Headers["X-Goog-Upload-Header-Content-Type"].ShouldBe("text/plain");
            start.Headers["x-goog-api-key"].ShouldBe("test-api-key");
            start.ContentType.ShouldBe("application/json");
            var body = start.Json.AsObject();
            body.Count.ShouldBe(4);
            body["displayName"]!.GetValue<string>().ShouldBe("My Doc");
            body["mimeType"]!.GetValue<string>().ShouldBe("text/plain");
            body["customMetadata"]![0]!["key"]!.GetValue<string>().ShouldBe("author");
            body["customMetadata"]![0]!["stringValue"]!.GetValue<string>().ShouldBe("Robert Graves");
            body["customMetadata"]![1]!["numericValue"]!.GetValue<double>().ShouldBe(1934);
            var whiteSpace = body["chunkingConfig"]!["whiteSpaceConfig"]!;
            whiteSpace["maxTokensPerChunk"]!.GetValue<int>().ShouldBe(200);
            whiteSpace["maxOverlapTokens"]!.GetValue<int>().ShouldBe(20);

            var upload = handler.Requests[1];
            upload.Method.ShouldBe(HttpMethod.Post);
            upload.Url.AbsoluteUri.ShouldBe(UploadSessionUrl);
            upload.Headers["X-Goog-Upload-Command"].ShouldBe("upload, finalize");
            upload.Headers["X-Goog-Upload-Offset"].ShouldBe("0");
            upload.Headers["x-goog-api-key"].ShouldBe("test-api-key");
            upload.BodyText.ShouldBe(content);

            operation.Name.ShouldBe("fileSearchStores/my-store/upload/operations/op-1");
            operation.Done.ShouldBe(false);
            progress.ShouldNotBeEmpty();
            progress[^1].ShouldBe(100);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task UploadStreamToFileSearchStore_UsesGivenMimeTypeAndReadsFromCurrentPosition()
    {
        var (client, handler) = Create();
        handler.Respond("{}", new Dictionary<string, string> { ["x-goog-upload-url"] = UploadSessionUrl })
            .Respond(DoneUploadOperationJson);

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("SKIPpdf-bytes"));
        stream.Position = 4;

        var operation = await client.UploadStreamToFileSearchStoreAsync("fileSearchStores/my-store", stream,
            "application/pdf", new UploadToFileSearchStoreConfig { MimeType = "text/plain", DisplayName = "Doc" },
            cancellationToken: TestContext.Current.CancellationToken);

        var start = handler.Requests[0];
        start.Headers["X-Goog-Upload-Header-Content-Length"].ShouldBe("9");
        start.Headers["X-Goog-Upload-Header-Content-Type"].ShouldBe("application/pdf");
        start.Json["mimeType"]!.GetValue<string>().ShouldBe("application/pdf");
        start.Json["displayName"]!.GetValue<string>().ShouldBe("Doc");
        handler.Requests[1].BodyText.ShouldBe("pdf-bytes");

        operation.Done.ShouldBe(true);
        operation.Response!.Parent.ShouldBe("fileSearchStores/my-store");
        operation.Response.DocumentName.ShouldBe("fileSearchStores/my-store/documents/doc-1");
        operation.Metadata.ShouldNotBeNull();
    }

    [Fact]
    public async Task UploadToFileSearchStore_ThrowsWhenNoUploadUrlReturned()
    {
        var (client, handler) = Create();
        handler.Respond("{}");

        await Should.ThrowAsync<GenerativeAIException>(() => client.UploadStreamToFileSearchStoreAsync("my-store",
            new MemoryStream(new byte[] { 1, 2, 3 }), "application/octet-stream",
            cancellationToken: TestContext.Current.CancellationToken));
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ImportFile_PostsFileNameAndConfig()
    {
        var (client, handler) = Create();
        handler.Respond("""{"name": "fileSearchStores/my-store/operations/imp-1"}""");

        var operation = await client.ImportFileAsync("my-store", "abc-123", new ImportFileConfig
        {
            CustomMetadata = new List<CustomMetadata>
            {
                new("tags") { StringListValue = new StringList { Values = new[] { "a", "b" } } }
            },
            ChunkingConfig = new ChunkingConfig { WhiteSpaceConfig = new WhiteSpaceConfig { MaxTokensPerChunk = 100 } }
        }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores/my-store:importFile");
        var body = request.Json.AsObject();
        body.Count.ShouldBe(3);
        body["fileName"]!.GetValue<string>().ShouldBe("files/abc-123");
        body["customMetadata"]![0]!["key"]!.GetValue<string>().ShouldBe("tags");
        body["customMetadata"]![0]!["stringListValue"]!["values"]![1]!.GetValue<string>().ShouldBe("b");
        body["chunkingConfig"]!["whiteSpaceConfig"]!["maxTokensPerChunk"]!.GetValue<int>().ShouldBe(100);
        body["chunkingConfig"]!["whiteSpaceConfig"]!.AsObject().ContainsKey("maxOverlapTokens").ShouldBeFalse();

        operation.Name.ShouldBe("fileSearchStores/my-store/operations/imp-1");
        operation.Done.ShouldBeNull();
    }

    [Fact]
    public async Task ImportFile_WithoutConfig_SendsOnlyFileName()
    {
        var (client, handler) = Create();
        handler.Respond("""{"name": "fileSearchStores/my-store/operations/imp-1"}""");

        await client.ImportFileAsync("fileSearchStores/my-store", "files/abc-123",
            cancellationToken: TestContext.Current.CancellationToken);

        var body = handler.Requests.ShouldHaveSingleItem().Json.AsObject();
        body.Count.ShouldBe(1);
        body["fileName"]!.GetValue<string>().ShouldBe("files/abc-123");
    }

    [Fact]
    public async Task GetOperation_GetsOperationByName()
    {
        var (client, handler) = Create();
        handler.Respond(DoneUploadOperationJson);

        var operation = await client.GetOperationAsync("fileSearchStores/my-store/upload/operations/op-1",
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores/my-store/upload/operations/op-1");
        operation.Response!.DocumentName.ShouldBe("fileSearchStores/my-store/documents/doc-1");
        operation.Response.Type.ShouldEndWith("UploadToFileSearchStoreResponse");
    }

    [Fact]
    public async Task WaitForOperation_PollsUntilDone()
    {
        var (client, handler) = Create();
        handler.Respond(PendingUploadOperationJson).Respond(DoneUploadOperationJson);

        var initial = new FileSearchOperation { Name = "fileSearchStores/my-store/upload/operations/op-1" };
        var done = await client.WaitForOperationAsync(initial, TimeSpan.FromMilliseconds(1), TimeSpan.FromMinutes(1),
            TestContext.Current.CancellationToken);

        handler.Requests.Count.ShouldBe(2);
        handler.Requests.ShouldAllBe(r => r.Method == HttpMethod.Get &&
                                          r.Url.AbsoluteUri == $"{BaseUrl}/fileSearchStores/my-store/upload/operations/op-1");
        done.Done.ShouldBe(true);
        done.Response!.DocumentName.ShouldBe("fileSearchStores/my-store/documents/doc-1");
    }

    [Fact]
    public async Task WaitForOperation_ReturnsImmediatelyWhenAlreadyDone()
    {
        var (client, handler) = Create();
        var done = new FileSearchOperation { Name = "fileSearchStores/s/operations/o", Done = true };

        var result = await client.WaitForOperationAsync(done, cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(done);
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task WaitForOperation_ThrowsOnOperationError()
    {
        var (client, handler) = Create();
        handler.Respond("""
            {"name": "fileSearchStores/s/operations/o", "done": true,
             "error": {"code": 3, "message": "Unsupported file type"}}
            """);

        var ex = await Should.ThrowAsync<GenerativeAIException>(() => client.WaitForOperationAsync(
            new FileSearchOperation { Name = "fileSearchStores/s/operations/o" }, TimeSpan.FromMilliseconds(1),
            cancellationToken: TestContext.Current.CancellationToken));
        ex.Details.ShouldBe("Unsupported file type");
    }

    [Fact]
    public async Task WaitForOperation_TimesOut()
    {
        var (client, handler) = Create();
        for (var i = 0; i < 10; i++)
            handler.Respond(PendingUploadOperationJson);

        await Should.ThrowAsync<TimeoutException>(() => client.WaitForOperationAsync(
            new FileSearchOperation { Name = "fileSearchStores/my-store/upload/operations/op-1" },
            TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken));
    }

    #endregion

    #region Documents

    [Fact]
    public async Task GetDocument_GetsByFullNameAndParsesDocument()
    {
        var (client, handler) = Create();
        handler.Respond(DocumentJson);

        var document = await client.GetDocumentAsync("fileSearchStores/my-store/documents/doc-1",
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores/my-store/documents/doc-1");

        document.Name.ShouldBe("fileSearchStores/my-store/documents/doc-1");
        document.DisplayName.ShouldBe("Doc One");
        document.State.ShouldBe(FileSearchDocumentState.STATE_ACTIVE);
        document.SizeBytes.ShouldBe(1234);
        document.MimeType.ShouldBe("application/pdf");
        document.CreateTime.ShouldNotBeNull();
        document.UpdateTime.ShouldNotBeNull();
        document.CustomMetadata!.Count.ShouldBe(3);
        document.CustomMetadata[0].StringValue.ShouldBe("Robert Graves");
        document.CustomMetadata[1].NumericValue.ShouldBe(1934);
        document.CustomMetadata[2].StringListValue!.Values.ShouldBe(new[] { "history", "rome" });
    }

    [Fact]
    public async Task GetDocument_RequiresFullDocumentName()
    {
        var (client, _) = Create();
        await Should.ThrowAsync<ArgumentException>(() =>
            client.GetDocumentAsync("doc-1", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListDocuments_SendsPagingQueryAndParsesResponse()
    {
        var (client, handler) = Create();
        handler.Respond($$"""{"documents": [{{DocumentJson}}, {"name": "fileSearchStores/my-store/documents/doc-2", "state": "STATE_PENDING"}], "nextPageToken": "n2"}""");

        var page = await client.ListDocumentsAsync("my-store", 20, "t1", TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.GetLeftPart(UriPartial.Path).ShouldBe($"{BaseUrl}/fileSearchStores/my-store/documents");
        request.Url.Query.ShouldBe("?pageSize=20&pageToken=t1");
        page.Documents!.Count.ShouldBe(2);
        page.Documents[1].State.ShouldBe(FileSearchDocumentState.STATE_PENDING);
        page.NextPageToken.ShouldBe("n2");
    }

    [Fact]
    public async Task ListAllDocuments_FollowsNextPageToken()
    {
        var (client, handler) = Create();
        handler.Respond("""{"documents": [{"name": "fileSearchStores/s/documents/a"}], "nextPageToken": "p2"}""")
            .Respond("""{"documents": [{"name": "fileSearchStores/s/documents/b"}]}""");

        var names = new List<string?>();
        await foreach (var doc in client.ListAllDocumentsAsync("s", cancellationToken: TestContext.Current.CancellationToken))
            names.Add(doc.Name);

        names.ShouldBe(new[] { "fileSearchStores/s/documents/a", "fileSearchStores/s/documents/b" });
        handler.Requests[0].Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores/s/documents");
        handler.Requests[1].Url.Query.ShouldBe("?pageToken=p2");
    }

    [Theory]
    [InlineData(true, "?force=true")]
    [InlineData(null, "")]
    public async Task DeleteDocument_SendsDeleteWithForce(bool? force, string expectedQuery)
    {
        var (client, handler) = Create();

        await client.DeleteDocumentAsync("fileSearchStores/my-store/documents/doc-1", force,
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Delete);
        request.Url.GetLeftPart(UriPartial.Path).ShouldBe($"{BaseUrl}/fileSearchStores/my-store/documents/doc-1");
        request.Url.Query.ShouldBe(expectedQuery);
    }

    [Fact]
    public async Task DownloadMedia_GetsAltMedia()
    {
        var (client, handler) = Create();
        handler.Respond("raw-bytes");

        var bytes = await client.DownloadMediaAsync("fileSearchStores/my-store/media/blob-1",
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe($"{BaseUrl}/fileSearchStores/my-store/media/blob-1?alt=media");
        Encoding.UTF8.GetString(bytes).ShouldBe("raw-bytes");
    }

    #endregion

    #region Platform, serialization, model integration

    [Fact]
    public async Task VertexPlatform_IsNotSupported()
    {
        var handler = new CapturingHandler();
        var client = new FileSearchStoresClient(
            new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token", validateAccessToken: false),
            new HttpClient(handler));

        await Should.ThrowAsync<NotSupportedException>(() =>
            client.ListFileSearchStoresAsync(cancellationToken: TestContext.Current.CancellationToken));
        await Should.ThrowAsync<NotSupportedException>(() =>
            client.CreateFileSearchStoreAsync("x", cancellationToken: TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public void SerializerContext_ProvidesMetadataForFileSearchTypes()
    {
        var context = FileSearchSerializerContext.Default;
        foreach (var type in new[]
                 {
                     typeof(FileSearchStore), typeof(FileSearchDocument), typeof(FileSearchDocumentState),
                     typeof(ChunkingConfig), typeof(WhiteSpaceConfig), typeof(UploadToFileSearchStoreConfig),
                     typeof(ImportFileConfig), typeof(ImportFileRequest), typeof(ListFileSearchStoresResponse),
                     typeof(ListFileSearchDocumentsResponse), typeof(FileSearchOperation),
                     typeof(FileSearchOperationResponse)
                 })
        {
            context.GetTypeInfo(type).ShouldNotBeNull(type.Name);
        }

        var store = JsonSerializer.Deserialize(StoreJson, FileSearchSerializerContext.Default.FileSearchStore)!;
        store.SizeBytes.ShouldBe(4096);
        var json = JsonSerializer.Serialize(new FileSearchStore { DisplayName = "x" },
            FileSearchSerializerContext.Default.FileSearchStore);
        JsonNode.Parse(json)!.AsObject().Select(p => p.Key).ShouldBe(new[] { "displayName" });
    }

    [Fact]
    public void GoogleAi_CreatesFileSearchStoresClient()
    {
        var googleAi = new GoogleAi("test-api-key");
        googleAi.CreateFileSearchStoresClient().ShouldNotBeNull();
    }

    [Fact]
    public async Task GenerativeModel_UseFileSearch_AddsFileSearchTool()
    {
        var handler = new CapturingHandler();
        handler.Respond("""{"candidates": [{"content": {"role": "model", "parts": [{"text": "hi"}]}}]}""");
        var model = new GenerativeModel(new GoogleAIPlatformAdapter("test-api-key"), GoogleAIModels.Gemini38Flash,
            httpClient: new HttpClient(handler));

        model.UseFileSearch(new[] { "my-store", "fileSearchStores/other" }, topK: 5, metadataFilter: "author = \"x\"");
        await model.GenerateContentAsync("hello", TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        var fileSearch = request.Json["tools"]!.AsArray()
            .Select(t => t!["fileSearch"]).Where(t => t != null).ShouldHaveSingleItem()!;
        fileSearch["fileSearchStoreNames"]!.AsArray().Select(n => n!.GetValue<string>())
            .ShouldBe(new[] { "fileSearchStores/my-store", "fileSearchStores/other" });
        fileSearch["topK"]!.GetValue<int>().ShouldBe(5);
        fileSearch["metadataFilter"]!.GetValue<string>().ShouldBe("author = \"x\"");
    }

    [Fact]
    public void GenerativeModel_UseFileSearch_RejectsVertex()
    {
        var model = new GenerativeModel(
            new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token", validateAccessToken: false),
            GoogleAIModels.Gemini38Flash);

        Should.Throw<NotSupportedException>(() => model.UseFileSearch("my-store"));
    }

    #endregion
}
