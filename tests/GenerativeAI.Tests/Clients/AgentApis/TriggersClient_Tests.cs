using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients.AgentApis;

/// <summary>
/// Offline tests of <see cref="TriggersClient"/> (wire format follows python-genai's <c>_gaos/triggers.py</c>).
/// </summary>
public class TriggersClient_Tests
{
    private const string Base = "https://generativelanguage.googleapis.com/v1beta/triggers";

    private const string TriggerJson = """
        {
          "id": "t-1", "display_name": "Daily", "schedule": "0 9 * * *", "time_zone": "UTC",
          "interaction": {"agent": "antigravity-preview-05-2026", "input": "Summarize the news",
                          "agent_config": {"type": "unreleased-agent", "x": 1}},
          "environment_id": "env-1", "execution_timeout_seconds": 600, "max_consecutive_failures": 3,
          "consecutive_failure_count": 0, "status": "active", "next_run_time": "2026-11-16T09:00:00Z"
        }
        """;

    private static (TriggersClient Client, AgentApiTestHandler Handler) Create(params string[] responses)
    {
        var handler = new AgentApiTestHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new TriggersClient(AgentApiTestHandler.GooglePlatform(), new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task CreateAsync_PostsTriggerAndParsesUnknownAgentConfig()
    {
        var (client, handler) = Create(TriggerJson);

        var trigger = await client.CreateAsync(new CreateTriggerRequest
        {
            DisplayName = "Daily",
            Schedule = "0 9 * * *",
            TimeZone = "UTC",
            EnvironmentId = "env-1",
            ExecutionTimeoutSeconds = 600,
            MaxConsecutiveFailures = 3,
            Interaction = new CreateInteractionRequest
            {
                Agent = InteractionAgents.AntigravityPreview052026,
                Input = "Summarize the news",
                Background = true
            }
        }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(Base);
        var body = JsonNode.Parse(request.Body!)!;
        body["schedule"]!.GetValue<string>().ShouldBe("0 9 * * *");
        body["time_zone"]!.GetValue<string>().ShouldBe("UTC");
        body["display_name"]!.GetValue<string>().ShouldBe("Daily");
        body["environment_id"]!.GetValue<string>().ShouldBe("env-1");
        body["execution_timeout_seconds"]!.GetValue<int>().ShouldBe(600);
        body["max_consecutive_failures"]!.GetValue<int>().ShouldBe(3);
        body["interaction"]!["agent"]!.GetValue<string>().ShouldBe("antigravity-preview-05-2026");
        body["interaction"]!["input"]!.GetValue<string>().ShouldBe("Summarize the news");
        body["interaction"]!["background"]!.GetValue<bool>().ShouldBeTrue();

        trigger.Id.ShouldBe("t-1");
        trigger.Status.ShouldBe(TriggerStatuses.Active);
        trigger.NextRunTime.ShouldBe("2026-11-16T09:00:00Z");
        trigger.Interaction!.Agent.ShouldBe("antigravity-preview-05-2026");
        trigger.Interaction.AgentConfig.ShouldBeOfType<UnknownInteractionAgentConfig>().Type.ShouldBe("unreleased-agent");
    }

    [Fact]
    public async Task CreateAsync_WrapsContentListInputInUserInputStep()
    {
        var (client, handler) = Create(TriggerJson);

        await client.CreateAsync(new CreateTriggerRequest
        {
            Schedule = "@daily",
            TimeZone = "UTC",
            Interaction = new CreateInteractionRequest
            {
                Agent = InteractionAgents.AntigravityPreview052026,
                Input = InteractionInput.FromContents(new[] { new InteractionTextContent { Text = "Hello" } })
            }
        }, TestContext.Current.CancellationToken);

        var input = JsonNode.Parse(handler.Requests.Single().Body!)!["interaction"]!["input"]!;
        input[0]!["type"]!.GetValue<string>().ShouldBe("user_input");
        input[0]!["content"]![0]!["text"]!.GetValue<string>().ShouldBe("Hello");
    }

    [Fact]
    public async Task UpdatePauseResume_SendPatch()
    {
        var (client, handler) = Create(TriggerJson, TriggerJson, TriggerJson);

        await client.UpdateAsync("triggers/t-1", new UpdateTriggerRequest { DisplayName = "Renamed" },
            TestContext.Current.CancellationToken);
        await client.PauseAsync("t-1", TestContext.Current.CancellationToken);
        await client.ResumeAsync("t-1", TestContext.Current.CancellationToken);

        handler.Requests.ShouldAllBe(r => r.Method.Method == "PATCH" && r.Url.AbsoluteUri == Base + "/t-1");
        var first = JsonNode.Parse(handler.Requests[0].Body!)!.AsObject();
        first["display_name"]!.GetValue<string>().ShouldBe("Renamed");
        first.ContainsKey("status").ShouldBeFalse();
        JsonNode.Parse(handler.Requests[1].Body!)!["status"]!.GetValue<string>().ShouldBe("paused");
        JsonNode.Parse(handler.Requests[2].Body!)!["status"]!.GetValue<string>().ShouldBe("active");
    }

    [Fact]
    public async Task GetDeleteAndList_UseExpectedUrls()
    {
        var (client, handler) = Create(TriggerJson, "{}", """{"triggers":[{"id":"t-1"}],"next_page_token":"n"}""");

        await client.GetAsync("t-1", TestContext.Current.CancellationToken);
        await client.DeleteAsync("t-1", TestContext.Current.CancellationToken);
        var page = await client.ListAsync("status=active", 2, "p", TestContext.Current.CancellationToken);

        handler.Requests[0].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe(Base + "/t-1");
        handler.Requests[1].Method.ShouldBe(HttpMethod.Delete);
        handler.Requests[2].Url.Query.ShouldBe("?filter=status%3Dactive&page_size=2&page_token=p");
        page.Triggers!.Single().Id.ShouldBe("t-1");
    }

    [Fact]
    public async Task RunAndListExecutions_UseExecutionsCollection()
    {
        var (client, handler) = Create(
            """{"id":"x-1","trigger_id":"t-1","status":"in_progress","scheduled_time":"2026-11-15T00:00:00Z"}""",
            """{"trigger_executions":[{"id":"x-1","trigger_id":"t-1","status":"completed","interaction_id":"i-1"}]}""");

        var execution = await client.RunAsync("t-1", TestContext.Current.CancellationToken);
        var page = await client.ListExecutionsAsync("t-1", pageSize: 10,
            cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests[0].Method.ShouldBe(HttpMethod.Post);
        handler.Requests[0].Url.AbsoluteUri.ShouldBe(Base + "/t-1/executions");
        handler.Requests[0].Body.ShouldBeNull();
        handler.Requests[1].Method.ShouldBe(HttpMethod.Get);
        handler.Requests[1].Url.AbsoluteUri.ShouldBe(Base + "/t-1/executions?page_size=10");
        execution.Status.ShouldBe(TriggerExecutionStatuses.InProgress);
        execution.ScheduledTime.ShouldBe("2026-11-15T00:00:00Z");
        page.TriggerExecutions!.Single().InteractionId.ShouldBe("i-1");
    }

    [Fact]
    public async Task WaitForExecutionAsync_PollsUntilTerminal()
    {
        var (client, handler) = Create(
            """{"trigger_executions":[{"id":"x-1","status":"in_progress"}]}""",
            """{"trigger_executions":[{"id":"x-0","status":"completed"}],"next_page_token":"p2"}""",
            """{"trigger_executions":[{"id":"x-1","status":"failed","error":"boom"}]}""");

        var execution = await client.WaitForExecutionAsync("t-1", "x-1", TimeSpan.FromMilliseconds(1),
            cancellationToken: TestContext.Current.CancellationToken);

        execution.Status.ShouldBe(TriggerExecutionStatuses.Failed);
        execution.Error.ShouldBe("boom");
        handler.Requests.Count.ShouldBe(3);
    }
}
