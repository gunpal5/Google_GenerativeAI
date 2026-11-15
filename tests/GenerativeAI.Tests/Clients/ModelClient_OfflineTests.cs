using System.Text;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Offline tests for the <see cref="ModelClient"/> update, delete and computeTokens methods.
/// </summary>
public class ModelClient_OfflineTests
{
    private const string VertexRoot = "https://us-central1-aiplatform.googleapis.com/v1beta1";
    private const string VertexLocation = VertexRoot + "/projects/p/locations/us-central1";
    private const string GeminiBase = "https://generativelanguage.googleapis.com/v1beta";

    private static (ModelClient Client, RecordingHttpHandler Handler) Vertex()
    {
        var handler = new RecordingHttpHandler();
        var platform = new VertextPlatformAdapter("p", "us-central1", accessToken: "t", validateAccessToken: false);
        return (new ModelClient(platform, new HttpClient(handler)), handler);
    }

    private static (ModelClient Client, RecordingHttpHandler Handler) Gemini()
    {
        var handler = new RecordingHttpHandler();
        return (new ModelClient(new GoogleAIPlatformAdapter("key"), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task Gemini_UpdateModel_PatchesTunedModel()
    {
        var (client, handler) = Gemini();
        handler.Reply("""{"name": "tunedModels/abc", "displayName": "new name", "description": "new desc", "baseModel": "models/gemini-1.5-flash-001-tuning"}""");

        var model = await client.UpdateModelAsync("tunedModels/abc",
            new UpdateModelConfig { DisplayName = "new name", Description = "new desc" },
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.Method.ShouldBe("PATCH");
        request.Url.AbsoluteUri.ShouldBe($"{GeminiBase}/tunedModels/abc");
        request.Body!["displayName"]!.GetValue<string>().ShouldBe("new name");
        request.Body["description"]!.GetValue<string>().ShouldBe("new desc");
        request.Body["defaultCheckpointId"].ShouldBeNull();
        model.Name.ShouldBe("tunedModels/abc");
        model.DisplayName.ShouldBe("new name");
    }

    [Fact]
    public async Task Vertex_UpdateModel_PatchesModelResource()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"name": "projects/p/locations/us-central1/models/123", "displayName": "n", "defaultCheckpointId": "2"}""");

        var model = await client.UpdateModelAsync("models/123", new UpdateModelConfig { DefaultCheckpointId = "2" },
            TestContext.Current.CancellationToken);
        await client.UpdateModelAsync("projects/p/locations/us-central1/models/123", new UpdateModelConfig { DisplayName = "n" },
            TestContext.Current.CancellationToken);

        handler.Requests[0].Method.Method.ShouldBe("PATCH");
        handler.Requests[0].Url.AbsoluteUri.ShouldBe($"{VertexLocation}/models/123");
        handler.Requests[0].Body!["defaultCheckpointId"]!.GetValue<string>().ShouldBe("2");
        handler.Requests[0].Headers["Authorization"].ShouldBe("Bearer t");
        handler.Requests[1].Url.AbsoluteUri.ShouldBe($"{VertexLocation}/models/123");
        handler.Requests[1].Body!["displayName"]!.GetValue<string>().ShouldBe("n");
        model.DefaultCheckpointId.ShouldBe("2");
    }

    [Fact]
    public async Task DeleteModel_SendsDeleteOnBothPlatforms()
    {
        var (gemini, geminiHandler) = Gemini();
        await gemini.DeleteModelAsync("tunedModels/abc", TestContext.Current.CancellationToken);
        var geminiRequest = geminiHandler.Requests.ShouldHaveSingleItem();
        geminiRequest.Method.ShouldBe(HttpMethod.Delete);
        geminiRequest.Url.AbsoluteUri.ShouldBe($"{GeminiBase}/tunedModels/abc");

        var (vertex, vertexHandler) = Vertex();
        vertexHandler.Reply("""{"name": "projects/p/locations/us-central1/operations/1"}""");
        await vertex.DeleteModelAsync("projects/p/locations/us-central1/models/123", TestContext.Current.CancellationToken);
        var vertexRequest = vertexHandler.Requests.ShouldHaveSingleItem();
        vertexRequest.Method.ShouldBe(HttpMethod.Delete);
        vertexRequest.Url.AbsoluteUri.ShouldBe($"{VertexLocation}/models/123");
    }

    [Fact]
    public async Task Vertex_ComputeTokens_PostsContentsAndParsesTokens()
    {
        var (client, handler) = Vertex();
        var hello = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello"));
        handler.Reply($$"""{"tokensInfo": [{"role": "user", "tokenIds": ["9259", "1"], "tokens": ["{{hello}}", "IQ=="]}]}""");

        var response = await client.ComputeTokensAsync("gemini-2.5-flash", new[] { new Content("Hello!", Roles.User) },
            TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe($"{VertexLocation}/publishers/google/models/gemini-2.5-flash:computeTokens");
        request.Body!["contents"]![0]!["parts"]![0]!["text"]!.GetValue<string>().ShouldBe("Hello!");
        var info = response.TokensInfo.ShouldHaveSingleItem();
        info.Role.ShouldBe("user");
        info.TokenIds.ShouldBe(new long[] { 9259, 1 });
        Encoding.UTF8.GetString(info.Tokens![0]).ShouldBe("Hello");
        Encoding.UTF8.GetString(info.Tokens[1]).ShouldBe("!");
    }

    [Fact]
    public async Task Vertex_ComputeTokens_ResolvesModelNamesLikeGoogleGenAI()
    {
        var (client, handler) = Vertex();
        handler.Reply("""{"tokensInfo": []}""");
        var contents = new[] { new Content("x", Roles.User) };

        await client.ComputeTokensAsync("meta/llama-3", contents, TestContext.Current.CancellationToken);
        await client.ComputeTokensAsync("publishers/google/models/gemini-2.5-pro", contents, TestContext.Current.CancellationToken);
        await client.ComputeTokensAsync("projects/p/locations/us-central1/endpoints/42", contents, TestContext.Current.CancellationToken);

        handler.Requests[0].Url.AbsoluteUri.ShouldBe($"{VertexLocation}/publishers/meta/models/llama-3:computeTokens");
        handler.Requests[1].Url.AbsoluteUri.ShouldBe($"{VertexLocation}/publishers/google/models/gemini-2.5-pro:computeTokens");
        handler.Requests[2].Url.AbsoluteUri.ShouldBe($"{VertexLocation}/endpoints/42:computeTokens");
    }

    [Fact]
    public async Task Gemini_ComputeTokens_IsNotSupported()
    {
        var (client, handler) = Gemini();
        await Should.ThrowAsync<NotSupportedException>(() => client.ComputeTokensAsync("gemini-2.5-flash",
            new[] { new Content("x", Roles.User) }, TestContext.Current.CancellationToken));
        await Should.ThrowAsync<ArgumentException>(() => client.UpdateModelAsync("models/../x", new UpdateModelConfig(),
            TestContext.Current.CancellationToken));
        handler.Requests.ShouldBeEmpty();
    }
}
