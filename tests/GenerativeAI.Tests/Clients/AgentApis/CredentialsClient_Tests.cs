using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>
/// Offline tests of <see cref="CredentialsClient"/> (wire format follows python-genai's <c>_gaos/credentials.py</c>).
/// </summary>
public class CredentialsClient_Tests
{
    private const string Base = "https://generativelanguage.googleapis.com/v1beta/credentials";

    private const string CredentialJson = """
        {"id": "gh", "type": "bearer_token", "status": "active",
         "create_time": "2026-11-01T00:00:00Z", "update_time": "2026-11-02T00:00:00Z"}
        """;

    private static (CredentialsClient Client, AgentApiTestHandler Handler) Create(params string[] responses)
    {
        var handler = new AgentApiTestHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new CredentialsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task CreateAsync_PostsFlatDiscriminatedBodies()
    {
        var (client, handler) = Create(CredentialJson, CredentialJson, CredentialJson);

        var created = await client.CreateAsync(new BearerTokenCredentialConfig
        {
            Id = "gh", Token = "secret", HeaderName = "Authorization", Prefix = "token"
        }, TestContext.Current.CancellationToken);
        await client.CreateAsync(new EnvironmentVariableCredentialConfig
        {
            Id = "api-key", Value = "v",
            InjectionLocation = new List<string> { CredentialInjectionLocations.Header },
            TrustedDomains = new List<string> { "api.example.com" }
        }, TestContext.Current.CancellationToken);
        await client.CreateAsync(new OAuth2CredentialConfig
        {
            Id = "oauth", ClientId = "cid", ClientSecret = "cs", RefreshToken = "rt",
            TokenUrl = "https://oauth2.googleapis.com/token", Scopes = new List<string> { "s1" }
        }, TestContext.Current.CancellationToken);

        handler.Requests.ShouldAllBe(r => r.Method == HttpMethod.Post && r.Url.AbsoluteUri == Base);
        var bearer = JsonNode.Parse(handler.Requests[0].Body!)!;
        bearer["type"]!.GetValue<string>().ShouldBe("bearer_token");
        bearer["id"]!.GetValue<string>().ShouldBe("gh");
        bearer["token"]!.GetValue<string>().ShouldBe("secret");
        bearer["header_name"]!.GetValue<string>().ShouldBe("Authorization");
        bearer["prefix"]!.GetValue<string>().ShouldBe("token");
        var env = JsonNode.Parse(handler.Requests[1].Body!)!;
        env["type"]!.GetValue<string>().ShouldBe("environment_variable");
        env["injection_location"]![0]!.GetValue<string>().ShouldBe("header");
        env["trusted_domains"]![0]!.GetValue<string>().ShouldBe("api.example.com");
        var oauth = JsonNode.Parse(handler.Requests[2].Body!)!;
        oauth["type"]!.GetValue<string>().ShouldBe("oauth2");
        oauth["client_id"]!.GetValue<string>().ShouldBe("cid");
        oauth["refresh_token"]!.GetValue<string>().ShouldBe("rt");
        oauth["token_url"]!.GetValue<string>().ShouldBe("https://oauth2.googleapis.com/token");
        oauth["scopes"]![0]!.GetValue<string>().ShouldBe("s1");

        created.Id.ShouldBe("gh");
        created.Type.ShouldBe(CredentialTypes.BearerToken);
        created.Status.ShouldBe(CredentialStatuses.Active);
        created.UpdateTime.ShouldBe("2026-11-02T00:00:00Z");
    }

    [Fact]
    public async Task CreateAsync_RequiresId()
    {
        var (client, _) = Create();
        await Should.ThrowAsync<ArgumentException>(() =>
            client.CreateAsync(new BearerTokenCredentialConfig { Token = "t" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateAsync_SendsPatchWithoutIdAndUpdateMask()
    {
        var (client, handler) = Create(CredentialJson);

        await client.UpdateAsync("credentials/gh", new BearerTokenCredentialConfig { Id = "ignored", Token = "new" },
            "token", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.Method.ShouldBe("PATCH");
        request.Url.AbsoluteUri.ShouldBe(Base + "/gh?update_mask=token");
        var body = JsonNode.Parse(request.Body!)!.AsObject();
        body.ContainsKey("id").ShouldBeFalse();
        body["type"]!.GetValue<string>().ShouldBe("bearer_token");
        body["token"]!.GetValue<string>().ShouldBe("new");
    }

    [Fact]
    public async Task GetDeleteAndList_UseExpectedUrls()
    {
        var (client, handler) = Create(CredentialJson, "{}",
            """{"credentials":[{"id":"gh","type":"some_future_type","status":"revoked"}],"next_page_token":"n"}""",
            """{"credentials":[{"id":"x"}]}""");

        await client.GetAsync("gh", TestContext.Current.CancellationToken);
        await client.DeleteAsync("gh", TestContext.Current.CancellationToken);
        var all = new List<Credential>();
        await foreach (var credential in client.ListAllAsync(20, TestContext.Current.CancellationToken))
            all.Add(credential);

        handler.Requests[0].Url.AbsoluteUri.ShouldBe(Base + "/gh");
        handler.Requests[1].Method.ShouldBe(HttpMethod.Delete);
        handler.Requests[2].Url.AbsoluteUri.ShouldBe(Base + "?page_size=20");
        handler.Requests[3].Url.AbsoluteUri.ShouldBe(Base + "?page_size=20&page_token=n");
        all.Select(c => c.Id).ShouldBe(new[] { "gh", "x" });
        all[0].Type.ShouldBe("some_future_type");
    }

    [Fact]
    public void CredentialConfig_ReadsSingleInjectionLocationAndUnknownTypes()
    {
        var env = JsonSerializer.Deserialize(
            """{"type":"environment_variable","id":"k","value":"v","injection_location":"query"}""",
            AgentApisSerializerContext.Default.CredentialConfig);
        env.ShouldBeOfType<EnvironmentVariableCredentialConfig>().InjectionLocation!.ShouldBe(new[] { "query" });

        var unknown = JsonSerializer.Deserialize("""{"type":"mtls","id":"k","cert":"..."}""",
            AgentApisSerializerContext.Default.CredentialConfig);
        unknown.ShouldBeOfType<UnknownCredentialConfig>().Type.ShouldBe("mtls");
        JsonNode.Parse(JsonSerializer.Serialize(unknown!, AgentApisSerializerContext.Default.CredentialConfig))!["cert"]!
            .GetValue<string>().ShouldBe("...");
    }
}
