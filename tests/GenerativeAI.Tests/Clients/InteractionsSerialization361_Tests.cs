using System.Text.Json;
using System.Text.Json.Nodes;
using GenerativeAI.Types;
using Shouldly;

namespace GenerativeAI.Tests.Clients;

/// <summary>
/// Serialization tests for the Interactions types added with python-genai 3.6.1 (media processing options, tool
/// search, environment mode, live interaction events and auth tokens).
/// </summary>
public class InteractionsSerialization361_Tests
{
    private static InteractionsSerializerContext Ctx => InteractionsSerializerContext.Default;

    private static JsonNode Write<T>(T value, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> info) =>
        JsonNode.Parse(JsonSerializer.Serialize(value, info))!;

    [Fact]
    public void MediaProcessing_StringAndObjectForms_RoundTrip()
    {
        const string json = """
            [{"type":"audio","uri":"gs://b/a.mp3","processing":"static"},
             {"type":"audio","uri":"gs://b/a.mp3","processing":{"type":"static","start_offset":"10.5s","end_offset":"30s"}},
             {"type":"document","uri":"gs://b/d.pdf","resolution":"high",
              "processing":{"type":"static","page_options":{"content_layers":["page_image","page_text"],"start_page":2,"end_page":5},
                            "spreadsheet_options":{"content_layers":["data","formulas"],"sheet_index":1,"visible_cells":"A1:D10"}}},
             {"type":"document","data":"AA==","mime_type":"application/pdf","processing":"static"}]
            """;

        var contents = JsonSerializer.Deserialize(json, Ctx.ListInteractionContent)!;

        var audioMode = contents[0].ShouldBeOfType<InteractionAudioContent>();
        audioMode.Processing!.Mode.ShouldBe(InteractionMediaProcessingModes.Static);
        audioMode.Processing.Options.ShouldBeNull();

        var audioSegment = contents[1].ShouldBeOfType<InteractionAudioContent>();
        audioSegment.Processing!.Options!.StartOffset.ShouldBe("10.5s");
        audioSegment.Processing.Options.EndOffset.ShouldBe("30s");

        var document = contents[2].ShouldBeOfType<InteractionDocumentContent>();
        document.Resolution.ShouldBe("high");
        var options = document.Processing!.Options!;
        options.PageOptions!.ContentLayers.ShouldBe(new[] { InteractionPageContentLayers.PageImage, InteractionPageContentLayers.PageText });
        options.PageOptions.StartPage.ShouldBe(2);
        options.PageOptions.EndPage.ShouldBe(5);
        options.SpreadsheetOptions!.ContentLayers.ShouldBe(new[] { InteractionSpreadsheetContentLayers.Data, InteractionSpreadsheetContentLayers.Formulas });
        options.SpreadsheetOptions.SheetIndex.ShouldBe(1);
        options.SpreadsheetOptions.VisibleCells.ShouldBe("A1:D10");

        contents[3].ShouldBeOfType<InteractionDocumentContent>().Processing!.Mode.ShouldBe("static");

        var written = Write(contents, Ctx.ListInteractionContent);
        JsonNode.DeepEquals(written, JsonNode.Parse(json)).ShouldBeTrue();
    }

    [Fact]
    public void MediaProcessing_BuiltWithImplicitConversions_SerializesAsWireShape()
    {
        var audio = new InteractionAudioContent
        {
            Uri = "gs://b/a.mp3",
            Processing = new InteractionAudioStaticMediaProcessing { StartOffset = "1s" }
        };
        var document = new InteractionDocumentContent { Uri = "gs://b/d.xlsx", Processing = "static" };
        var video = new InteractionVideoContent
        {
            Uri = "gs://b/v.mp4",
            Processing = new InteractionStaticMediaProcessing
            {
                Fps = 0.5,
                ContentLayers = new List<string> { InteractionVideoContentLayers.Frames, InteractionVideoContentLayers.Transcript }
            }.ToJsonNode()
        };

        var audioJson = Write<InteractionContent>(audio, Ctx.InteractionContent);
        audioJson["processing"]!["type"]!.GetValue<string>().ShouldBe("static");
        audioJson["processing"]!["start_offset"]!.GetValue<string>().ShouldBe("1s");

        Write<InteractionContent>(document, Ctx.InteractionContent)["processing"]!.GetValue<string>().ShouldBe("static");

        var videoJson = Write<InteractionContent>(video, Ctx.InteractionContent);
        videoJson["processing"]!["type"]!.GetValue<string>().ShouldBe("static");
        videoJson["processing"]!["fps"]!.GetValue<double>().ShouldBe(0.5);
        videoJson["processing"]!["content_layers"]![1]!.GetValue<string>().ShouldBe("transcript");
    }

    [Fact]
    public void Environment_ModeAndCustomConfig_RoundTrip()
    {
        var request = new CreateInteractionRequest
        {
            Agent = InteractionAgents.AntigravityPreview092026,
            Environment = new InteractionEnvironment
            {
                Mode = EnvironmentModes.SelfHosted,
                CustomConfig = new JsonObject { ["pool"] = "gpu" }
            }
        };

        var written = Write(request, Ctx.CreateInteractionRequest);
        written["environment"]!["mode"]!.GetValue<string>().ShouldBe("self_hosted");
        written["environment"]!["custom_config"]!["pool"]!.GetValue<string>().ShouldBe("gpu");

        var interaction = JsonSerializer.Deserialize(
            """{"id":"i","status":"idle","environment":{"type":"remote","mode":"on_demand","custom_config":{"k":1}}}""",
            Ctx.Interaction)!;
        interaction.Status.ShouldBe(InteractionStatuses.Idle);
        interaction.Environment!.Environment!.Mode.ShouldBe(EnvironmentModes.OnDemand);
        interaction.Environment.Environment.CustomConfig!["k"]!.GetValue<int>().ShouldBe(1);
    }

    [Fact]
    public void Tools_DeferLoadingAndToolSearch_RoundTrip()
    {
        const string json = """
            [{"type":"function","name":"get_weather","description":"Gets the weather","short_description":"weather","defer_loading":true,
              "parameters":{"type":"object"}},
             {"type":"mcp_server","name":"srv","url":"https://mcp.example.com","defer_loading":true},
             {"type":"tool_search","execution":"client","name":"search_tools","description":"Searches tools",
              "parameters":{"type":"object","properties":{"query":{"type":"string"}}}}]
            """;

        var tools = JsonSerializer.Deserialize(json, Ctx.ListInteractionTool)!;

        var function = tools[0].ShouldBeOfType<InteractionFunctionTool>();
        function.DeferLoading.ShouldBe(true);
        function.ShortDescription.ShouldBe("weather");
        tools[1].ShouldBeOfType<InteractionMcpServerTool>().DeferLoading.ShouldBe(true);
        var search = tools[2].ShouldBeOfType<InteractionToolSearchTool>();
        search.Type.ShouldBe(InteractionToolTypes.ToolSearch);
        search.Execution.ShouldBe(InteractionToolSearchExecutions.Client);
        search.Name.ShouldBe("search_tools");
        search.Parameters!["properties"]!["query"]!["type"]!.GetValue<string>().ShouldBe("string");

        JsonNode.DeepEquals(Write(tools, Ctx.ListInteractionTool), JsonNode.Parse(json)).ShouldBeTrue();

        var serverSearch = Write<InteractionTool>(
            new InteractionToolSearchTool { Execution = InteractionToolSearchExecutions.Server }, Ctx.InteractionTool);
        JsonNode.DeepEquals(serverSearch, JsonNode.Parse("""{"type":"tool_search","execution":"server"}""")).ShouldBeTrue();
    }

    [Fact]
    public void ToolSearchSteps_RoundTripInInteractionAndInput()
    {
        const string steps = """
            [{"type":"tool_search_call","id":"c1","arguments":{"function_names":["get_weather"]},"signature":"c2ln"},
             {"type":"tool_search_result","call_id":"c1","signature":"c2ln",
              "result":[{"type":"function","name":"get_weather","description":"Gets the weather","parameters":{"type":"object"}}]},
             {"type":"function_result","call_id":"c2","result":[{"type":"function","name":"lookup"},{"type":"text","text":"ok"}]}]
            """;

        var interaction = JsonSerializer.Deserialize($$"""{"id":"i","status":"completed","steps":{{steps}}}""", Ctx.Interaction)!;

        var call = interaction.Steps![0].ShouldBeOfType<InteractionToolSearchCallStep>();
        call.Id.ShouldBe("c1");
        call.Arguments!.FunctionNames.ShouldBe(new[] { "get_weather" });
        call.Signature.ShouldBe("c2ln");
        var result = interaction.Steps[1].ShouldBeOfType<InteractionToolSearchResultStep>();
        result.CallId.ShouldBe("c1");
        result.Result!.Single().Name.ShouldBe("get_weather");
        result.Result!.Single().Type.ShouldBe(InteractionToolTypes.Function);
        interaction.Steps[2].ShouldBeOfType<InteractionFunctionResultStep>().Result![0]!["type"]!.GetValue<string>().ShouldBe("function");

        var written = Write(interaction, Ctx.Interaction);
        JsonNode.DeepEquals(written["steps"], JsonNode.Parse(steps)).ShouldBeTrue();

        // A list of steps given as input is detected as steps (not contents) and round-trips.
        var request = JsonSerializer.Deserialize($$"""{"model":"m","input":{{steps}}}""", Ctx.CreateInteractionRequest)!;
        request.Input!.Steps!.Count.ShouldBe(3);
        request.Input.Steps[0].ShouldBeOfType<InteractionToolSearchCallStep>();
        JsonNode.DeepEquals(Write(request, Ctx.CreateInteractionRequest)["input"], JsonNode.Parse(steps)).ShouldBeTrue();
    }

    [Fact]
    public void ToolSearchDeltas_AreReadFromStepDeltaEvents()
    {
        var callEvent = JsonSerializer.Deserialize(
            """{"event_type":"step.delta","index":0,"delta":{"type":"tool_search_call","arguments":{"function_names":["a","b"]},"signature":"cw=="}}""",
            Ctx.InteractionSseEvent).ShouldBeOfType<InteractionStepDeltaEvent>();
        var callDelta = callEvent.Delta.ShouldBeOfType<InteractionToolSearchCallDelta>();
        callDelta.Arguments!.FunctionNames.ShouldBe(new[] { "a", "b" });
        callDelta.Signature.ShouldBe("cw==");

        const string resultJson = """{"event_type":"step.delta","index":1,"delta":{"type":"tool_search_result","result":[{"type":"function","name":"a"}]}}""";
        var resultEvent = JsonSerializer.Deserialize(resultJson, Ctx.InteractionSseEvent).ShouldBeOfType<InteractionStepDeltaEvent>();
        resultEvent.Delta.ShouldBeOfType<InteractionToolSearchResultDelta>().Result!.Single().Name.ShouldBe("a");
        JsonNode.DeepEquals(Write<InteractionSseEvent>(resultEvent, Ctx.InteractionSseEvent), JsonNode.Parse(resultJson)).ShouldBeTrue();
    }

    [Fact]
    public void ServerEvents_LiveAndStreamingVariants_AreDistinguished()
    {
        const string frames = """
            [{"event_type":"interaction.created","interaction_id":"int-1"},
             {"event_type":"interaction.connected"},
             {"event_type":"step.start","index":0,"step":{"type":"model_output"}},
             {"event_type":"step.delta","index":0,"delta":{"type":"text","text":"Hi"}},
             {"event_type":"step.stop","index":0},
             {"event_type":"interaction.cancelled","usage":{"total_tokens":12}},
             {"event_type":"interaction.completed","created":"2026-11-24T10:00:00Z","updated":"2026-11-24T10:00:05Z",
              "status":{"code":0,"message":"ok"},"usage":{"total_tokens":12}},
             {"event_type":"error","error":{"code":"x","message":"boom"}},
             {"event_type":"interaction.created","interaction":{"id":"sse-1","status":"in_progress"}},
             {"event_type":"interaction.completed","interaction":{"id":"sse-1","status":"completed"}},
             {"event_type":"interaction.future","x":1}]
            """;

        var events = JsonNode.Parse(frames)!.AsArray()
            .Select(frame => frame.Deserialize(Ctx.InteractionSseEvent)!)
            .ToList();

        events[0].ShouldBeOfType<LiveInteractionCreatedEvent>().InteractionId.ShouldBe("int-1");
        events[1].ShouldBeOfType<InteractionConnectedEvent>().EventType.ShouldBe(InteractionEventTypes.InteractionConnected);
        events[2].ShouldBeOfType<InteractionStepStartEvent>();
        events[3].ShouldBeOfType<InteractionStepDeltaEvent>().Delta.ShouldBeOfType<InteractionTextDelta>().Text.ShouldBe("Hi");
        events[4].ShouldBeOfType<InteractionStepStopEvent>();
        events[5].ShouldBeOfType<InteractionCancelledEvent>().Usage!.TotalTokens.ShouldBe(12);
        var completed = events[6].ShouldBeOfType<LiveInteractionCompletedEvent>();
        completed.Created.ShouldBe("2026-11-24T10:00:00Z");
        completed.Updated.ShouldBe("2026-11-24T10:00:05Z");
        completed.Status!.Message.ShouldBe("ok");
        completed.Usage!.TotalTokens.ShouldBe(12);
        events[7].ShouldBeOfType<InteractionErrorEvent>().Error!.Message.ShouldBe("boom");
        events[8].ShouldBeOfType<InteractionCreatedEvent>().Interaction!.Id.ShouldBe("sse-1");
        events[9].ShouldBeOfType<InteractionCompletedEvent>().Interaction!.Status.ShouldBe("completed");
        events[10].ShouldBeOfType<UnknownInteractionSseEvent>();

        var expected = JsonNode.Parse(frames)!.AsArray();
        for (var i = 0; i < events.Count; i++)
            JsonNode.DeepEquals(Write(events[i], Ctx.InteractionSseEvent), expected[i]).ShouldBeTrue($"event {i}");
    }

    [Fact]
    public void ClientEvents_SerializeToWireShape()
    {
        var events = new List<InteractionClientEvent>
        {
            new CreateInteractionEvent
            {
                Model = InteractionModels.Gemini4Neon,
                SystemInstruction = "Be brief.",
                Store = false,
                PreviousInteractionId = "prev",
                Tools = new List<InteractionTool> { new InteractionToolSearchTool() }
            },
            new ConnectInteractionEvent { Id = "int-1", Background = true },
            InteractionInputEvent.FromText("Hello"),
            new InteractionUserActionInput { Action = InteractionUserActionTypes.KeyDown, Input = InteractionUserActionKeys.KeyW },
            new InteractionWorldSetupEvent
            {
                WorldPrompt = "A misty forest",
                ReferenceImage = new InteractionWorldReferenceImage { Uri = "gs://b/i.png", MimeType = "image/png" }
            },
            new InteractionWorldSetupEvent { WorldPrompt = new InteractionWorldPromptContent { Text = "A desert" } },
            new CompleteInteractionEvent(),
            new CancelInteractionEvent(),
            new InteractionDisconnectEvent()
        };

        const string expected = """
            {"events":[
              {"event_type":"interaction.create","model":"gemini-4-neon","previous_interaction_id":"prev","store":false,
               "system_instruction":"Be brief.","tools":[{"type":"tool_search"}]},
              {"event_type":"interaction.connect","id":"int-1","background":true},
              {"event_type":"input","content":[{"type":"text","text":"Hello"}]},
              {"event_type":"user_action","action":"key_down","input":"key_w"},
              {"event_type":"world_setup","reference_image":{"mime_type":"image/png","uri":"gs://b/i.png"},"world_prompt":"A misty forest"},
              {"event_type":"world_setup","world_prompt":{"text":"A desert"}},
              {"event_type":"interaction.complete"},
              {"event_type":"interaction.cancel"},
              {"event_type":"interaction.disconnect"}]}
            """;

        var batch = InteractionClientEventBatch.FromEvents(events);
        var written = Write(batch, Ctx.InteractionClientEventBatch);
        JsonNode.DeepEquals(written, JsonNode.Parse(expected)).ShouldBeTrue(written.ToJsonString());

        InteractionClientEventBatch single = new CompleteInteractionEvent();
        JsonNode.DeepEquals(Write(single, Ctx.InteractionClientEventBatch),
            JsonNode.Parse("""{"event_type":"interaction.complete"}""")).ShouldBeTrue();
    }

    [Fact]
    public void ClientEvents_ReadPolymorphically_AndKeepUnknownEvents()
    {
        const string json = """
            {"events":[
              {"event_type":"interaction.create","model":"m","future_field":1},
              {"event_type":"input","content":[{"type":"image","uri":"gs://b/i.png","mime_type":"image/png"}]},
              {"event_type":"world_setup","world_prompt":{"text":"A desert"}},
              {"event_type":"world_setup","world_prompt":"A forest"},
              {"event_type":"user_action","action":"key_click","input":"space"},
              {"event_type":"interaction.teleport","where":"moon"}]}
            """;

        var batch = JsonSerializer.Deserialize(json, Ctx.InteractionClientEventBatch)!;
        batch.Event.ShouldBeNull();
        var events = batch.EventList!.Events!;

        var create = events[0].ShouldBeOfType<CreateInteractionEvent>();
        create.Model.ShouldBe("m");
        create.ExtensionData!["future_field"].GetInt32().ShouldBe(1);
        events[1].ShouldBeOfType<InteractionInputEvent>().Content!.Single().ShouldBeOfType<InteractionImageContent>();
        events[2].ShouldBeOfType<InteractionWorldSetupEvent>().WorldPrompt!.Content!.Text.ShouldBe("A desert");
        events[3].ShouldBeOfType<InteractionWorldSetupEvent>().WorldPrompt!.Text.ShouldBe("A forest");
        var action = events[4].ShouldBeOfType<InteractionUserActionInput>();
        action.Action.ShouldBe(InteractionUserActionTypes.KeyClick);
        action.Input.ShouldBe(InteractionUserActionKeys.Space);
        events[5].ShouldBeOfType<UnknownInteractionClientEvent>().EventType.ShouldBe("interaction.teleport");

        JsonNode.DeepEquals(Write(batch, Ctx.InteractionClientEventBatch), JsonNode.Parse(json)).ShouldBeTrue();

        var single = JsonSerializer.Deserialize("""{"event_type":"interaction.cancel"}""", Ctx.InteractionClientEventBatch)!;
        single.EventList.ShouldBeNull();
        single.Event.ShouldBeOfType<CancelInteractionEvent>();
    }

    [Fact]
    public void AuthToken_ReadsTimestamps_AndCreateParamsSerialize()
    {
        var token = JsonSerializer.Deserialize(
            """{"auth_token":"tok","expires_at":"2026-11-24T10:30:00.123456789Z"}""", Ctx.InteractionAuthToken)!;
        token.AuthToken.ShouldBe("tok");
        token.ExpiresAt.ShouldNotBeNull();
        token.ExpiresAt!.Value.UtcDateTime.ShouldBe(new DateTime(2026, 11, 24, 10, 30, 0, DateTimeKind.Utc).AddTicks(1234567));

        var parameters = new InteractionAuthTokenCreateParams
        {
            ExpireTime = new DateTimeOffset(2026, 11, 24, 11, 0, 0, TimeSpan.Zero)
        };
        var written = Write(parameters, Ctx.InteractionAuthTokenCreateParams);
        DateTimeOffset.Parse(written["expire_time"]!.GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture)
            .ShouldBe(parameters.ExpireTime.Value);
    }
}
