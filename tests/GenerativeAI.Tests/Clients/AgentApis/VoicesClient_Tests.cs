using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>
/// Offline tests of <see cref="VoicesClient"/> (wire format follows python-genai's <c>_gaos/voices.py</c>).
/// </summary>
public class VoicesClient_Tests
{
    private const string Base = "https://generativelanguage.googleapis.com/v1beta/voices";

    private const string VoiceJson = """
        {
          "type": "prompted", "id": "v-1", "key": "voices/v-1", "display_name": "Narrator",
          "language_code": "en-US", "region_code": "US", "pitch": "low", "model": "voice-model",
          "prompted": {"input": "A calm narrator", "base_voice": "Kore"},
          "sample_audio": {"data": "AAAA", "mime_type": "audio/wav"},
          "expire_time": "2026-11-20T00:00:00Z",
          "usage": {"total_tokens": 5},
          "new_field": true
        }
        """;

    private static (VoicesClient Client, AgentApiTestHandler Handler) Create(params string[] responses)
    {
        var handler = new AgentApiTestHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new VoicesClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task DesignAsync_PostsPromptedVoice()
    {
        var (client, handler) = Create(VoiceJson);

        var voice = await client.DesignAsync("A calm narrator", "Narrator", store: true,
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(Base);
        var body = JsonNode.Parse(request.Body!)!;
        body["store"]!.GetValue<bool>().ShouldBeTrue();
        body["voice"]!["type"]!.GetValue<string>().ShouldBe("prompted");
        body["voice"]!["display_name"]!.GetValue<string>().ShouldBe("Narrator");
        body["voice"]!["prompted"]!["input"]!.GetValue<string>().ShouldBe("A calm narrator");
        body["voice"]!.AsObject().ContainsKey("replicated").ShouldBeFalse();

        voice.Id.ShouldBe("v-1");
        voice.Key.ShouldBe("voices/v-1");
        voice.Pitch.ShouldBe(VoicePitches.Low);
        voice.LanguageCode.ShouldBe("en-US");
        voice.Prompted!.BaseVoice.ShouldBe("Kore");
        voice.SampleAudio!.MimeType.ShouldBe("audio/wav");
        voice.ExpireTime.ShouldBe("2026-11-20T00:00:00Z");
        voice.Usage!.TotalTokens.ShouldBe(5);
        voice.ExtensionData!.ShouldContainKey("new_field");
    }

    [Fact]
    public async Task RemixAndReplicate_SendExpectedBodies()
    {
        var (client, handler) = Create(VoiceJson, VoiceJson);

        await client.RemixAsync("Kore", "Make it brighter", cancellationToken: TestContext.Current.CancellationToken);
        await client.ReplicateAsync(VoiceAudioData.FromBytes(new byte[] { 1, 2, 3 }, "audio/wav"),
            new VoiceAudioData { Data = "Y29uc2VudA==", MimeType = "audio/mp3" }, "Me",
            cancellationToken: TestContext.Current.CancellationToken);

        var remix = JsonNode.Parse(handler.Requests[0].Body!)!["voice"]!;
        remix["prompted"]!["base_voice"]!.GetValue<string>().ShouldBe("Kore");
        remix["prompted"]!["input"]!.GetValue<string>().ShouldBe("Make it brighter");

        var replicate = JsonNode.Parse(handler.Requests[1].Body!)!;
        replicate.AsObject().ContainsKey("store").ShouldBeFalse();
        replicate["voice"]!["type"]!.GetValue<string>().ShouldBe("replicated");
        replicate["voice"]!["replicated"]!["source_audio"]!["data"]!.GetValue<string>().ShouldBe("AQID");
        replicate["voice"]!["replicated"]!["source_audio"]!["mime_type"]!.GetValue<string>().ShouldBe("audio/wav");
        replicate["voice"]!["replicated"]!["consent_audio"]!["mime_type"]!.GetValue<string>().ShouldBe("audio/mp3");
    }

    [Fact]
    public async Task GetAndDelete_UseVoiceIdPath()
    {
        var (client, handler) = Create(VoiceJson, "{}");

        await client.GetAsync("voices/v-1", TestContext.Current.CancellationToken);
        var deleted = await client.DeleteAsync("v-1", TestContext.Current.CancellationToken);

        handler.Requests[0].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe(Base + "/v-1");
        handler.Requests[1].Method.ShouldBe(HttpMethod.Delete);
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(Base + "/v-1");
        deleted.ShouldNotBeNull();
    }

    [Fact]
    public async Task ListAsync_SendsRepeatedSnakeCaseFilters()
    {
        var (client, handler) = Create("""{"voices":[{"type":"prebuilt","display_name":"Kore"}],"next_page_token":"t2"}""");

        var page = await client.ListAsync(new ListVoicesOptions
        {
            LanguageCode = new[] { "en-US", "fr-FR" },
            Contexts = new[] { "News" },
            Type = new[] { VoiceTypes.Prebuilt },
            RegionCode = new[] { "US" },
            Search = "warm voice"
        }, pageSize: 10, pageToken: "t1", cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests.Single().Url.Query.ShouldBe(
            "?context=News&language_code=en-US&language_code=fr-FR&page_size=10&page_token=t1&region_code=US&search=warm%20voice&type=prebuilt");
        page.Voices!.Single().DisplayName.ShouldBe("Kore");
        page.NextPageToken.ShouldBe("t2");
    }

    [Fact]
    public async Task ListAllAsync_FollowsPageTokens()
    {
        var (client, handler) = Create(
            """{"voices":[{"type":"prompted","id":"a"}],"next_page_token":"p2"}""",
            """{"voices":[{"type":"prebuilt","id":"b"}]}""");

        var ids = new List<string?>();
        await foreach (var voice in client.ListAllAsync(cancellationToken: TestContext.Current.CancellationToken))
            ids.Add(voice.Id);

        ids.ShouldBe(new[] { "a", "b" });
        handler.Requests[1].Url.Query.ShouldBe("?page_token=p2");
    }
}
