using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Live;
using GenerativeAI.Tests.Live;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Tests for ephemeral Live API auth tokens: token creation (request format and field-mask rules,
/// matching google-genai) and connecting the Live client with a token.
/// </summary>
public class AuthTokensClient_Tests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Uri? Url { get; private set; }
        public JsonNode? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Url = request.RequestUri;
            Body = JsonNode.Parse(await request.Content!.ReadAsStringAsync().ConfigureAwait(false));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"name":"auth_tokens/abc123","expireTime":"2026-11-15T12:30:00Z","uses":1}""",
                    Encoding.UTF8, "application/json")
            };
        }
    }

    private static readonly BidiGenerateContentSetup Constraints = new()
    {
        Model = "models/gemini-3.8-live",
        GenerationConfig = new GenerationConfig { Temperature = 0.7, ResponseModalities = new List<Modality> { Modality.AUDIO } },
        SystemInstruction = new Content("Be brief.", Roles.System)
    };

    [Fact]
    public async Task CreateAsync_PostsToV1AlphaAndReturnsToken()
    {
        var handler = new CapturingHandler();
        var client = new AuthTokensClient(new GoogleAIPlatformAdapter("real-key"), new HttpClient(handler));

        var expire = new DateTime(2026, 11, 15, 12, 30, 0, DateTimeKind.Utc);
        var token = await client.CreateAsync(new CreateAuthTokenConfig { Uses = 1, ExpireTime = expire },
            TestContext.Current.CancellationToken);

        handler.Url!.AbsolutePath.ShouldBe("/v1alpha/auth_tokens");
        handler.Body!["uses"]!.GetValue<int>().ShouldBe(1);
        handler.Body["expireTime"].ShouldNotBeNull();
        handler.Body["bidiGenerateContentSetup"].ShouldBeNull();
        handler.Body["fieldMask"].ShouldBeNull();
        token.Name.ShouldBe("auth_tokens/abc123");
        token.Uses.ShouldBe(1);
    }

    [Fact]
    public void Constraints_WithoutLockFields_LockEverything()
    {
        var request = AuthTokensClient.BuildRequest(new CreateAuthTokenConfig { LiveConnectConstraints = Constraints });

        request.BidiGenerateContentSetup.ShouldBeSameAs(Constraints);
        request.FieldMask.ShouldBeNull();
    }

    [Fact]
    public void Constraints_WithEmptyLockFields_LockOnlyTheSetFields()
    {
        var request = AuthTokensClient.BuildRequest(new CreateAuthTokenConfig
        {
            LiveConnectConstraints = Constraints,
            LockAdditionalFields = new List<string>()
        });

        request.FieldMask!.Split(',').ShouldBe(new[]
        {
            "model", "generationConfig.responseModalities", "generationConfig.temperature",
            "systemInstruction.parts", "systemInstruction.role"
        }, ignoreOrder: true);
    }

    [Fact]
    public void Constraints_WithAdditionalLockFields_PrefixGenerationConfigFields()
    {
        var request = AuthTokensClient.BuildRequest(new CreateAuthTokenConfig
        {
            LiveConnectConstraints = new BidiGenerateContentSetup { Model = "models/gemini-3.8-live" },
            LockAdditionalFields = new List<string> { "topK", "tools" }
        });

        request.FieldMask.ShouldBe("model,generationConfig.topK,tools");
    }

    [Fact]
    public void LockFields_WithoutConstraints_AreSentAsIs()
    {
        var request = AuthTokensClient.BuildRequest(new CreateAuthTokenConfig
        {
            LockAdditionalFields = new List<string> { "temperature" }
        });

        request.FieldMask.ShouldBe("generationConfig.temperature");
    }

    [Fact]
    public async Task CreateAsync_OnVertex_Throws()
    {
        var platform = new VertextPlatformAdapter("p", "us-central1", accessToken: "t", validateAccessToken: false);
        var client = new AuthTokensClient(platform, new HttpClient(new CapturingHandler()));

        await Should.ThrowAsync<NotSupportedException>(() =>
            client.CreateAsync(new CreateAuthTokenConfig(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LiveClient_WithEphemeralToken_UsesConstrainedEndpointWithoutKey()
    {
        string? connectedUrl = null;
        var live = new MultiModalLiveClient(new GoogleAIPlatformAdapter("auth_tokens/abc123"), "models/gemini-3.8-live")
        {
            WebsocketClientFactory = url =>
            {
                connectedUrl = url;
                return new FakeWebsocketClient(url);
            }
        };

        using (live)
        {
            await live.ConnectAsync(autoSendSetup: false, TestContext.Current.CancellationToken);
        }

        connectedUrl.ShouldBe(
            "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1alpha.GenerativeService.BidiGenerateContentConstrained");
    }

    [Fact]
    public async Task LiveClient_WithRegularKey_UsesStandardEndpoint()
    {
        string? connectedUrl = null;
        var live = new MultiModalLiveClient(new GoogleAIPlatformAdapter("real-key"), "models/gemini-3.8-live")
        {
            WebsocketClientFactory = url =>
            {
                connectedUrl = url;
                return new FakeWebsocketClient(url);
            }
        };

        using (live)
        {
            await live.ConnectAsync(autoSendSetup: false, TestContext.Current.CancellationToken);
        }

        connectedUrl.ShouldNotBeNull();
        connectedUrl.ShouldContain(".BidiGenerateContent?key=real-key");
    }
}
