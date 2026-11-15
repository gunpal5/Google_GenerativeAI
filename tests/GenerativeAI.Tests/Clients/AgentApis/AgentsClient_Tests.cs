using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>
/// Offline tests of <see cref="AgentsClient"/> (wire format follows python-genai's <c>_gaos/agents.py</c>).
/// </summary>
public class AgentsClient_Tests
{
    private const string Base = "https://generativelanguage.googleapis.com/v1beta/agents";

    private const string AgentJson = """
        {
          "id": "my-agent",
          "base_agent": "antigravity-preview-05-2026",
          "description": "Helper",
          "system_instruction": "Be helpful",
          "agent_config": {"type": "antigravity", "model": "gemini-3-pro", "max_total_tokens": "100000"},
          "base_environment": {"type": "remote", "network": "disabled",
                               "sources": [{"type": "gcs", "source": "gs://bucket/data", "target": "/data"}]},
          "tools": [
            {"type": "google_search"},
            {"type": "function", "name": "lookup", "parameters": {"type": "object"}},
            {"type": "brand_new_tool", "setting": 1}
          ]
        }
        """;

    private static (AgentsClient Client, AgentApiTestHandler Handler) Create(params string[] responses)
    {
        var handler = new AgentApiTestHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new AgentsClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task CreateAsync_PostsAgentAndParsesUnknownTool()
    {
        var (client, handler) = Create(AgentJson);

        var agent = await client.CreateAsync(new Agent
        {
            Id = "my-agent",
            BaseAgent = InteractionAgents.AntigravityPreview052026,
            SystemInstruction = "Be helpful",
            AgentConfig = new AntigravityAgentConfig { Model = "gemini-3-pro", MaxTotalTokens = 100000 },
            BaseEnvironment = "env-1",
            Tools = new List<InteractionTool> { new InteractionGoogleSearchTool() }
        }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(Base);
        var body = JsonNode.Parse(request.Body!)!;
        body["id"]!.GetValue<string>().ShouldBe("my-agent");
        body["base_agent"]!.GetValue<string>().ShouldBe("antigravity-preview-05-2026");
        body["system_instruction"]!.GetValue<string>().ShouldBe("Be helpful");
        body["agent_config"]!["type"]!.GetValue<string>().ShouldBe("antigravity");
        body["agent_config"]!["max_total_tokens"]!.GetValue<string>().ShouldBe("100000");
        body["base_environment"]!.GetValue<string>().ShouldBe("env-1");
        body["tools"]![0]!["type"]!.GetValue<string>().ShouldBe("google_search");

        agent.AgentConfig.ShouldBeOfType<AntigravityAgentConfig>().MaxTotalTokens.ShouldBe(100000);
        agent.BaseEnvironment!.Environment!.Network!.GetValue<string>().ShouldBe("disabled");
        agent.BaseEnvironment.Environment.Sources!.Single().Source.ShouldBe("gs://bucket/data");
        agent.Tools!.Count.ShouldBe(3);
        agent.Tools[0].ShouldBeOfType<InteractionGoogleSearchTool>();
        agent.Tools[1].ShouldBeOfType<InteractionFunctionTool>().Name.ShouldBe("lookup");
        var unknown = agent.Tools[2].ShouldBeOfType<UnknownInteractionTool>();
        unknown.Type.ShouldBe("brand_new_tool");
    }

    [Fact]
    public async Task GetDeleteAndList_UseExpectedUrls()
    {
        var (client, handler) = Create(AgentJson, "{}",
            """{"agents":[{"id":"a"}],"next_page_token":"n"}""");

        (await client.GetAsync("agents/my-agent", TestContext.Current.CancellationToken)).Id.ShouldBe("my-agent");
        await client.DeleteAsync("my-agent", TestContext.Current.CancellationToken);
        var page = await client.ListAsync(5, "tok", "projects/p", TestContext.Current.CancellationToken);

        handler.Requests[0].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe(Base + "/my-agent");
        handler.Requests[1].Method.ShouldBe(HttpMethod.Delete);
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(Base + "/my-agent");
        handler.Requests[2].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[2].Url.Query.ShouldBe("?page_size=5&page_token=tok&parent=projects%2Fp");
        page.Agents!.Single().Id.ShouldBe("a");
        page.NextPageToken.ShouldBe("n");
    }

    [Fact]
    public async Task ListAllAsync_FollowsPageTokens()
    {
        var (client, handler) = Create("""{"agents":[{"id":"a"}],"next_page_token":"p2"}""", """{"agents":[{"id":"b"}]}""");

        var ids = new List<string?>();
        await foreach (var agent in client.ListAllAsync(cancellationToken: TestContext.Current.CancellationToken))
            ids.Add(agent.Id);

        ids.ShouldBe(new[] { "a", "b" });
        handler.Requests.Count.ShouldBe(2);
    }

    [Fact]
    public async Task VertexAI_UsesProjectLocationPathAndBearerToken()
    {
        var handler = new AgentApiTestHandler().Enqueue(AgentJson);
        var platform = AgentApiTestHandler.VertexPlatform();
        var client = new AgentsClient(platform, new HttpClient(handler));

        await client.GetAsync("my-agent", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Url.AbsoluteUri.ShouldBe(
            $"https://us-central1-aiplatform.googleapis.com/{platform.GetApiVersion()}/projects/test-project/locations/us-central1/agents/my-agent");
        request.Headers["Authorization"].ShouldStartWith("Bearer");
    }
}
