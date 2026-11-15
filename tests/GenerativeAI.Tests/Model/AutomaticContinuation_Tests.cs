using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Model;

/// <summary>
/// Automatic continuation: when the API ends a response early with finishReason CONTINUATION and a continuation
/// token, the SDK re-sends the request with that token and merges (or keeps streaming) the result, like google-genai.
/// </summary>
public class AutomaticContinuation_Tests
{
    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Queue<string> _responses;

        public ScriptedHandler(params string[] responses) => _responses = new Queue<string>(responses);

        public List<JsonNode> RequestBodies { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBodies.Add(JsonNode.Parse(await request.Content!.ReadAsStringAsync().ConfigureAwait(false))!);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responses.Dequeue(), Encoding.UTF8, "application/json")
            };
        }
    }

    private static GenerativeModel CreateModel(ScriptedHandler handler) =>
        new(new GoogleAIPlatformAdapter("test-key"), GoogleAIModels.Gemini38Flash, httpClient: new HttpClient(handler));

    private static string Chunk(string text, string? finishReason, string? token, int candidates = 5, int total = 15)
    {
        var finish = finishReason == null ? "" : ",\"finishReason\":\"" + finishReason + "\"";
        var continuation = token == null ? "" : ",\"continuationToken\":\"" + token + "\"";
        return "{\"candidates\":[{\"index\":0,\"content\":{\"role\":\"model\",\"parts\":[{\"text\":\"" + text + "\"}]}"
               + finish + continuation + "}],"
               + "\"usageMetadata\":{\"promptTokenCount\":10,\"candidatesTokenCount\":" + candidates
               + ",\"totalTokenCount\":" + total + "}}";
    }

    [Fact]
    public async Task GenerateContent_FollowsContinuationTokenAndMergesResponses()
    {
        var handler = new ScriptedHandler(Chunk("Hello ", "CONTINUATION", "dG9rLTE="), Chunk("world", "STOP", null));
        var model = CreateModel(handler);
        var request = new GenerateContentRequest();
        request.AddText("Say hello");

        var response = await model.GenerateContentAsync(request, TestContext.Current.CancellationToken);

        handler.RequestBodies.Count.ShouldBe(2);
        handler.RequestBodies[0]["continuationToken"].ShouldBeNull();
        handler.RequestBodies[1]["continuationToken"]!.GetValue<string>().ShouldBe("dG9rLTE=");
        response.Candidates![0].Content!.Parts.ShouldHaveSingleItem().Text.ShouldBe("Hello world");
        response.Candidates![0].FinishReason.ShouldBe(FinishReason.STOP);
        response.Candidates[0].ContinuationToken.ShouldBeNull();
        response.UsageMetadata!.CandidatesTokenCount.ShouldBe(10);
        response.UsageMetadata.TotalTokenCount.ShouldBe(30);
        request.ContinuationToken.ShouldBeNull(); // the caller's request is restored
    }

    [Fact]
    public async Task GenerateContent_WhenDisabled_ReturnsFirstResponse()
    {
        var handler = new ScriptedHandler(Chunk("Hello ", "CONTINUATION", "dG9rLTE="));
        var model = CreateModel(handler);
        model.EnableAutomaticContinuation = false;

        var response = await model.GenerateContentAsync("Say hello", TestContext.Current.CancellationToken);

        handler.RequestBodies.Count.ShouldBe(1);
        response.Candidates![0].FinishReason.ShouldBe(FinishReason.CONTINUATION);
        response.Candidates[0].ContinuationToken.ShouldBe("dG9rLTE=");
    }

    [Fact]
    public async Task GenerateContent_StopsAtMaxContinuations()
    {
        var handler = new ScriptedHandler(
            Chunk("a", "CONTINUATION", "dDE="), Chunk("b", "CONTINUATION", "dDI="), Chunk("c", "CONTINUATION", "dDM="));
        var model = CreateModel(handler);
        model.MaxContinuations = 2;

        var response = await model.GenerateContentAsync("go", TestContext.Current.CancellationToken);

        handler.RequestBodies.Count.ShouldBe(3);
        response.Candidates![0].Content!.Parts.ShouldHaveSingleItem().Text.ShouldBe("abc");
    }

    [Fact]
    public async Task GenerateContent_TokenWithFinalFinishReason_IsNotFollowed()
    {
        var handler = new ScriptedHandler(Chunk("done", "STOP", "dG9r"));
        var model = CreateModel(handler);

        await model.GenerateContentAsync("go", TestContext.Current.CancellationToken);

        handler.RequestBodies.Count.ShouldBe(1);
    }

    [Fact]
    public async Task StreamContent_StartsNewStreamWhenHopEndsWithContinuation()
    {
        var handler = new ScriptedHandler(
            $"[{Chunk("Hel", null, null)},{Chunk("lo ", "CONTINUATION", "dG9rLTE=")}]",
            $"[{Chunk("world", "STOP", null)}]");
        var model = CreateModel(handler);

        var texts = new List<string>();
        await foreach (var chunk in model.StreamContentAsync("Say hello", TestContext.Current.CancellationToken))
            texts.Add(chunk.Candidates![0].Content!.Parts[0].Text!);

        texts.ShouldBe(new[] { "Hel", "lo ", "world" });
        handler.RequestBodies.Count.ShouldBe(2);
        handler.RequestBodies[1]["continuationToken"]!.GetValue<string>().ShouldBe("dG9rLTE=");
    }

    [Fact]
    public async Task StreamContent_ResumesAfterCutOffStreamUsingLastCheckpoint()
    {
        // First stream is cut off mid-array after a checkpoint chunk (no finish reason, but a token).
        var handler = new ScriptedHandler(
            $"[{Chunk("Hel", null, "Y2twdA==")},{{\"candidates\":[",
            $"[{Chunk("lo", "STOP", null)}]");
        var model = CreateModel(handler);

        var texts = new List<string>();
        await foreach (var chunk in model.StreamContentAsync("Say hello", TestContext.Current.CancellationToken))
            texts.Add(chunk.Candidates![0].Content!.Parts[0].Text!);

        texts.ShouldBe(new[] { "Hel", "lo" });
        handler.RequestBodies[1]["continuationToken"]!.GetValue<string>().ShouldBe("Y2twdA==");
    }

    [Fact]
    public async Task StreamContent_CutOffWithoutCheckpoint_Throws()
    {
        var handler = new ScriptedHandler($"[{Chunk("Hel", null, null)},{{\"candidates\":[");
        var model = CreateModel(handler);

        await Should.ThrowAsync<Exception>(async () =>
        {
            await foreach (var _ in model.StreamContentAsync("Say hello", TestContext.Current.CancellationToken))
            {
            }
        });
        handler.RequestBodies.Count.ShouldBe(1);
    }
}
