using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Model;

/// <summary>
/// Checks that embeddings use the request formats Vertex AI expects (matching google-genai):
/// Gemini embedding models call embedContent/batchEmbedContents with options in embedContentConfig;
/// other text embedding models call predict with instances and parameters.
/// </summary>
public class VertexEmbedding_Tests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly string _responseJson;

        public CapturingHandler(string responseJson) => _responseJson = responseJson;

        public List<(Uri Url, JsonNode Body)> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content == null ? "{}" : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            Requests.Add((request.RequestUri!, JsonNode.Parse(body)!));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            };
        }
    }

    private static (EmbeddingModel Model, CapturingHandler Handler) CreateVertex(string model, string responseJson)
    {
        var handler = new CapturingHandler(responseJson);
        var platform = new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token",
            validateAccessToken: false);
        return (new EmbeddingModel(platform, model, new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task GeminiModel_EmbedContent_NestsOptionsInEmbedContentConfig()
    {
        var (model, handler) = CreateVertex(VertexAIModels.Embeddings.GeminiEmbedding001,
            """{"embedding": {"values": [0.1, 0.2]}, "usageMetadata": {"promptTokenCount": 7}, "truncated": true}""");

        var response = await model.EmbedContentAsync(new EmbedContentRequest
        {
            Content = new Content("hello", Roles.User),
            TaskType = TaskType.RETRIEVAL_DOCUMENT,
            Title = "Doc",
            OutputDimensionality = 256,
            AutoTruncate = false,
            DocumentOcr = true,
            AudioTrackExtraction = true
        }, TestContext.Current.CancellationToken);

        var (url, body) = handler.Requests.ShouldHaveSingleItem();
        url.AbsoluteUri.ShouldEndWith("gemini-embedding-001:embedContent");
        body["content"]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("hello");
        body["taskType"].ShouldBeNull();
        var config = body["embedContentConfig"]!;
        config["taskType"]!.GetValue<string>().ShouldBe("RETRIEVAL_DOCUMENT");
        config["title"]!.GetValue<string>().ShouldBe("Doc");
        config["outputDimensionality"]!.GetValue<int>().ShouldBe(256);
        config["autoTruncate"]!.GetValue<bool>().ShouldBeFalse();
        config["documentOcr"]!.GetValue<bool>().ShouldBeTrue();
        config["audioTrackExtraction"]!.GetValue<bool>().ShouldBeTrue();

        response.Embedding!.Values.ShouldBe(new[] { 0.1f, 0.2f });
        response.Embedding.Statistics!.TokenCount.ShouldBe(7);
        response.Embedding.Statistics.Truncated.ShouldBe(true);
    }

    [Fact]
    public async Task GeminiModel_BatchEmbed_UsesRequestsWithEmbedContentConfig()
    {
        var (model, handler) = CreateVertex(VertexAIModels.Embeddings.GeminiEmbedding001,
            """{"embeddings": [{"values": [1]}, {"values": [2]}]}""");

        var response = await model.BatchEmbedContentAsync(new[]
        {
            new EmbedContentRequest { Content = new Content("a", Roles.User), TaskType = TaskType.SEMANTIC_SIMILARITY },
            new EmbedContentRequest { Content = new Content("b", Roles.User), TaskType = TaskType.SEMANTIC_SIMILARITY }
        }, TestContext.Current.CancellationToken);

        var (url, body) = handler.Requests.ShouldHaveSingleItem();
        url.AbsoluteUri.ShouldEndWith("gemini-embedding-001:batchEmbedContents");
        var requests = body["requests"]!.AsArray();
        requests.Count.ShouldBe(2);
        requests[0]!["model"].ShouldBeNull();
        requests[1]!["embedContentConfig"]!["taskType"]!.GetValue<string>().ShouldBe("SEMANTIC_SIMILARITY");
        response.Embeddings!.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GeminiModel_BatchEmbed_SendsAllConfigFieldsAndParsesUsageMetadata()
    {
        var (model, handler) = CreateVertex("gemini-embedding-2",
            """
            {"embeddings": [{"values": [0.1, 0.2], "statistics": {"tokenCount": 3, "truncated": false}}, {"values": [0.3]}],
             "usageMetadata": {"promptTokenCount": 9, "promptTokensDetails": [{"modality": "TEXT", "tokenCount": 9}]}}
            """);

        var response = await model.BatchEmbedContentAsync(new[]
        {
            new EmbedContentRequest
            {
                Content = new Content("first", Roles.User),
                TaskType = TaskType.RETRIEVAL_DOCUMENT,
                Title = "Doc",
                OutputDimensionality = 64,
                AutoTruncate = true,
                DocumentOcr = true,
                AudioTrackExtraction = false
            },
            new EmbedContentRequest { Content = new Content("second", Roles.User) }
        }, TestContext.Current.CancellationToken);

        var (url, body) = handler.Requests.ShouldHaveSingleItem();
        url.AbsoluteUri.ShouldEndWith("/gemini-embedding-2:batchEmbedContents");
        var requests = body["requests"]!.AsArray();
        requests.Count.ShouldBe(2);
        requests[0]!["content"]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("first");
        requests[1]!["content"]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("second");
        var config = requests[0]!["embedContentConfig"]!;
        config["taskType"]!.GetValue<string>().ShouldBe("RETRIEVAL_DOCUMENT");
        config["title"]!.GetValue<string>().ShouldBe("Doc");
        config["outputDimensionality"]!.GetValue<int>().ShouldBe(64);
        config["autoTruncate"]!.GetValue<bool>().ShouldBeTrue();
        config["documentOcr"]!.GetValue<bool>().ShouldBeTrue();
        config["audioTrackExtraction"]!.GetValue<bool>().ShouldBeFalse();
        requests[0]!["taskType"].ShouldBeNull();

        response.Embeddings!.Count.ShouldBe(2);
        response.Embeddings[0].Values.ShouldBe(new[] { 0.1f, 0.2f });
        response.Embeddings[0].Statistics!.TokenCount.ShouldBe(3);
        response.UsageMetadata!.PromptTokenCount.ShouldBe(9);
        response.UsageMetadata.PromptTokensDetails!.ShouldHaveSingleItem().TokenCount.ShouldBe(9);
    }

    [Fact]
    public async Task TextEmbeddingModel_UsesPredictWithInstancesAndParameters()
    {
        var (model, handler) = CreateVertex(VertexAIModels.Embeddings.TextMultilingualEmbedding002,
            """{"predictions": [{"embeddings": {"values": [0.5], "statistics": {"token_count": 3, "truncated": false}}}]}""");

        var response = await model.EmbedContentAsync(new EmbedContentRequest
        {
            Content = new Content("hello", Roles.User),
            TaskType = TaskType.RETRIEVAL_DOCUMENT,
            Title = "Doc",
            OutputDimensionality = 128,
            AutoTruncate = true
        }, TestContext.Current.CancellationToken);

        var (url, body) = handler.Requests.ShouldHaveSingleItem();
        url.AbsoluteUri.ShouldEndWith("text-multilingual-embedding-002:predict");
        var instance = body["instances"]![0]!;
        instance["content"]!.GetValue<string>().ShouldBe("hello");
        instance["task_type"]!.GetValue<string>().ShouldBe("RETRIEVAL_DOCUMENT");
        instance["title"]!.GetValue<string>().ShouldBe("Doc");
        body["parameters"]!["outputDimensionality"]!.GetValue<int>().ShouldBe(128);
        body["parameters"]!["autoTruncate"]!.GetValue<bool>().ShouldBeTrue();

        response.Embedding!.Values.ShouldBe(new[] { 0.5f });
        response.Embedding.Statistics!.TokenCount.ShouldBe(3);
    }

    [Fact]
    public async Task TextEmbeddingModel_Batch_SendsOneInstancePerRequest()
    {
        var (model, handler) = CreateVertex(VertexAIModels.Embeddings.TextMultilingualEmbedding002,
            """{"predictions": [{"embeddings": {"values": [1]}}, {"embeddings": {"values": [2]}}]}""");

        var response = await model.BatchEmbedContentAsync(new[]
        {
            new EmbedContentRequest { Content = new Content("a", Roles.User) },
            new EmbedContentRequest { Content = new Content("b", Roles.User) }
        }, TestContext.Current.CancellationToken);

        handler.Requests.ShouldHaveSingleItem().Body["instances"]!.AsArray().Count.ShouldBe(2);
        response.Embeddings!.Select(e => e.Values![0]).ShouldBe(new[] { 1f, 2f });
    }

    [Fact]
    public async Task TextEmbeddingModel_Batch_RejectsMixedParameters()
    {
        var (model, _) = CreateVertex(VertexAIModels.Embeddings.TextMultilingualEmbedding002, "{}");

        await Should.ThrowAsync<NotSupportedException>(() => model.BatchEmbedContentAsync(new[]
        {
            new EmbedContentRequest { Content = new Content("a", Roles.User), OutputDimensionality = 64 },
            new EmbedContentRequest { Content = new Content("b", Roles.User), OutputDimensionality = 128 }
        }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GeminiApi_RejectsVertexOnlyOptions()
    {
        var handler = new CapturingHandler("{}");
        var model = new EmbeddingModel(new GoogleAIPlatformAdapter("test-key"), GoogleAIModels.GeminiEmbedding,
            new HttpClient(handler));

        await Should.ThrowAsync<NotSupportedException>(() => model.EmbedContentAsync(new EmbedContentRequest
        {
            Content = new Content("hello", Roles.User),
            AutoTruncate = true
        }, TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task GeminiApi_KeepsFlatRequestFormat()
    {
        var handler = new CapturingHandler("""{"embedding": {"values": [1]}}""");
        var model = new EmbeddingModel(new GoogleAIPlatformAdapter("test-key"), GoogleAIModels.GeminiEmbedding,
            new HttpClient(handler));

        await model.EmbedContentAsync(new EmbedContentRequest
        {
            Content = new Content("hello", Roles.User),
            TaskType = TaskType.CLASSIFICATION
        }, TestContext.Current.CancellationToken);

        var (url, body) = handler.Requests.ShouldHaveSingleItem();
        url.AbsoluteUri.ShouldEndWith("gemini-embedding-001:embedContent");
        body["taskType"]!.GetValue<string>().ShouldBe("CLASSIFICATION");
        body["embedContentConfig"].ShouldBeNull();
    }
}
