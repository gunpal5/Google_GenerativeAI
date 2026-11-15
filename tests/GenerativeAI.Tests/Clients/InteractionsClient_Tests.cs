using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Clients;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Unit tests for <see cref="InteractionsClient"/> and the Interactions types (wire format follows python-genai's
/// generated Interactions client).
/// </summary>
public class InteractionsClient_Tests
{
    private const string GoogleBase = "https://generativelanguage.googleapis.com/v1beta/interactions";

    private sealed record CapturedRequest(HttpMethod Method, Uri Url, string? Body, HttpRequestMessage Message);

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Queue<(string Body, string MediaType)> _responses = new();

        public List<CapturedRequest> Requests { get; } = new();

        public CapturingHandler Enqueue(string body, string mediaType = "application/json")
        {
            _responses.Enqueue((body, mediaType));
            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri!, body, request));
            var (responseBody, mediaType) = _responses.Count > 0 ? _responses.Dequeue() : ("{}", "application/json");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, mediaType)
            };
        }
    }

    private static (InteractionsClient Client, CapturingHandler Handler) CreateGoogle(params string[] responses)
    {
        var handler = new CapturingHandler();
        foreach (var response in responses)
            handler.Enqueue(response);
        return (new InteractionsClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler)), handler);
    }

    private const string CompletedInteractionJson = """
        {
          "id": "v1_abc",
          "status": "completed",
          "model": "gemini-3-flash-preview",
          "created": "2026-11-01T10:00:00Z",
          "updated": "2026-11-01T10:00:05Z",
          "steps": [
            {"type": "user_input", "content": [{"type": "text", "text": "Hi"}]},
            {"type": "thought", "signature": "c2ln", "summary": [{"type": "text", "text": "thinking"}]},
            {"type": "model_output", "content": [{"type": "text", "text": "Hello, "}, {"type": "text", "text": "world"}]}
          ],
          "usage": {"total_input_tokens": 3, "total_output_tokens": 4, "total_thought_tokens": 5, "total_tokens": 12,
                    "output_tokens_by_modality": [{"modality": "text", "tokens": 4}]}
        }
        """;

    private static Interaction Deserialize(string json) =>
        JsonSerializer.Deserialize(json, InteractionsSerializerContext.Default.Interaction)!;

    [Fact]
    public void SerializerContext_ProvidesTypeInfoForRootTypes()
    {
        InteractionsSerializerContext.Default.GetTypeInfo(typeof(Interaction)).ShouldNotBeNull();
        InteractionsSerializerContext.Default.GetTypeInfo(typeof(CreateInteractionRequest)).ShouldNotBeNull();
        InteractionsSerializerContext.Default.GetTypeInfo(typeof(InteractionSseEvent)).ShouldNotBeNull();
        InteractionsSerializerContext.Default.GetTypeInfo(typeof(InteractionStep)).ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_PostsSnakeCaseBodyToInteractions()
    {
        var (client, handler) = CreateGoogle(CompletedInteractionJson);

        var result = await client.CreateAsync(new CreateInteractionRequest
        {
            Model = InteractionModels.Gemini3FlashPreview,
            Input = "Hi",
            SystemInstruction = "Be brief",
            Store = false,
            PreviousInteractionId = "v1_prev",
            GenerationConfig = new InteractionGenerationConfig
            {
                ThinkingLevel = InteractionThinkingLevels.Low,
                MaxOutputTokens = 100,
                ToolChoice = new InteractionToolChoice { Mode = InteractionToolChoiceModes.Auto }
            },
            Tools = new List<InteractionTool>
            {
                new InteractionFunctionTool
                {
                    Name = "get_weather", Description = "Weather",
                    Parameters = JsonNode.Parse("""{"type":"object","properties":{"city":{"type":"string"}}}""")
                },
                new InteractionGoogleSearchTool()
            },
            Stream = true // ignored by CreateAsync
        }, TestContext.Current.CancellationToken);

        var request = handler.Requests.ShouldHaveSingleItem();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase);
        request.Message.Headers.GetValues("x-goog-api-key").ShouldContain("test-key");
        var body = JsonNode.Parse(request.Body!)!;
        body["model"]!.GetValue<string>().ShouldBe("gemini-3-flash-preview");
        body["input"]!.GetValue<string>().ShouldBe("Hi");
        body["system_instruction"]!.GetValue<string>().ShouldBe("Be brief");
        body["store"]!.GetValue<bool>().ShouldBeFalse();
        body["previous_interaction_id"]!.GetValue<string>().ShouldBe("v1_prev");
        body["generation_config"]!["thinking_level"]!.GetValue<string>().ShouldBe("low");
        body["generation_config"]!["max_output_tokens"]!.GetValue<int>().ShouldBe(100);
        body["generation_config"]!["tool_choice"]!.GetValue<string>().ShouldBe("auto");
        body["tools"]![0]!["type"]!.GetValue<string>().ShouldBe("function");
        body["tools"]![0]!["name"]!.GetValue<string>().ShouldBe("get_weather");
        body["tools"]![0]!["parameters"]!["properties"]!["city"]!["type"]!.GetValue<string>().ShouldBe("string");
        body["tools"]![1]!["type"]!.GetValue<string>().ShouldBe("google_search");
        body.AsObject().ContainsKey("stream").ShouldBeFalse();
        body.AsObject().ContainsKey("agent").ShouldBeFalse();

        result.Id.ShouldBe("v1_abc");
        result.Status.ShouldBe(InteractionStatuses.Completed);
        result.OutputText.ShouldBe("Hello, world");
    }

    [Fact]
    public async Task CreateAsync_AgentInteraction_SendsAgentConfigAndBackground()
    {
        var (client, handler) = CreateGoogle("""{"id":"v1_dr","status":"in_progress","agent":"deep-research-preview-04-2026"}""");

        var result = await client.CreateAsync(new CreateInteractionRequest
        {
            Agent = InteractionAgents.DeepResearchPreview042026,
            Input = "Research quantum batteries",
            Background = true,
            AgentConfig = new DeepResearchAgentConfig
            {
                ThinkingSummaries = InteractionThinkingSummaries.Auto, CollaborativePlanning = true
            }
        }, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Requests.Single().Body!)!;
        body["agent"]!.GetValue<string>().ShouldBe("deep-research-preview-04-2026");
        body["background"]!.GetValue<bool>().ShouldBeTrue();
        body["agent_config"]!["type"]!.GetValue<string>().ShouldBe("deep-research");
        body["agent_config"]!["thinking_summaries"]!.GetValue<string>().ShouldBe("auto");
        body["agent_config"]!["collaborative_planning"]!.GetValue<bool>().ShouldBeTrue();
        body.AsObject().ContainsKey("model").ShouldBeFalse();
        result.Status.ShouldBe(InteractionStatuses.InProgress);
        InteractionStatuses.IsRunning(result.Status).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAsync_AntigravityAgent_WritesEnvironmentAndInt64AsString()
    {
        var (client, handler) = CreateGoogle("""{"id":"v1_ag","status":"queued"}""");

        await client.CreateAsync(new CreateInteractionRequest
        {
            Agent = InteractionAgents.AntigravityPreview092026,
            Input = "Fix the build",
            AgentConfig = new AntigravityAgentConfig { MaxTotalTokens = 5000000000 },
            Environment = new InteractionEnvironment
            {
                Sources = new List<InteractionEnvironmentSource>
                    { new() { Type = "repository", Source = "https://github.com/org/repo", Target = "/src" } },
                Network = JsonValue.Create("disabled")
            }
        }, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Requests.Single().Body!)!;
        body["agent_config"]!["type"]!.GetValue<string>().ShouldBe("antigravity");
        body["agent_config"]!["max_total_tokens"]!.GetValue<string>().ShouldBe("5000000000");
        body["environment"]!["type"]!.GetValue<string>().ShouldBe("remote");
        body["environment"]!["network"]!.GetValue<string>().ShouldBe("disabled");
        body["environment"]!["sources"]![0]!["target"]!.GetValue<string>().ShouldBe("/src");

        var byId = JsonSerializer.Serialize(new CreateInteractionRequest { Agent = "a", Environment = "env-123" },
            InteractionsSerializerContext.Default.CreateInteractionRequest);
        JsonNode.Parse(byId)!["environment"]!.GetValue<string>().ShouldBe("env-123");
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidModelAgentCombinations()
    {
        var (client, _) = CreateGoogle();
        var ct = TestContext.Current.CancellationToken;
        await Should.ThrowAsync<ArgumentException>(() =>
            client.CreateAsync(new CreateInteractionRequest { Model = "m", Agent = "a" }, ct));
        await Should.ThrowAsync<ArgumentException>(() => client.CreateAsync(new CreateInteractionRequest(), ct));
        await Should.ThrowAsync<ArgumentException>(() => client.CreateAsync(new CreateInteractionRequest
            { Agent = "a", GenerationConfig = new InteractionGenerationConfig() }, ct));
        await Should.ThrowAsync<ArgumentException>(() => client.CreateAsync(new CreateInteractionRequest
            { Model = "m", AgentConfig = new DeepResearchAgentConfig() }, ct));
    }

    [Fact]
    public async Task CreateAsync_VideoGeneration_SendsResponseFormatAndVideoConfig()
    {
        var (client, handler) = CreateGoogle(GeneratedVideoJson);

        var result = await client.CreateAsync(new CreateInteractionRequest
        {
            Model = InteractionModels.GeminiOmni12Flash,
            Input = InteractionInput.FromContents(new InteractionContent[]
            {
                InteractionContent.FromText("Animate this cat"),
                new InteractionImageContent { Uri = "https://example.com/cat.png", MimeType = "image/png" }
            }),
            ResponseFormat = new List<InteractionResponseFormat>
            {
                new InteractionVideoResponseFormat
                {
                    AspectRatio = "16:9", Resolution = "1080p", Duration = "8s",
                    Delivery = InteractionDeliveryModes.Uri
                }
            },
            GenerationConfig = new InteractionGenerationConfig
                { VideoConfig = new InteractionVideoConfig { Task = "image_to_video" } }
        }, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Requests.Single().Body!)!;
        body["model"]!.GetValue<string>().ShouldBe("gemini-omni-1.2-flash");
        body["input"]!.AsArray().Count.ShouldBe(2);
        body["input"]![1]!["type"]!.GetValue<string>().ShouldBe("image");
        body["input"]![1]!["uri"]!.GetValue<string>().ShouldBe("https://example.com/cat.png");
        var format = body["response_format"]!.AsObject(); // a single format is sent as an object
        format["type"]!.GetValue<string>().ShouldBe("video");
        format["aspect_ratio"]!.GetValue<string>().ShouldBe("16:9");
        format["resolution"]!.GetValue<string>().ShouldBe("1080p");
        format["delivery"]!.GetValue<string>().ShouldBe("uri");
        body["generation_config"]!["video_config"]!["task"]!.GetValue<string>().ShouldBe("image_to_video");

        result.OutputVideo.ShouldNotBeNull();
    }

    private const string GeneratedVideoJson = """
        {
          "id": "v1_video",
          "status": "completed",
          "model": "gemini-omni-1.2-flash",
          "steps": [
            {"type": "user_input", "content": [{"type": "text", "text": "Animate this cat"}]},
            {"type": "model_output", "content": [
              {"type": "text", "text": "Here is your video."},
              {"type": "image", "data": "aW1n", "mime_type": "image/png"},
              {"type": "audio", "data": "YXVk", "mime_type": "audio/wav", "sample_rate": 24000, "channels": 1},
              {"type": "video", "uri": "https://generativelanguage.googleapis.com/v1beta/files/abc:download", "mime_type": "video/mp4", "resolution": "high"}
            ]},
            {"type": "model_output", "content": [
              {"type": "video", "data": "AAECAw==", "mime_type": "video/mp4"}
            ]}
          ],
          "usage": {"total_output_tokens": 10, "output_tokens_by_modality": [{"modality": "video", "tokens": 10}]}
        }
        """;

    [Fact]
    public void Deserialize_GeneratedVideo_ExposesOutputHelpers()
    {
        var interaction = Deserialize(GeneratedVideoJson);

        var video = interaction.OutputVideo.ShouldNotBeNull();
        video.MimeType.ShouldBe("video/mp4");
        video.Uri.ShouldBeNull();
        video.GetDataBytes().ShouldBe(new byte[] { 0, 1, 2, 3 });

        var uriVideo = interaction.Steps!.OfType<InteractionModelOutputStep>().First().Content!
            .OfType<InteractionVideoContent>().Single();
        uriVideo.Uri.ShouldBe("https://generativelanguage.googleapis.com/v1beta/files/abc:download");
        uriVideo.Resolution.ShouldBe("high");
        uriVideo.GetDataBytes().ShouldBeNull();

        interaction.OutputImage!.GetDataBytes().ShouldBe(Encoding.ASCII.GetBytes("img"));
        interaction.OutputAudio!.SampleRate.ShouldBe(24000);
        interaction.OutputAudio.Channels.ShouldBe(1);
        // As in python-genai, non-text content after the text is skipped when looking for the trailing text.
        interaction.OutputText.ShouldBe("Here is your video.");
        interaction.Usage!.OutputTokensByModality![0].Modality.ShouldBe(InteractionModalities.Video);
    }

    [Fact]
    public void Deserialize_StepsOfAllKinds_IncludingUnknownStep()
    {
        const string json = """
            {
              "id": "v1_steps", "status": "requires_action", "object": "interaction",
              "some_future_field": {"a": 1},
              "steps": [
                {"type": "user_input", "content": [{"type": "text", "text": "Weather in Paris?"}]},
                {"type": "google_search_call", "id": "gs1", "arguments": {"queries": ["paris weather"]}, "search_type": "web_search", "signature": "c2ln"},
                {"type": "google_search_result", "call_id": "gs1", "result": [{"search_suggestions": "<div/>"}]},
                {"type": "code_execution_call", "id": "ce1", "arguments": {"code": "print(1)", "language": "python"}},
                {"type": "code_execution_result", "call_id": "ce1", "result": "1\n", "is_error": false},
                {"type": "url_context_call", "id": "u1", "arguments": {"urls": ["https://a.b"]}},
                {"type": "url_context_result", "call_id": "u1", "result": [{"status": "success", "url": "https://a.b"}]},
                {"type": "mcp_server_tool_call", "id": "m1", "name": "list", "server_name": "srv", "arguments": {"x": 1}},
                {"type": "mcp_server_tool_result", "call_id": "m1", "result": [{"type": "text", "text": "ok"}]},
                {"type": "model_output", "content": [{"type": "text", "text": "Let me check.", "annotations": [
                  {"type": "url_citation", "url": "https://a.b", "title": "A", "start_index": 0, "end_index": 5},
                  {"type": "brand_new_citation", "foo": "bar"}
                ]}]},
                {"type": "function_call", "id": "fc1", "name": "get_weather", "arguments": {"city": "Paris"}},
                {"type": "holographic_projection", "id": "h1", "payload": {"x": [1, 2]}}
              ],
              "usage": {"total_thought_tokens": 7}
            }
            """;

        var interaction = Deserialize(json);
        var steps = interaction.Steps!;
        steps.Count.ShouldBe(12);
        steps[0].ShouldBeOfType<InteractionUserInputStep>();
        var search = steps[1].ShouldBeOfType<InteractionGoogleSearchCallStep>();
        search.Id.ShouldBe("gs1");
        search.Arguments!.Queries!.ShouldBe(new[] { "paris weather" });
        search.SearchType.ShouldBe("web_search");
        steps[2].ShouldBeOfType<InteractionGoogleSearchResultStep>().Result![0].SearchSuggestions.ShouldBe("<div/>");
        steps[3].ShouldBeOfType<InteractionCodeExecutionCallStep>().Arguments!.Language.ShouldBe("python");
        steps[4].ShouldBeOfType<InteractionCodeExecutionResultStep>().Result!.GetValue<string>().ShouldBe("1\n");
        steps[5].ShouldBeOfType<InteractionUrlContextCallStep>().Arguments!.Urls!.ShouldHaveSingleItem();
        steps[6].ShouldBeOfType<InteractionUrlContextResultStep>().Result![0].Status.ShouldBe("success");
        steps[7].ShouldBeOfType<InteractionMcpServerToolCallStep>().ServerName.ShouldBe("srv");
        steps[8].ShouldBeOfType<InteractionMcpServerToolResultStep>().Result!.AsArray().Count.ShouldBe(1);
        var output = steps[9].ShouldBeOfType<InteractionModelOutputStep>();
        var text = output.Content![0].ShouldBeOfType<InteractionTextContent>();
        text.Annotations![0].ShouldBeOfType<InteractionUrlCitation>().EndIndex.ShouldBe(5);
        text.Annotations[1].ShouldBeOfType<UnknownInteractionAnnotation>().Type.ShouldBe("brand_new_citation");
        var call = steps[10].ShouldBeOfType<InteractionFunctionCallStep>();
        call.Arguments!["city"]!.GetValue<string>().ShouldBe("Paris");

        var unknown = steps[11].ShouldBeOfType<UnknownInteractionStep>();
        unknown.Type.ShouldBe("holographic_projection");
        unknown.Raw["payload"]!["x"]![1]!.GetValue<int>().ShouldBe(2);

        interaction.Object.ShouldBe("interaction");
        interaction.ExtensionData!["some_future_field"].GetProperty("a").GetInt32().ShouldBe(1);
        interaction.Usage!.TotalThoughtTokens.ShouldBe(7);
        interaction.FunctionCalls.ShouldHaveSingleItem().Name.ShouldBe("get_weather");

        // Unknown variants are written back unchanged; known ones keep their discriminator exactly once.
        var roundTrip = JsonNode.Parse(JsonSerializer.Serialize(interaction, InteractionsSerializerContext.Default.Interaction))!;
        roundTrip["steps"]![11]!.ToJsonString().ShouldBe(unknown.Raw.ToJsonString());
        roundTrip["steps"]![10]!["type"]!.GetValue<string>().ShouldBe("function_call");
        roundTrip["steps"]![9]!["content"]![0]!["annotations"]![1]!["foo"]!.GetValue<string>().ShouldBe("bar");
        roundTrip["some_future_field"].ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_UnknownContentToolAgentConfigAndFields_DoNotThrow()
    {
        const string json = """
            {
              "id": "x", "status": "brand_new_status",
              "agent_config": {"type": "time-travel", "year": 3000},
              "tools": [{"type": "teleport", "range": 5}, {"type": "url_context"}],
              "steps": [{"type": "model_output", "content": [{"type": "smell", "intensity": 3},
                {"type": "text", "text": "hi", "extra_text_field": true}]}]
            }
            """;

        var interaction = Deserialize(json);
        interaction.Status.ShouldBe("brand_new_status");
        InteractionStatuses.IsRunning(interaction.Status).ShouldBeFalse();
        interaction.AgentConfig.ShouldBeOfType<UnknownInteractionAgentConfig>().Raw["year"]!.GetValue<int>().ShouldBe(3000);
        interaction.Tools![0].ShouldBeOfType<UnknownInteractionTool>().Type.ShouldBe("teleport");
        interaction.Tools[1].ShouldBeOfType<InteractionUrlContextTool>();
        var content = ((InteractionModelOutputStep)interaction.Steps![0]).Content!;
        content[0].ShouldBeOfType<UnknownInteractionContent>().Type.ShouldBe("smell");
        content[1].ExtensionData!["extra_text_field"].GetBoolean().ShouldBeTrue();
        interaction.OutputText.ShouldBe("hi");
    }

    [Fact]
    public void InteractionInput_RoundTripsAllShapes()
    {
        var steps = InteractionInput.FromSteps(new InteractionStep[]
        {
            new InteractionUserInputStep { Content = new List<InteractionContent> { InteractionContent.FromText("Weather?") } },
            new InteractionFunctionCallStep { Id = "fc1", Name = "get_weather", Arguments = new JsonObject { ["city"] = "Paris" } },
            new InteractionFunctionResultStep { CallId = "fc1", Name = "get_weather", Result = new JsonObject { ["temp"] = 21 } }
        });
        var request = new CreateInteractionRequest { Model = "m", Input = steps };
        var json = JsonSerializer.Serialize(request, InteractionsSerializerContext.Default.CreateInteractionRequest);
        var node = JsonNode.Parse(json)!;
        node["input"]![2]!["type"]!.GetValue<string>().ShouldBe("function_result");
        node["input"]![2]!["call_id"]!.GetValue<string>().ShouldBe("fc1");
        node["input"]![2]!["result"]!["temp"]!.GetValue<int>().ShouldBe(21);

        var back = JsonSerializer.Deserialize(json, InteractionsSerializerContext.Default.CreateInteractionRequest)!;
        back.Input!.Steps!.Count.ShouldBe(3);
        back.Input.Steps[2].ShouldBeOfType<InteractionFunctionResultStep>().CallId.ShouldBe("fc1");

        var contents = JsonSerializer.Deserialize("""{"model":"m","input":[{"type":"text","text":"a"},{"type":"image","uri":"u"}]}""",
            InteractionsSerializerContext.Default.CreateInteractionRequest)!;
        contents.Input!.Contents!.Count.ShouldBe(2);
        contents.Input.Contents[1].ShouldBeOfType<InteractionImageContent>().Uri.ShouldBe("u");

        var single = JsonSerializer.Deserialize("""{"model":"m","input":{"type":"text","text":"a"}}""",
            InteractionsSerializerContext.Default.CreateInteractionRequest)!;
        single.Input!.Content.ShouldBeOfType<InteractionTextContent>().Text.ShouldBe("a");

        var text = JsonSerializer.Deserialize("""{"model":"m","input":"hello"}""",
            InteractionsSerializerContext.Default.CreateInteractionRequest)!;
        text.Input!.Text.ShouldBe("hello");
    }

    [Fact]
    public async Task GetAsync_UsesIdAndIncludeInputQuery()
    {
        var (client, handler) = CreateGoogle(CompletedInteractionJson);

        var interaction = await client.GetAsync("interactions/v1_abc", includeInput: true,
            cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase + "/v1_abc?include_input=true");
        interaction.Usage!.TotalThoughtTokens.ShouldBe(5);
        interaction.Steps!.Count.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteAsync_SendsDelete()
    {
        var (client, handler) = CreateGoogle("{}");

        await client.DeleteAsync("v1_abc", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Delete);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase + "/v1_abc");
    }

    [Fact]
    public async Task CancelAsync_PostsToCancelWithoutBody()
    {
        var (client, handler) = CreateGoogle("""{"id":"v1_abc","status":"cancelled"}""");

        var interaction = await client.CancelAsync("v1_abc", TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase + "/v1_abc/cancel");
        request.Body.ShouldBeNull();
        interaction.Status.ShouldBe(InteractionStatuses.Cancelled);
    }

    [Fact]
    public async Task VertexAI_UsesProjectLocationPathAndBearerToken()
    {
        var handler = new CapturingHandler().Enqueue(CompletedInteractionJson);
        var platform = new VertextPlatformAdapter("test-project", "us-central1", accessToken: "test-token",
            validateAccessToken: false);
        var client = new InteractionsClient(platform, new HttpClient(handler));

        await client.GetAsync("v1_abc", cancellationToken: TestContext.Current.CancellationToken);

        var request = handler.Requests.Single();
        request.Url.AbsoluteUri.ShouldBe(
            $"https://us-central1-aiplatform.googleapis.com/{platform.GetApiVersion()}/projects/test-project/locations/us-central1/interactions/v1_abc");
        request.Message.Headers.Authorization!.Scheme.ShouldBe("Bearer");
    }

    private const string SseStream =
        "event: interaction.created\n" +
        "data: {\"event_type\":\"interaction.created\",\"event_id\":\"e1\",\"interaction\":{\"id\":\"v1_s\",\"status\":\"in_progress\",\"object\":\"interaction\"}}\n" +
        "\n" +
        ": keep-alive comment\n" +
        "\n" +
        "data: {\"event_type\":\"step.start\",\"event_id\":\"e2\",\"index\":0,\"step\":{\"type\":\"model_output\"}}\n" +
        "\n" +
        "data: {\"event_type\":\"step.delta\",\"event_id\":\"e3\",\"index\":0,\r\n" +
        "data: \"delta\":{\"type\":\"text\",\"text\":\"Hel\"}}\r\n" +
        "\r\n" +
        "data: {\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"text\",\"text\":\"lo\"},\"metadata\":{\"total_usage\":{\"total_tokens\":9}}}\n" +
        "\n" +
        "data: {\"event_type\":\"step.delta\",\"index\":0,\"delta\":{\"type\":\"sparkle\",\"amount\":2}}\n" +
        "\n" +
        "data: {\"event_type\":\"step.stop\",\"index\":0,\"usage\":{\"total_output_tokens\":2}}\n" +
        "\n" +
        "data: {\"event_type\":\"interaction.status_update\",\"interaction_id\":\"v1_s\",\"status\":\"requires_continuation\",\"continuation_token\":\"tok\"}\n" +
        "\n" +
        "data: {\"event_type\":\"some.future_event\",\"event_id\":\"e9\",\"x\":1}\n" +
        "\n" +
        "data: {\"event_type\":\"error\",\"error\":{\"code\":\"quota\",\"message\":\"slow down\"}}\n" +
        "\n" +
        "data: {\"event_type\":\"interaction.completed\",\"event_id\":\"e10\",\"interaction\":{\"id\":\"v1_s\",\"status\":\"completed\",\"usage\":{\"total_tokens\":9}}}\n" +
        "\n" +
        "data: [DONE]\n" +
        "\n" +
        "data: {\"event_type\":\"step.stop\",\"index\":99}\n" +
        "\n";

    [Fact]
    public async Task StreamAsync_PostsStreamTrueAndParsesServerSentEvents()
    {
        var handler = new CapturingHandler().Enqueue(SseStream, "text/event-stream");
        var client = new InteractionsClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler));

        var events = new List<InteractionSseEvent>();
        await foreach (var e in client.StreamAsync(new CreateInteractionRequest { Model = "gemini-3-flash-preview", Input = "Hi" },
                           TestContext.Current.CancellationToken))
            events.Add(e);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Post);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase);
        request.Message.Headers.Accept.ToString().ShouldBe("text/event-stream");
        JsonNode.Parse(request.Body!)!["stream"]!.GetValue<bool>().ShouldBeTrue();

        events.Count.ShouldBe(10); // [DONE] ends the stream
        var created = events[0].ShouldBeOfType<InteractionCreatedEvent>();
        created.EventId.ShouldBe("e1");
        created.Interaction!.Id.ShouldBe("v1_s");
        events[1].ShouldBeOfType<InteractionStepStartEvent>().Step.ShouldBeOfType<InteractionModelOutputStep>();
        var delta1 = events[2].ShouldBeOfType<InteractionStepDeltaEvent>(); // multi-line data, CRLF line endings
        delta1.Delta.ShouldBeOfType<InteractionTextDelta>().Text.ShouldBe("Hel");
        var delta2 = events[3].ShouldBeOfType<InteractionStepDeltaEvent>();
        delta2.Metadata!.TotalUsage!.TotalTokens.ShouldBe(9);
        events[4].ShouldBeOfType<InteractionStepDeltaEvent>().Delta.ShouldBeOfType<UnknownInteractionStepDeltaData>()
            .Type.ShouldBe("sparkle");
        events[5].ShouldBeOfType<InteractionStepStopEvent>().Usage!.TotalOutputTokens.ShouldBe(2);
        var status = events[6].ShouldBeOfType<InteractionStatusUpdateEvent>();
        status.Status.ShouldBe(InteractionStatuses.RequiresContinuation);
        status.ContinuationToken.ShouldBe("tok");
        var unknown = events[7].ShouldBeOfType<UnknownInteractionSseEvent>();
        unknown.EventType.ShouldBe("some.future_event");
        unknown.Raw["x"]!.GetValue<int>().ShouldBe(1);
        events[8].ShouldBeOfType<InteractionErrorEvent>().Error!.Message.ShouldBe("slow down");
        events[9].ShouldBeOfType<InteractionCompletedEvent>().Interaction!.Status.ShouldBe(InteractionStatuses.Completed);

        var text = string.Concat(events.OfType<InteractionStepDeltaEvent>().Select(e => e.Delta).OfType<InteractionTextDelta>()
            .Select(d => d.Text));
        text.ShouldBe("Hello");
    }

    [Fact]
    public async Task ResumeStreamAsync_GetsWithStreamAndLastEventId()
    {
        var handler = new CapturingHandler().Enqueue(
            "data: {\"event_type\":\"step.stop\",\"index\":1}\n\n", "text/event-stream");
        var client = new InteractionsClient(new GoogleAIPlatformAdapter("test-key"), new HttpClient(handler));

        var events = new List<InteractionSseEvent>();
        await foreach (var e in client.ResumeStreamAsync("v1_s", lastEventId: "e3",
                           cancellationToken: TestContext.Current.CancellationToken))
            events.Add(e);

        var request = handler.Requests.Single();
        request.Method.ShouldBe(HttpMethod.Get);
        request.Url.AbsoluteUri.ShouldBe(GoogleBase + "/v1_s?last_event_id=e3&stream=true");
        request.Message.Headers.Accept.ToString().ShouldBe("text/event-stream");
        events.ShouldHaveSingleItem().ShouldBeOfType<InteractionStepStopEvent>().Index.ShouldBe(1);
    }

    [Fact]
    public async Task WaitForCompletionAsync_PollsUntilNotRunning()
    {
        var (client, handler) = CreateGoogle(
            """{"id":"v1_bg","status":"queued"}""",
            """{"id":"v1_bg","status":"in_progress"}""",
            CompletedInteractionJson);

        var result = await client.WaitForCompletionAsync("v1_bg", TimeSpan.FromMilliseconds(1),
            cancellationToken: TestContext.Current.CancellationToken);

        handler.Requests.Count.ShouldBe(3);
        handler.Requests.ShouldAllBe(r => r.Method == HttpMethod.Get);
        result.Status.ShouldBe(InteractionStatuses.Completed);
    }

    [Fact]
    public async Task ContinueAsync_SendsContinuationTokenWithPreviousInteraction()
    {
        var (client, handler) = CreateGoogle(CompletedInteractionJson);
        var original = new CreateInteractionRequest { Model = "gemini-3-flash-preview", Input = "Write a long story" };
        var partial = new Interaction
        {
            Id = "v1_hop1", Status = InteractionStatuses.RequiresContinuation, ContinuationToken = "tok-1",
            Model = "gemini-3-flash-preview"
        };

        await client.ContinueAsync(partial, original, TestContext.Current.CancellationToken);

        var body = JsonNode.Parse(handler.Requests.Single().Body!)!;
        body["continuation_token"]!.GetValue<string>().ShouldBe("tok-1");
        body["previous_interaction_id"]!.GetValue<string>().ShouldBe("v1_hop1");
        body.AsObject().ContainsKey("input").ShouldBeFalse();
        original.ContinuationToken.ShouldBeNull(); // original request untouched

        var unstored = InteractionsClient.CreateContinuationRequest(partial,
            new CreateInteractionRequest { Model = "m", Input = "again", Store = false });
        unstored.Input!.Text.ShouldBe("again");
        unstored.PreviousInteractionId.ShouldBeNull();
        unstored.ContinuationToken.ShouldBe("tok-1");

        Should.Throw<ArgumentException>(() => InteractionsClient.CreateContinuationRequest(new Interaction()));
    }

    [Fact]
    public async Task LegacyOutputs_AreExposedAsModelOutputStep()
    {
        var (client, _) = CreateGoogle(
            """{"id":"v1_lyria","status":"completed","model":"lyria-3-clip-preview","outputs":[{"type":"audio","data":"AAE=","mime_type":"audio/mpeg"}]}""");

        var interaction = await client.GetAsync("v1_lyria", cancellationToken: TestContext.Current.CancellationToken);

        interaction.Steps.ShouldHaveSingleItem().ShouldBeOfType<InteractionModelOutputStep>();
        interaction.OutputAudio!.GetDataBytes().ShouldBe(new byte[] { 0, 1 });
        interaction.ExtensionData?.ContainsKey("outputs").ShouldNotBe(true);
    }

    [Fact]
    public void ResponseFormat_AcceptsSingleObjectOrArray()
    {
        var single = Deserialize("""{"id":"x","status":"completed","response_format":{"type":"text","mime_type":"application/json","schema":{"type":"object"}}}""");
        single.ResponseFormat!.ShouldHaveSingleItem().ShouldBeOfType<InteractionTextResponseFormat>().MimeType
            .ShouldBe("application/json");

        var list = Deserialize("""{"id":"x","status":"completed","response_format":[{"type":"text"},{"type":"audio","delivery":"inline"},{"anything":1}]}""");
        list.ResponseFormat!.Count.ShouldBe(3);
        list.ResponseFormat[1].ShouldBeOfType<InteractionAudioResponseFormat>().Delivery.ShouldBe("inline");
        list.ResponseFormat[2].ShouldBeOfType<UnknownInteractionResponseFormat>();

        var written = JsonNode.Parse(JsonSerializer.Serialize(list, InteractionsSerializerContext.Default.Interaction))!;
        written["response_format"]!.AsArray().Count.ShouldBe(3);
    }

    [Fact]
    public void GenAi_CreatesInteractionsClient()
    {
        var client = new GoogleAi("test-key").CreateInteractionsClient();
        client.ShouldNotBeNull();
        client.Platform.ShouldBeOfType<GoogleAIPlatformAdapter>();
    }
}
